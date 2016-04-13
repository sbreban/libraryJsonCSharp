using System;
using System.Collections.Generic;
using System.Windows.Forms;
using model;
using networking;
using networking.dto;
using services;

namespace client
{
    public partial class LibraryWindow : Form
    {
        private LibraryClientController controller;
        private IList<Book> availableBooks;
        private IList<Book> userBooks;

        public LibraryWindow(LibraryClientController controller)
        {
            InitializeComponent();
            this.controller = controller;

            setAvailableBooks();
            setUserBooks();

            controller.updateEvent += userUpdate;
        }

        private void ChatWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("LibraryWindow closing " + e.CloseReason);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                controller.logout();
                controller.updateEvent -= userUpdate;
                Application.Exit();
            }
        }

        public void setAvailableBooks()
        {
            this.availableBooks = controller.getAvailableBooks();
            availableBooksList.DataSource = availableBooks;
        }

        public void setUserBooks()
        {
            this.userBooks = controller.getUserBooks();
            userBooksList.DataSource = userBooks;
        }

        public void userUpdate(object sender, LibraryUserEventArgs e)
        {
            if (e.UserEventType == LibraryUserEvent.BookBorrowed)
            {
                BookBorrowedDTO bookBorrowedDto = (BookBorrowedDTO)e.Data;
                Book updated = null;
                foreach (Book availableBook in availableBooks)
                {
                    if (availableBook.Id == bookBorrowedDto.BookId)
                    {
                        updated = availableBook;
                    }
                }
                if (bookBorrowedDto.NewQuantity == 0)
                {
                    availableBooks.Remove(updated);
                }
                else
                {
                    updated.Available = bookBorrowedDto.NewQuantity;
                }

                availableBooksList.BeginInvoke(new UpdateListBoxCallback(this.updateListBox),
                    new Object[] { availableBooksList, availableBooks });

                if (bookBorrowedDto.ByThisUser)
                {
                    userBooks.Add(updated);

                    userBooksList.BeginInvoke(new UpdateListBoxCallback(this.updateListBox),
                        new Object[] {userBooksList, userBooks});
                }
            }

            if (e.UserEventType == LibraryUserEvent.BookReturned)
            {
                BookReturnedDTO bookReturnedDto = (BookReturnedDTO)e.Data;

                Book returnedAvailable = null;
                foreach (Book availableBook in availableBooks)
                {
                    if (availableBook.Id == bookReturnedDto.Id)
                    {
                        returnedAvailable = availableBook;
                    }
                }
                if (returnedAvailable == null)
                {
                    returnedAvailable = new Book(bookReturnedDto.Id, bookReturnedDto.Author, bookReturnedDto.Title, 1);
                    availableBooks.Add(returnedAvailable);
                }
                else
                {
                    returnedAvailable.Available++;
                }

                availableBooksList.BeginInvoke(new UpdateListBoxCallback(this.updateListBox),
                    new Object[] { availableBooksList, availableBooks });

                if (bookReturnedDto.ByThisUser)
                {
                    Book returnedUser = null;
                    foreach (Book userBook in userBooks)
                    {
                        if (userBook.Id == bookReturnedDto.Id)
                        {
                            returnedUser = userBook;
                        }
                    }
                    userBooks.Remove(returnedUser);

                    userBooksList.BeginInvoke(new UpdateListBoxCallback(this.updateListBox),
                        new Object[] {userBooksList, userBooks});
                }
            }
        }

        private void updateListBox(ListBox listBox, IList<Book> newData)
        {
            listBox.DataSource = null;
            listBox.DataSource = newData;
        }

        public delegate void UpdateListBoxCallback(ListBox list, IList<Book> data);

        private void borrowButton_Click(object sender, EventArgs e)
        {
            int alreadyBorrowed = userBooksList.Items.Count;
            if (alreadyBorrowed >= 3)
            {
                MessageBox.Show("Already borrowed 3 books!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Book selectedBook = (Book)availableBooksList.SelectedItem;
                bool alreadyHave = false;
                foreach (Book yourBook in userBooksList.Items)
                {
                    if (yourBook.Id == selectedBook.Id)
                    {
                        MessageBox.Show("You already have this book!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        alreadyHave = true;
                    }
                }
                if (!alreadyHave)
                {
                    try
                    {
                        controller.borrowBook(selectedBook.Id);
                    }
                    catch (LibraryException exception)
                    {
                        MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<Book> foundBooks = controller.searchBooks(searchTextBox.Text);

                availableBooks = foundBooks;
                availableBooksList.DataSource = null;
                availableBooksList.DataSource = availableBooks;
            }
            catch (LibraryException exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            try
            {
                searchTextBox.Text = "";
                availableBooks = controller.getAvailableBooks();
                availableBooksList.DataSource = null;
                availableBooksList.DataSource = availableBooks;
            }
            catch (LibraryException exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}

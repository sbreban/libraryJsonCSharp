using System;

namespace networking.dto
{
    [Serializable]
    public class BookBorrowedDTO
    {
        private int bookId;
        private int newQuantity;
        private bool byThisUser;

        public BookBorrowedDTO(int bookId, int newQuantity, bool byThisUser)
        {
            this.bookId = bookId;
            this.newQuantity = newQuantity;
            this.byThisUser = byThisUser;
        }

        public int BookId
        {
            get { return bookId; }
            set { bookId = value; }
        }

        public int NewQuantity
        {
            get { return newQuantity; }
            set { newQuantity = value; }
        }

        public bool ByThisUser
        {
            get { return byThisUser; }
            set { byThisUser = value; }
        }
    }
}

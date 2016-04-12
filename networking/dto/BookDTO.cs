using System;

namespace networking.dto
{
    [Serializable]
    public class BookDTO
    {
        private int id;
        private String author, title;

        public BookDTO(int id, string author, string title)
        {
            this.id = id;
            this.author = author;
            this.title = title;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
    }
}

using System;

namespace networking.dto
{
    [Serializable]
    public class BookReturnedDTO
    {
        private int id;
        private String author, title;
        private bool byThisUser;

        public BookReturnedDTO(int id, string author, string title, bool byThisUser)
        {
            this.id = id;
            this.author = author;
            this.title = title;
            this.byThisUser = byThisUser;
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

        public bool ByThisUser
        {
            get { return byThisUser; }
            set { byThisUser = value; }
        }
    }
}

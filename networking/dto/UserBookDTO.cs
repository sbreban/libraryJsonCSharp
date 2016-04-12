using System;

namespace networking.dto
{
    [Serializable]
    public class UserBookDTO
    {
        private int userId;
        private int bookId;

        public UserBookDTO(int userId, int bookId)
        {
            this.userId = userId;
            this.bookId = bookId;
        }

        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public int BookId
        {
            get { return bookId; }
            set { bookId = value; }
        }
    }
}

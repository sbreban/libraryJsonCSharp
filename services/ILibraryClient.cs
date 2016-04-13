using System;

namespace services
{
    public interface ILibraryClient
    {
        void bookUpdated(int bookId, int newQuantity, bool byThisUser);
        void bookReturned(int bookId, String author, String title, bool byThisUser);
    }
}

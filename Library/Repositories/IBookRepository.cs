using System.Collections.Generic;
using Library.Books;

namespace Library.Activities
{
    interface IBookRepository
    {
        bool AddBookToCatalog(string title, string author, string isbn);
        bool RemoveBookFromCatalog(BookRecord bookToRemove);
        bool BorrowBook(BookRecord bookToBorrow);
        BookRecord GetBook(string isbn);
        BookRecord GetBook(string title, string author);
        List<BookRecord> GetBooksNotBorrowedForWeeks(int weeks);
        List<BorrowerRecord> GetCurrentBorrowers();
    }
}
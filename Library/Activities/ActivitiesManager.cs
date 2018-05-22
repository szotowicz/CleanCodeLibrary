using Library.Books;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Activities
{
    class ActivitiesManager : IActivitiesManager
    {
        private readonly IBookRepository _bookRepository;

        private readonly Dictionary<AvailableActivities, string> _descriptionOfActivities = new Dictionary<AvailableActivities, string>
        {
            [AvailableActivities.AddBookToCatalog] = "Add book to the catalog",
            [AvailableActivities.RemoveBookFromCatalog] = "Remove book from the catalog",
            [AvailableActivities.SearchBookByParameter] = "Search book by title, author or ISBN",
            [AvailableActivities.SearchBooksNotBorrowedForWeeks] = "Search for books that have not been borrowed for the last weeks",
            [AvailableActivities.BorrowBook] = "Borrow a book",
            [AvailableActivities.GetCurrentBorrowers] = "Display a list of people who currently have borrowed any book"
        };

        public ActivitiesManager(string bookCatalogPath)
        {
            switch (Path.GetExtension(bookCatalogPath))
            {
                case ".xml":
                    _bookRepository = new BookRepositoryXML(bookCatalogPath);
                    break;
                case ".json":
                    _bookRepository = new BookRepositoryJSON(bookCatalogPath);
                    break;
                default:
                    throw new Exception("[Error] The specified book catalog has an invalid extension. Available formats: .xml or .json");
            }
        }

        public IEnumerable<string> GetAvailableActivities()
        {
            foreach (var activity in _descriptionOfActivities)
            {
                yield return activity.Value;
            }
        }

        public string PerformAction(int activityId)
        {
            AvailableActivities selectedActivity = _descriptionOfActivities.Keys.ElementAt(activityId);
            switch (selectedActivity)
            {
                case AvailableActivities.AddBookToCatalog:
                    Console.Write("Title: ");
                    string title = Console.ReadLine().Trim();
                    Console.Write("Author: ");
                    string author = Console.ReadLine().Trim();
                    Console.Write("ISBN: ");
                    string isbn = Console.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
                    {
                        return "Incorrect data provided. Try again.";
                    }

                    try
                    {
                        if (_bookRepository.AddBookToCatalog(title, author, isbn))
                        {
                            return $"Book \"{title}\" has been added to the catalog.";
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        return $"[Warning] Book \"{title}\" has not been added to the catalog. Try again.";
                    }

                case AvailableActivities.RemoveBookFromCatalog:
                    try
                    {
                        BookRecord bookToRemove = GetBookRecord();
                        if (bookToRemove != null)
                        {
                            if (_bookRepository.RemoveBookFromCatalog(bookToRemove))
                            {
                                return "Book has been removed from catalog correctly.";
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            return "[Warning] The book was not found. Try again.";
                        }
                    }
                    catch (Exception)
                    {
                        return $"[Warning] Book has not been removed from catalog. Try again.";
                    }

                case AvailableActivities.SearchBookByParameter:
                    try
                    {
                        BookRecord bookRecord = GetBookRecord();
                        if (bookRecord != null)
                        {
                            string response = $"The book was found. Title: \"{bookRecord.Book.Title}\", author: {bookRecord.Book.Author}, ISBN: {bookRecord.Book.ISBN}.";
                            if (bookRecord.LastBorrowDate == DateTime.MinValue || bookRecord.LastBorrowDate == null)
                            {
                                response += "\n(not borrowed yet)";
                            }
                            else
                            {
                                response += $"\nBorrowed by {bookRecord.LastBorrower.Name} {bookRecord.LastBorrower.LastName} (Date: {bookRecord.LastBorrowDate.ToShortDateString()})";
                            }
                            return response;

                        }
                    }
                    catch (Exception)
                    { }
                    return "[Warning] The book was not found. Try again.";

                case AvailableActivities.SearchBooksNotBorrowedForWeeks:
                    try
                    {
                        Console.Write("Number of weeks: ");
                        int weeks = int.Parse(Console.ReadLine().Trim());

                        try
                        {
                            List<BookRecord> booksNotBorrowed = _bookRepository.GetBooksNotBorrowedForWeeks(weeks);
                            foreach (BookRecord bookNotBorrowed in booksNotBorrowed)
                            {
                                Console.WriteLine($"\nBook: \"{bookNotBorrowed.Book.Title}\" {bookNotBorrowed.Book.Author} ISBN: {bookNotBorrowed.Book.ISBN}.");
                                if (bookNotBorrowed.LastBorrowDate == DateTime.MinValue || bookNotBorrowed.LastBorrowDate == null)
                                {
                                    Console.WriteLine("(not borrowed yet)");
                                }
                                else
                                {
                                    Console.WriteLine($"Borrowed by {bookNotBorrowed.LastBorrower.Name} {bookNotBorrowed.LastBorrower.LastName} (Date: {bookNotBorrowed.LastBorrowDate.ToShortDateString()})");
                                }
                            }
                            return $"{booksNotBorrowed.Count} records were found correctly.";
                        }
                        catch (Exception)
                        {
                            return "[Warning] There was a problem downloading the data. Try again.";
                        }
                    }
                    catch (Exception)
                    {
                        return "[Warning] Incorrect data provided. Try again.";
                    }

                case AvailableActivities.BorrowBook:
                    try
                    {
                        Console.Write("Borrower name: ");
                        string borrowerName = Console.ReadLine().Trim();
                        Console.Write("Borrower last name: ");
                        string borrowerLastName = Console.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(borrowerName) || string.IsNullOrWhiteSpace(borrowerLastName))
                        {
                            return "Incorrect data provided. Try again.";
                        }

                        Borrower borrower = new Borrower()
                        {
                            Name = borrowerName,
                            LastName = borrowerLastName
                        };

                        BookRecord bookToBorrow = GetBookRecord();
                        if (bookToBorrow != null)
                        {

                            bookToBorrow.LastBorrower = borrower;
                            bookToBorrow.LastBorrowDate = DateTime.Now;

                            if (_bookRepository.BorrowBook(bookToBorrow))
                            {
                                return "Book has been borrowed correctly.";
                            }
                            else
                            {
                                return "[Warning] Book has not been borrowed. Try again.";
                            }
                        }
                        else
                        {
                            return "[Warning] The book was not found. Try again.";
                        }
                    }
                    catch (Exception)
                    {
                        return $"[Warning] Book has not been borrowed. Try again.";
                    }

                case AvailableActivities.GetCurrentBorrowers:
                    try
                    {
                        List<BorrowerRecord> borrowers = _bookRepository.GetCurrentBorrowers();
                        foreach (BorrowerRecord borrower in borrowers)
                        {
                            Console.WriteLine($"{borrower.LastBorrower.Name} {borrower.LastBorrower.LastName}: {borrower.NumberOfBooksBorrowed} books");
                        }
                        return $"{borrowers.Count} records were found correctly.";
                    }
                    catch (Exception)
                    {
                        return "[Warning] There was a problem downloading the data. Try again.";
                    }
            }
            return "[Error] Action for selected activity is not defined.";
        }

        private BookRecord GetBookRecord()
        {
            BookRecord bookRecord = null;

            Console.WriteLine("Find book by:\n 1: Title and author\n 2: ISBN");
            Console.Write("Command: ");
            switch (Console.ReadLine().Trim())
            {
                case "1":
                    Console.Write("Title: ");
                    string title = Console.ReadLine().Trim();
                    Console.Write("Author: ");
                    string author = Console.ReadLine().Trim();


                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                    {
                        Console.WriteLine("Incorrect data provided. Try again.");
                        break;
                    }

                    bookRecord = _bookRepository.GetBook(title, author);
                    break;

                case "2":
                    Console.Write("ISBN: ");
                    string isbn = Console.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(isbn))
                    {
                        Console.WriteLine("Incorrect data provided. Try again.");
                        break;
                    }

                    bookRecord = _bookRepository.GetBook(isbn);
                    break;

                default:
                    Console.WriteLine("[Warning] The parameter was not recognized");
                    break;
            }

            return bookRecord;
        }
    }
}
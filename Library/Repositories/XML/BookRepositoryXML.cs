using Library.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Library.Activities
{
    class BookRepositoryXML : IBookRepository
    {
        private readonly string _bookCatalogPath;

        public BookRepositoryXML(string bookCatalogPath)
        {
            _bookCatalogPath = bookCatalogPath;
        }

        public bool AddBookToCatalog(string title, string author, string isbn)
        {
            try
            {
                XDocument doc = XDocument.Load(_bookCatalogPath);

                XElement emptyBorrower = new XElement(ConstantsBookRepository.BORROWER_OBJECT);
                emptyBorrower.Add(new XElement(ConstantsBookRepository.BORROWER_NAME, ""));
                emptyBorrower.Add(new XElement(ConstantsBookRepository.BORROWER_LAST_NAME, ""));

                XElement newBook = new XElement(ConstantsBookRepository.BOOK_OBJECT);
                newBook.Add(new XElement(ConstantsBookRepository.BOOK_TITLE, title));
                newBook.Add(new XElement(ConstantsBookRepository.BOOK_AUTHOR, author));
                newBook.Add(new XElement(ConstantsBookRepository.BOOK_ISBN, isbn));
                newBook.Add(new XElement(ConstantsBookRepository.BOOK_LAST_BORROW, DateTime.MinValue.ToShortDateString()));
                newBook.Add(emptyBorrower);

                doc.Element(ConstantsBookRepository.MAIN_OBJECT_IN_REPOSITORY).Add(newBook);
                doc.Save(_bookCatalogPath);

                return true;
            }
            catch (Exception)
            { }

            return false;
        }

        public bool RemoveBookFromCatalog(BookRecord bookToRemove)
        {
            try
            {
                XDocument doc = XDocument.Load(_bookCatalogPath);
                doc.Descendants(ConstantsBookRepository.BOOK_OBJECT)
                    .FirstOrDefault(e => e.Element(ConstantsBookRepository.BOOK_ISBN).Value == bookToRemove.Book.ISBN).Remove();
                doc.Save(_bookCatalogPath);

                return true;
            }
            catch (Exception)
            { }

            return false;
        }

        public BookRecord GetBook(string isbn)
        {
            BookRecord bookRecord = null;

            try
            {
                XElement doc = XElement.Load(_bookCatalogPath);
                var targetBook = doc.Elements(ConstantsBookRepository.BOOK_OBJECT)
                    .FirstOrDefault(e => e.Element(ConstantsBookRepository.BOOK_ISBN).Value == isbn);

                if (targetBook != null)
                {
                    bookRecord = GetSpecificBook(targetBook);
                }
            }
            catch (Exception)
            { }

            return bookRecord;
        }

        public BookRecord GetBook(string title, string author)
        {
            BookRecord bookRecord = null;

            try
            {
                XElement doc = XElement.Load(_bookCatalogPath);
                XElement targetBook = doc.Elements(ConstantsBookRepository.BOOK_OBJECT)
                    .FirstOrDefault(e => e.Element(ConstantsBookRepository.BOOK_TITLE).Value == title
                    && e.Element(ConstantsBookRepository.BOOK_AUTHOR).Value == author);

                if (targetBook != null)
                {
                    bookRecord = GetSpecificBook(targetBook);
                }
            }
            catch (Exception)
            { }

            return bookRecord;
        }

        public List<BookRecord> GetBooksNotBorrowedForWeeks(int weeks)
        {
            List<BookRecord> booksNotBorrowedForWeeks = new List<BookRecord>();

            try
            {
                XElement doc = XElement.Load(_bookCatalogPath);
                var allBooks = doc.Elements(ConstantsBookRepository.BOOK_OBJECT).ToList();

                if (allBooks != null)
                {
                    foreach (XElement book in allBooks)
                    {
                        BookRecord newBookRecord = null;

                        DateTime lastBorrow = DateTime.MinValue;
                        string lastBorrowValue = book.Element(ConstantsBookRepository.BOOK_LAST_BORROW).Value;
                        if (lastBorrowValue != "" && lastBorrowValue != "01.01.0001")
                        {
                            if (!DateTime.TryParse(lastBorrowValue, out lastBorrow))
                            {
                                continue;
                            }

                            if (DateTime.Now.Subtract(lastBorrow).Days <= weeks * 7)
                            {
                                continue;
                            }
                        }

                        newBookRecord = new BookRecord()
                        {
                            Book = new Book()
                            {
                                Title = book.Element(ConstantsBookRepository.BOOK_TITLE).Value,
                                Author = book.Element(ConstantsBookRepository.BOOK_AUTHOR).Value,
                                ISBN = book.Element(ConstantsBookRepository.BOOK_ISBN).Value
                            },
                            LastBorrowDate = lastBorrow,
                            LastBorrower = new Borrower()
                            {
                                Name = book.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_NAME).Value,
                                LastName = book.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_LAST_NAME).Value,
                            }
                        };

                        booksNotBorrowedForWeeks.Add(newBookRecord);
                    }
                }
            }
            catch (Exception)
            { }

            return booksNotBorrowedForWeeks;
        }

        public bool BorrowBook(BookRecord bookToBorrow)
        {
            try
            {
                XDocument doc = XDocument.Load(_bookCatalogPath);

                doc.Descendants(ConstantsBookRepository.BOOK_OBJECT)
                    .FirstOrDefault(e => e.Element(ConstantsBookRepository.BOOK_ISBN).Value == bookToBorrow.Book.ISBN)
                    .SetElementValue(ConstantsBookRepository.BOOK_LAST_BORROW, DateTime.Now.ToShortDateString());

                doc.Save(_bookCatalogPath);

                return true;
            }
            catch (Exception)
            { }

            return false;
        }

        public List<BorrowerRecord> GetCurrentBorrowers()
        {
            List<BorrowerRecord> currentBorrowers = new List<BorrowerRecord>();

            try
            {
                XElement doc = XElement.Load(_bookCatalogPath);
                var borrowedBooks = doc.Elements(ConstantsBookRepository.BOOK_OBJECT)
                    .Where(e => e.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_NAME).Value != "").ToList();

                if (borrowedBooks != null)
                {
                    foreach (XElement borrowedBook in borrowedBooks)
                    {
                        BorrowerRecord borrowerRecord = currentBorrowers
                            .FirstOrDefault(b => b.LastBorrower.Name == borrowedBook.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_NAME).Value
                            && b.LastBorrower.LastName == borrowedBook.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_LAST_NAME).Value);

                        if (borrowerRecord != null)
                        {
                            borrowerRecord.NumberOfBooksBorrowed += 1;
                        }
                        else
                        {
                            currentBorrowers.Add(new BorrowerRecord()
                            {
                                LastBorrower = new Borrower()
                                {
                                    Name = borrowedBook.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_NAME).Value,
                                    LastName = borrowedBook.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_LAST_NAME).Value
                                },
                                NumberOfBooksBorrowed = 1
                            });
                        }
                    }
                }
            }
            catch (Exception)
            { }

            return currentBorrowers;
        }

        private BookRecord GetSpecificBook(XElement targetBook)
        {
            DateTime lastBorrow = DateTime.MinValue;
            DateTime.TryParse(targetBook.Element(ConstantsBookRepository.BOOK_LAST_BORROW).Value, out lastBorrow);

            BookRecord bookRecord = new BookRecord()
            {
                Book = new Book()
                {
                    Title = targetBook.Element(ConstantsBookRepository.BOOK_TITLE).Value,
                    Author = targetBook.Element(ConstantsBookRepository.BOOK_AUTHOR).Value,
                    ISBN = targetBook.Element(ConstantsBookRepository.BOOK_ISBN).Value
                },
                LastBorrowDate = lastBorrow,
                LastBorrower = new Borrower()
                {
                    Name = targetBook.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_NAME).Value,
                    LastName = targetBook.Element(ConstantsBookRepository.BORROWER_OBJECT).Element(ConstantsBookRepository.BORROWER_LAST_NAME).Value,
                }
            };

            return bookRecord;
        }
    }
}
using Library.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Library.Activities
{
    class BookRepositoryJSON : IBookRepository
    {
        private readonly string _bookCatalogPath;

        public BookRepositoryJSON(string bookCatalogPath)
        {
            _bookCatalogPath = bookCatalogPath;
        }

        public bool AddBookToCatalog(string title, string author, string isbn)
        {
            try
            {
                var allBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath));

                Library newBook = new Library()
                {
                    Title = title,
                    Author = author,
                    Isbn = isbn,
                    LastBorrowDate = DateTime.MinValue.ToShortDateString(),
                    LastBorrower = new LastBorrower()
                    {
                        Name = "",
                        Lastname = ""
                    }
                };

                allBooks.Add(newBook);
                File.WriteAllText(_bookCatalogPath, JsonConvert.SerializeObject(allBooks, Formatting.Indented));

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
                var allBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath));

                Library toRemove = allBooks.FirstOrDefault(b => b.Isbn == bookToRemove.Book.ISBN);

                if (toRemove != null)
                {
                    allBooks.Remove(toRemove);
                    File.WriteAllText(_bookCatalogPath, JsonConvert.SerializeObject(allBooks, Formatting.Indented));

                    return true;
                }
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
                List<Library> allBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath));

                Library targetBook = null;
                if (allBooks.Count > 0)
                {
                    targetBook = allBooks.FirstOrDefault(b => b.Isbn == isbn);
                }

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
                List<Library> allBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath));

                Library targetBook = null;
                if (allBooks.Count > 0)
                {
                    targetBook = allBooks.FirstOrDefault(b => b.Title == title && b.Author == author);
                }

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
                var allBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath));

                if (allBooks.Count > 0)
                {
                    foreach (Library book in allBooks)
                    {
                        BookRecord newBookRecord = null;

                        DateTime lastBorrow = DateTime.MinValue;
                        string lastBorrowValue = book.LastBorrowDate;
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
                                Title = book.Title,
                                Author = book.Author,
                                ISBN = book.Isbn
                            },
                            LastBorrowDate = lastBorrow,
                            LastBorrower = new Borrower()
                            {
                                Name = book.LastBorrower.Name,
                                LastName = book.LastBorrower.Lastname,
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
                var allBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath));
                
                if (allBooks.Count > 0)
                {
                    allBooks.FirstOrDefault(b => b.Isbn == bookToBorrow.Book.ISBN).LastBorrowDate = DateTime.Now.ToShortDateString();
                    allBooks.FirstOrDefault(b => b.Isbn == bookToBorrow.Book.ISBN).LastBorrower.Name = bookToBorrow.LastBorrower.Name;
                    allBooks.FirstOrDefault(b => b.Isbn == bookToBorrow.Book.ISBN).LastBorrower.Lastname = bookToBorrow.LastBorrower.LastName;

                    File.WriteAllText(_bookCatalogPath, JsonConvert.SerializeObject(allBooks, Formatting.Indented));

                    return true;
                }
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
                List<Library> borrowedBooks = JsonConvert.DeserializeObject<List<Library>>(File.ReadAllText(_bookCatalogPath))
                     .Where(b => b.LastBorrower.Name != "").ToList();

                if (borrowedBooks.Count > 0)
                {
                    foreach (Library book in borrowedBooks)
                    {
                        BorrowerRecord borrowerRecord = currentBorrowers.FirstOrDefault(b => b.LastBorrower.Name == book.LastBorrower.Name
                            && b.LastBorrower.LastName == book.LastBorrower.Lastname);

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
                                    Name = book.LastBorrower.Name,
                                    LastName = book.LastBorrower.Lastname
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

        private BookRecord GetSpecificBook(Library targetBook)
        {
            DateTime lastBorrow = DateTime.MinValue;
            DateTime.TryParse(targetBook.LastBorrowDate, out lastBorrow);

            BookRecord bookRecord = new BookRecord()
            {
                Book = new Book()
                {
                    Title = targetBook.Title,
                    Author = targetBook.Author,
                    ISBN = targetBook.Isbn
                },
                LastBorrowDate = lastBorrow,
                LastBorrower = new Borrower()
                {
                    Name = targetBook.LastBorrower.Name,
                    LastName = targetBook.LastBorrower.Lastname
                }
            };
            return bookRecord;
        }
    }
}
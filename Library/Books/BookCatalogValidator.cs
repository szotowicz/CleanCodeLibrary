using System;
using System.IO;

namespace Library.Books
{
    class BookCatalogValidator : IBookCatalogValidator
    {
        public bool IsValid(string bookCatalogPath)
        {
            try
            {
                if (!File.Exists(bookCatalogPath))
                {
                    return false;
                }

                if (Path.GetExtension(bookCatalogPath) != ".xml" && Path.GetExtension(bookCatalogPath) != ".json")
                {
                    return false;
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
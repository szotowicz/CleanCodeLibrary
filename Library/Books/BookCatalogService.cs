using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Books
{
    class BookCatalogService : IBookCatalogService
    {
        private readonly IBookCatalogValidator _bookCatalogValidator = new BookCatalogValidator();
        private string _bookCatalogPath;

        public bool SetBookCatalog(string fileCatalog)
        {
            if (_bookCatalogValidator.IsValid(fileCatalog))
            {
                _bookCatalogPath = fileCatalog;
                return true;
            }
            return false;
        }
    }
}
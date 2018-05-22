using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Books
{
    class BookRecord
    {
        public Book Book { get; set; }
        public DateTime LastBorrowDate { get; set; }
        public Borrower LastBorrower { get; set; }
    }
}
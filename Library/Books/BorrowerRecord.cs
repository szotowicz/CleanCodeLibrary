using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Books
{
    class BorrowerRecord : Borrower
    {
        public Borrower LastBorrower { get; set; }
        public int NumberOfBooksBorrowed { get; set; }
    }
}
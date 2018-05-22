namespace Library.Activities
{
    public class Library
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public string LastBorrowDate { get; set; }
        public LastBorrower LastBorrower { get; set; }
    }
}
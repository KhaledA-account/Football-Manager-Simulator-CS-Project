using System;

namespace FootballManager
{
    public class Transfer
    {
        public Player Player { get; set; }
        public Club FromClub { get; set; }
        public Club ToClub { get; set; }
        public double Fee { get; set; }
        public DateTime Date { get; set; }
    }

    // Merged Listing class
    public class Listing
    {
        public Player Player { get; set; }
        public Club Club { get; set; }
        public DateTime Date { get; set; }
    }
}

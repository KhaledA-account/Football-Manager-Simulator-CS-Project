using System;

namespace FootballManager
{
    public class Fixture
    {
        public Club HomeTeam { get; set; }
        public Club AwayTeam { get; set; }
        public DateTime Date { get; set; }
        public bool Played { get; set; }
        public string Score { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
    }
}

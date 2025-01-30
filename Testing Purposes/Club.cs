using System;
using System.Collections.Generic;

namespace FootballManager
{
    public class Club
    {
        public string Name { get; }
        public List<Player> Players { get; }
        public ClubLeagueStatistics Stats { get; }

        // Added properties for finances
        public double Balance { get; set; }
        public double WagesPaidThisWeek { get; set; }
        public double TotalTransfersOut { get; set; }
        public double TotalTransfersIn { get; set; }
        public double TotalProfit => TotalTransfersOut - TotalTransfersIn;
        public DateTime LastFinanceUpdateDate { get; set; }

        // Added properties for formation and position assignments
        public Formation SelectedFormation { get; set; }
        public Dictionary<int, Player> PositionAssignments { get; set; }

        public Club(string name)
        {
            Name = name;
            Players = new List<Player>();
            Stats = new ClubLeagueStatistics();
            LastFinanceUpdateDate = new DateTime(2024, 8, 17); // Starting date
            WagesPaidThisWeek = 0.0;
            TotalTransfersOut = 0.0;
            TotalTransfersIn = 0.0;
            Balance = 0.0; // Default balance, can be set when loading
            SelectedFormation = null; // Default to no formation selected
            PositionAssignments = new Dictionary<int, Player>();
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }
    }

    public class ClubLeagueStatistics
    {
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Played => Wins + Draws + Losses;

        public ClubLeagueStatistics()
        {
            Wins = 0;
            Draws = 0;
            Losses = 0;
            Points = 0;
            GoalsFor = 0;
            GoalsAgainst = 0;
        }
    }
}

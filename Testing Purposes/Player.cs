using System;
using System.Collections.Generic;

namespace FootballManager
{
    public class Player
    {
        public string Name { get; set; }
        public double Value { get; set; } // Value in millions
        public int Rating { get; set; }
        public int Potential { get; set; }
        public string Position { get; set; }
        public bool AvailableForTransfer { get; set; }
        public Club CurrentClub { get; set; }
        public int Age { get; set; }
        public Dictionary<string, int> Statistics { get; set; }

        // Contract management
        public double Wage { get; set; }            // Wage in K per week
        public double WageProposal { get; set; }
        public int ContractLength { get; set; }      // Contract length in years
        public int ContractLengthProposal { get; set; }
        public string SquadStatus { get; set; }      // e.g. First Team Member, Backup Player, Youngster
        public string SquadStatusProposal { get; set; }

        // Additional properties for transfer market
        public double BoughtFor { get; set; }        // Amount bought for
        public double Profit { get; set; }           // Profit from selling
        public double TransferPrice { get; set; }    // Price in millions

        public Player(string name, string position, double value, int rating, int potential, Club currentClub)
        {
            Name = name;
            Position = position;
            Value = value;
            Rating = rating;
            Potential = potential;
            CurrentClub = currentClub;
            Age = 18; // Default age, can be set later
            Statistics = new Dictionary<string, int>
            {
                { "Goals", 0 },
                { "Assists", 0 },
                { "Appearances", 0 }
            };
            Wage = 5.0; // Fixed at £5K per week
            ContractLength = 3; // Default contract length
            SquadStatus = "First Team Member";
            AvailableForTransfer = false;
            TransferPrice = 0.0;
        }
    }
}

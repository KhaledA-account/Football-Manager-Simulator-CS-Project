using System;
using System.Collections.Generic;
using System.Linq;

namespace FootballManager
{
    public class League
    {
        public List<Club> Clubs { get; private set; }
        public List<Fixture> Fixtures { get; private set; }
        public DateTime CurrentDate { get; set; }
        public DateTime SeasonStartDate { get; private set; }
        public DateTime SeasonEndDate { get; private set; }

        private Random randomGenerator = new Random();

        // Predefined club names
        private readonly List<string> predefinedClubNames = new List<string>
        {
            "Liverpool",
            "Arsenal",
            "Nottm Forest",
            "Man City",
            "Newcastle",
            "Chelsea",
            "AFC Bournemouth",
            "Aston Villas",
            "Brighton",
            "Fulham",
            "Brentford",
            "Man Utd",
            "Crystal Palace",
            "West Ham",
            "Spurs",
            "Everton"
        };

        // Predefined first and last names for player generation
        private readonly List<string> firstNames = new List<string>
        {
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph",
            "Thomas", "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Mark",
            "Donald", "Steven", "Paul", "Andrew", "Joshua", "Kevin", "Brian", "George",
            "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob"
        };

        private readonly List<string> lastNames = new List<string>
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
            "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"
        };

        // Positions list
        private readonly List<string> positions = new List<string>
        {
            "GK", "LB", "CB", "RB", "CM", "CAM", "LM", "RM", "LW", "RW", "ST"
        };

        public League()
        {
            Clubs = new List<Club>();
            Fixtures = new List<Fixture>();
            SeasonStartDate = new DateTime(DateTime.Now.Year, 8, 1);
            SeasonEndDate = new DateTime(DateTime.Now.Year + 1, 5, 31);
            CurrentDate = SeasonStartDate;

            GenerateClubsAndPlayers();
            GenerateFixtures();
        }

        private void GenerateClubsAndPlayers()
        {
            foreach (var clubName in predefinedClubNames)
            {
                Club club = new Club(clubName)
                {
                    Balance = 70_000_000 // 70 Million pounds
                };

                // Generate players for the club
                int numberOfPlayers = randomGenerator.Next(20, 26); // Each club has between 20 to 25 players
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    Player player = GenerateRandomPlayer(club);
                    club.Players.Add(player);
                }

                Clubs.Add(club);
            }
        }

        private Player GenerateRandomPlayer(Club club)
        {
            string firstName = firstNames[randomGenerator.Next(firstNames.Count)];
            string lastName = lastNames[randomGenerator.Next(lastNames.Count)];
            string name = $"{firstName} {lastName}";

            string position = positions[randomGenerator.Next(positions.Count)];

            double value = Math.Round(randomGenerator.NextDouble() * 90 + 10, 2); // Value between £10M to £100M
            int rating = randomGenerator.Next(50, 101); // Rating between 50 to 100
            int potential = randomGenerator.Next(rating, 101); // Potential >= current rating
            int age = randomGenerator.Next(18, 35); // Age between 18 to 34

            Player player = new Player(name, position, value, rating, potential, club)
            {
                Wage = 5.0, // Fixed at £5K per week
                ContractLength = randomGenerator.Next(1, 6), // Contract length between 1 to 5 years
                SquadStatus = "First Team Member",
                AvailableForTransfer = false,
                TransferPrice = 0.0
            };

            // Initialize some statistics
            player.Statistics = new Dictionary<string, int>
            {
                { "Goals", randomGenerator.Next(0, 21) },
                { "Assists", randomGenerator.Next(0, 21) },
                { "Appearances", randomGenerator.Next(0, 38) }
            };

            return player;
        }

        private void GenerateFixtures()
        {
            // Simple round-robin schedule: each club plays every other club twice (home and away)
            for (int i = 0; i < Clubs.Count; i++)
            {
                for (int j = 0; j < Clubs.Count; j++)
                {
                    if (i != j)
                    {
                        Fixture fixture = new Fixture
                        {
                            HomeTeam = Clubs[i],
                            AwayTeam = Clubs[j],
                            Date = CurrentDate,
                            Played = false,
                            Score = "0:0",
                            HomeGoals = 0,
                            AwayGoals = 0
                        };
                        Fixtures.Add(fixture);
                    }
                }
            }

            // Optionally, you can randomize fixture dates within the season
            ShuffleFixtures();
            AssignFixtureDates();
        }

        private void ShuffleFixtures()
        {
            // Fisher-Yates shuffle
            int n = Fixtures.Count;
            while (n > 1)
            {
                n--;
                int k = randomGenerator.Next(n + 1);
                Fixture value = Fixtures[k];
                Fixtures[k] = Fixtures[n];
                Fixtures[n] = value;
            }
        }

        private void AssignFixtureDates()
        {
            DateTime date = SeasonStartDate;
            int matchesPerWeek = 4; // Assuming 4 matches per week
            int daysOffset = 0;

            foreach (var fixture in Fixtures)
            {
                // Find the next Saturday
                while (date.DayOfWeek != DayOfWeek.Saturday)
                {
                    date = date.AddDays(1);
                }

                fixture.Date = date;
                date = date.AddDays(7 / matchesPerWeek); // Distribute matches evenly
            }
        }

        public List<Player> GetAllPlayers()
        {
            return Clubs.SelectMany(c => c.Players).ToList();
        }

        public void StartNewSeason()
        {
            // Reset statistics
            foreach (var club in Clubs)
            {
                club.Stats = new ClubLeagueStatistics();
                foreach (var player in club.Players)
                {
                    player.Age += 1;
                    // Optionally, update player attributes here
                }
            }

            // Reset fixtures
            Fixtures.Clear();
            GenerateFixtures();

            // Reset current date
            CurrentDate = SeasonStartDate;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FootballManager
{
    public class League
    {
        public List<Club> Clubs { get; }
        public List<Fixture> Fixtures { get; private set; }
        public DateTime CurrentDate { get; set; }
        public DateTime SeasonStartDate { get; private set; }
        public DateTime SeasonEndDate { get; private set; }

        public League()
        {
            Clubs = new List<Club>();
            Fixtures = new List<Fixture>();

            // Example season window
            SeasonStartDate = new DateTime(2024, 8, 16);
            SeasonEndDate = new DateTime(2025, 5, 25);
            CurrentDate = SeasonStartDate;

            LoadClubs();
            LoadTransferBudgets();

            // Ensure each club has at least 18 players
            foreach (var club in Clubs)
            {
                EnsureMinimumPlayers(club, 18);
            }

            GenerateFixtures();
        }

        private void LoadClubs()
        {
            try
            {
                string directoryPath = @"C:\Users\mobze\Documents\Computer Science Projects\FM2025_1.0\3rd  prototype\3rd  prototype\important stuff";
                string[] clubFiles = Directory.GetFiles(directoryPath, "*.txt");

                foreach (string filePath in clubFiles)
                {
                    Club club = LoadClubFromFile(filePath);
                    if (club != null)
                    {
                        Clubs.Add(club);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading clubs: {ex.Message}");
            }
        }

        private Club LoadClubFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int i = 0;

                // Extract the club name from the file name
                string clubName = System.IO.Path.GetFileNameWithoutExtension(filePath);

                Club club = new Club(clubName);

                while (i < lines.Length)
                {
                    string line = lines[i++].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Split the line at the first colon
                    int colonIndex = line.IndexOf(':');
                    if (colonIndex == -1)
                        continue; // Skip lines without a colon

                    string positions = line.Substring(0, colonIndex).Trim();
                    string nameAndAttributes = line.Substring(colonIndex + 1).Trim();

                    // Variables to store player attributes
                    string playerName = "";
                    int rating = 0, potential = 0, age = 0;
                    double value = 0.0;
                    Dictionary<string, int> statistics = new Dictionary<string, int>();
                    Player player = null;

                    int nameStartIndex = nameAndAttributes.IndexOf('(');
                    int nameEndIndex = nameAndAttributes.LastIndexOf(')');
                    if (nameStartIndex == -1 || nameEndIndex == -1 || nameEndIndex <= nameStartIndex)
                    {
                        playerName = nameAndAttributes.Trim();
                        // Default attributes
                        rating = 0;
                        potential = 0;
                        age = 0;
                        value = 0.0;
                        statistics = new Dictionary<string, int>();

                        player = new Player(playerName, positions, value, rating, potential, club)
                        {
                            Age = age,
                            Statistics = statistics
                        };
                        club.Players.Add(player);
                        continue;
                    }

                    playerName = nameAndAttributes.Substring(0, nameStartIndex).Trim();
                    string attributesStr = nameAndAttributes.Substring(
                        nameStartIndex + 1,
                        nameEndIndex - nameStartIndex - 1
                    ).Trim();

                    // Parse attributes using regex
                    rating = 0;
                    potential = 0;
                    age = 0;
                    var attrMatches = Regex.Matches(attributesStr, @"(\w+):\s*(\d+)");
                    foreach (var matchObj in attrMatches)
                    {
                        var match = (Match)matchObj;
                        string key = match.Groups[1].Value.Trim();
                        string attrValue = match.Groups[2].Value.Trim();

                        if (key.Equals("Rating", StringComparison.OrdinalIgnoreCase))
                            int.TryParse(attrValue, out rating);
                        else if (key.Equals("Potential", StringComparison.OrdinalIgnoreCase))
                            int.TryParse(attrValue, out potential);
                        else if (key.Equals("Age", StringComparison.OrdinalIgnoreCase))
                            int.TryParse(attrValue, out age);
                    }

                    // Parse Value
                    if (i >= lines.Length)
                        continue;

                    string valueLine = lines[i++].Trim();
                    if (!valueLine.StartsWith("Value:", StringComparison.OrdinalIgnoreCase))
                    {
                        i--;
                        value = 0.0;
                    }
                    else
                    {
                        string valueStr = valueLine.Substring("Value:".Length)
                                                  .Trim()
                                                  .Replace(",", "")
                                                  .Replace("£", "")
                                                  .Replace("$", "")
                                                  .Replace("€", "");

                        value = 0.0;
                        if (!string.IsNullOrWhiteSpace(valueStr))
                        {
                            if (valueStr.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                            {
                                valueStr = valueStr.Substring(0, valueStr.Length - 1);
                                if (double.TryParse(valueStr, out value))
                                    value *= 1_000_000;
                            }
                            else if (valueStr.EndsWith("K", StringComparison.OrdinalIgnoreCase))
                            {
                                valueStr = valueStr.Substring(0, valueStr.Length - 1);
                                if (double.TryParse(valueStr, out value))
                                    value *= 1_000;
                            }
                            else
                            {
                                double.TryParse(valueStr, out value);
                            }
                        }
                    }
                    value /= 1_000_000; // Convert to millions

                    // Parse Player Statistics
                    statistics = new Dictionary<string, int>();
                    if (i < lines.Length)
                    {
                        string statsHeader = lines[i].Trim();
                        if (statsHeader.Equals("Player Statistics:", StringComparison.OrdinalIgnoreCase))
                        {
                            i++; // Move to the first stat line
                            while (i < lines.Length && lines[i].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                            {
                                string statLine = lines[i++].Trim('-', ' ');
                                var statParts = statLine.Split(':');
                                if (statParts.Length == 2)
                                {
                                    string statName = statParts[0].Trim();
                                    string statValueStr = statParts[1].Trim();
                                    if (int.TryParse(statValueStr, out int statValue))
                                    {
                                        statistics[statName] = statValue;
                                    }
                                }
                            }
                        }
                    }

                    // Create player
                    player = new Player(playerName, positions, value, rating, potential, club)
                    {
                        Age = age,
                        Statistics = statistics
                    };
                    club.Players.Add(player);
                }

                return club;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading club from file {filePath}: {ex.Message}");
                return null;
            }
        }

        private void LoadTransferBudgets()
        {
            string budgetFilePath = @"C:\Users\mobze\Documents\Computer Science Projects\FM2025_1.0\3rd  prototype\3rd  prototype\Transfer Budget.txt";
            if (!System.IO.File.Exists(budgetFilePath))
            {
                Console.WriteLine("Transfer budgets file not found.");
                return;
            }

            string[] lines = System.IO.File.ReadAllLines(budgetFilePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string clubName = parts[0].Trim();
                    string budgetStr = parts[1].Trim()
                                              .Replace(",", "")
                                              .Replace("£", "")
                                              .Replace("$", "")
                                              .Replace("€", "");
                    double budget = 0.0;

                    if (budgetStr.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                    {
                        budgetStr = budgetStr.Substring(0, budgetStr.Length - 1);
                        if (double.TryParse(budgetStr, out budget))
                        {
                            budget *= 1_000_000;
                        }
                    }
                    else if (budgetStr.EndsWith("K", StringComparison.OrdinalIgnoreCase))
                    {
                        budgetStr = budgetStr.Substring(0, budgetStr.Length - 1);
                        if (double.TryParse(budgetStr, out budget))
                        {
                            budget *= 1_000;
                        }
                    }
                    else
                    {
                        double.TryParse(budgetStr, out budget);
                    }

                    var club = Clubs.FirstOrDefault(c => c.Name.Equals(clubName, StringComparison.OrdinalIgnoreCase));
                    if (club != null)
                    {
                        club.Balance = budget;
                    }
                }
            }
        }

        private void EnsureMinimumPlayers(Club club, int minCount)
        {
            var random = new Random();

            string[] firstNames = { "John", "Michael", "Alex", "Chris", "David", "James", "Robert", "Daniel", "Mark", "Steven", "Paul" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Davis", "Miller", "Wilson", "Anderson", "Taylor", "Thomas" };
            string[] possiblePositions = {
                "GK", "LB", "RB", "CB", "CM", "CDM", "CAM", "LM", "RM", "LW", "RW", "ST"
            };

            while (club.Players.Count < minCount)
            {
                // Generate random name
                string fName = firstNames[random.Next(firstNames.Length)];
                string lName = lastNames[random.Next(lastNames.Length)];
                string fullName = fName + " " + lName + " (Auto)";

                // Generate random position
                string randPos = possiblePositions[random.Next(possiblePositions.Length)];

                // Random rating & potential
                int rating = random.Next(55, 80); // e.g., 55–79
                int potential = rating + random.Next(0, 6); // up to +5

                // Random value in millions
                double value = Math.Round(random.NextDouble() * 10.0, 2); // up to 10M

                Player newPlayer = new Player(fullName, randPos, value, rating, potential, club)
                {
                    Age = random.Next(18, 35),
                    Wage = random.Next(5, 50),          // 5K–50K/week
                    ContractLength = random.Next(1, 5), // 1–4 years
                    SquadStatus = "Backup Player",
                    AvailableForTransfer = false
                };

                club.Players.Add(newPlayer);
            }
        }

        public List<Player> GetAllPlayers()
        {
            List<Player> allPlayers = new List<Player>();
            foreach (var club in Clubs)
            {
                allPlayers.AddRange(club.Players);
            }
            return allPlayers;
        }

        public void GenerateFixtures()
        {
            var clubsCopy = new List<Club>(Clubs);
            int numClubs = clubsCopy.Count;

            // If odd, add a null to represent a "bye" slot
            bool isEven = (numClubs % 2 == 0);
            if (!isEven)
            {
                clubsCopy.Add(null);
                numClubs++;
            }

            // We want exactly 38 total rounds
            int totalRounds = 38;
            int matchesPerRound = numClubs / 2;

            // Generate match dates using the merged code:
            List<DateTime> matchDates = GenerateMatchDates(SeasonStartDate, SeasonEndDate);

            for (int round = 0; round < totalRounds; round++)
            {
                DateTime roundDate = matchDates[round];
                List<Fixture> roundFixtures = new List<Fixture>();

                for (int matchIdx = 0; matchIdx < matchesPerRound; matchIdx++)
                {
                    int homeIndex = (round + matchIdx) % (numClubs - 1);
                    int awayIndex = (numClubs - 1 - matchIdx + round) % (numClubs - 1);

                    // The last slot in the list is the 'null' if we had an odd number
                    if (matchIdx == 0)
                    {
                        awayIndex = numClubs - 1;
                    }

                    var homeTeam = clubsCopy[homeIndex];
                    var awayTeam = clubsCopy[awayIndex];

                    // Skip if bye
                    if (homeTeam == null || awayTeam == null)
                        continue;

                    // Alternate home/away each half of the season
                    if (round >= totalRounds / 2)
                    {
                        var temp = homeTeam;
                        homeTeam = awayTeam;
                        awayTeam = temp;
                    }

                    roundFixtures.Add(new Fixture
                    {
                        HomeTeam = homeTeam,
                        AwayTeam = awayTeam,
                        Date = roundDate,
                        Played = false,
                        Score = null,
                        HomeGoals = 0,
                        AwayGoals = 0
                    });
                }

                Fixtures.AddRange(roundFixtures);
            }

            // Remove the null placeholder if we added it
            if (!isEven)
            {
                clubsCopy.RemoveAt(clubsCopy.Count - 1);
            }
        }

        public void StartNewSeason()
        {
            // Reset club statistics
            foreach (var club in Clubs)
            {
                club.Stats.Wins = 0;
                club.Stats.Draws = 0;
                club.Stats.Losses = 0;
                club.Stats.Points = 0;
                club.Stats.GoalsFor = 0;
                club.Stats.GoalsAgainst = 0;
            }

            // Move season one year forward
            SeasonStartDate = SeasonStartDate.AddYears(1);
            SeasonEndDate = SeasonEndDate.AddYears(1);
            CurrentDate = SeasonStartDate;

            // Clear & re-generate
            Fixtures.Clear();
            GenerateFixtures();
        }

        // -------------------------------------------------------------------
        // SCHEDULE MANAGER METHODS (merged from old ScheduleManager.cs)
        // -------------------------------------------------------------------

        private List<DateTime> GenerateMatchDates(DateTime seasonStart, DateTime seasonEnd)
        {
            var matchDates = new List<DateTime>();
            var blockedDates = GetBlockedDates();

            // First, collect all Saturdays in the range that are not blocked
            var saturdays = CollectWeekday(seasonStart, seasonEnd, DayOfWeek.Saturday, blockedDates);

            // We need exactly 38 matchdays. If saturdays are enough, we pick the first 38.
            // Otherwise, we add some Wednesdays as needed to get up to 38.
            if (saturdays.Count >= 38)
            {
                // Just take the earliest 38 Saturdays
                for (int i = 0; i < 38; i++)
                {
                    matchDates.Add(saturdays[i]);
                }
            }
            else
            {
                matchDates.AddRange(saturdays);
                int needed = 38 - matchDates.Count;

                // Now gather Wednesdays for additional matchdays, skipping blocked dates
                var wednesdays = CollectWeekday(seasonStart, seasonEnd, DayOfWeek.Wednesday, blockedDates);

                int index = 0;
                while (matchDates.Count < 38 && index < wednesdays.Count)
                {
                    matchDates.Add(wednesdays[index]);
                    index++;
                }
            }

            // Sort final list of dates in ascending order
            matchDates.Sort();

            // If for some reason we still don’t have 38, we’ll just stop at however many we got
            if (matchDates.Count > 38)
            {
                matchDates = matchDates.GetRange(0, 38);
            }

            return matchDates;
        }

        private HashSet<DateTime> GetBlockedDates()
        {
            var blocked = new HashSet<DateTime>();

            // Christmas break (23–26 Dec 2024)
            for (DateTime d = new DateTime(2024, 12, 23);
                 d <= new DateTime(2024, 12, 26);
                 d = d.AddDays(1))
            {
                blocked.Add(d);
            }

            // New Year: 1 Jan 2025
            blocked.Add(new DateTime(2025, 1, 1));

            // Easter (29 Mar–1 Apr 2025) - approximate
            for (DateTime d = new DateTime(2025, 3, 29);
                 d <= new DateTime(2025, 4, 1);
                 d = d.AddDays(1))
            {
                blocked.Add(d);
            }

            // Bank holidays:
            // 26 Aug 2024
            blocked.Add(new DateTime(2024, 8, 26));
            // 5 May 2025
            blocked.Add(new DateTime(2025, 5, 5));
            // 26 May 2025
            blocked.Add(new DateTime(2025, 5, 26));

            return blocked;
        }

        private List<DateTime> CollectWeekday(
            DateTime start,
            DateTime end,
            DayOfWeek dayOfWeek,
            HashSet<DateTime> blockedDates)
        {
            var dates = new List<DateTime>();

            // Move "current" to the first desired dayOfWeek on or after start
            DateTime current = FindFirstDayOfWeekOnOrAfter(start, dayOfWeek);

            while (current <= end)
            {
                if (!blockedDates.Contains(current))
                {
                    dates.Add(current);
                }
                current = current.AddDays(7); // jump to next same weekday
            }

            return dates;
        }

        private DateTime FindFirstDayOfWeekOnOrAfter(DateTime start, DayOfWeek dayOfWeek)
        {
            while (start.DayOfWeek != dayOfWeek)
            {
                start = start.AddDays(1);
            }
            return start;
        }
    }
}

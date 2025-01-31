using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FootballManager
{
    public class MainMenuWindow : Window
    {
        private List<string> _menuOptions;
        private int _activeMenuIndex;
        private Club _userClub;
        private League _league;
        private DateTime _currentDate;
        private LinkedList<Transfer> _recentTransfers;
        private LinkedList<Listing> _recentListings;
        private const int MaxRecentItems = 5;

        private bool isSimulating = false;

        public MainMenuWindow(Club userClub, League league, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _userClub = userClub;
            _league = league;
            _currentDate = league.CurrentDate;
            _menuOptions = new List<string>
            {
                "Table",
                "Players",
                "Squad Management",
                "Transfers",
                "Fixtures",
                "Exit"
            };
            _activeMenuIndex = 0;
            _recentTransfers = new LinkedList<Transfer>();
            _recentListings = new LinkedList<Listing>();
        }

        public override void Update()
        {
            var key = UserInterface.Input.Key;

            // Spacebar toggles daily simulation
            if (key == ConsoleKey.Spacebar)
            {
                isSimulating = !isSimulating;
            }

            if (isSimulating)
            {
                // Simulate one day
                _currentDate = _currentDate.AddDays(1);
                _league.CurrentDate = _currentDate;
                ProcessDailyEvents();
            }
            else
            {
                // Not simulating – normal menu navigation
                if (key == ConsoleKey.UpArrow)
                {
                    _activeMenuIndex = (_activeMenuIndex - 1 + _menuOptions.Count) % _menuOptions.Count;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    _activeMenuIndex = (_activeMenuIndex + 1) % _menuOptions.Count;
                }
                else if (key == ConsoleKey.Enter)
                {
                    HandleMenuSelection();
                }
            }
        }

        public override void Draw(bool active)
        {
            // Clear the window area only
            ClearWindowArea();

            base.Draw(active);

            // Draw main menu, finances, recent transfers, etc.
            DrawMenu();
            DrawCurrentBalanceAndDate();
            DrawRecentTransfers();
            DrawRecentListings();
            DrawFinances();
            DrawRecentAndUpcomingFixtures();

            // Indicate simulation state
            if (isSimulating)
            {
                Console.SetCursorPosition(_rectangle.X + _rectangle.Width - 20, _rectangle.Y + 1);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Simulating...");
                Console.ResetColor();
            }
        }

        private void HandleMenuSelection()
        {
            string selectedOption = _menuOptions[_activeMenuIndex];

            switch (selectedOption)
            {
                case "Table":
                    CurrentAction = InterfaceAction.ViewTable;
                    break;
                case "Players":
                    CurrentAction = InterfaceAction.ViewPlayers;
                    break;
                case "Squad Management":
                    CurrentAction = InterfaceAction.ManageSquad;
                    break;
                case "Transfers":
                    CurrentAction = InterfaceAction.ViewTransfers;
                    break;
                case "Fixtures":
                    CurrentAction = InterfaceAction.ViewFixtures;
                    break;
                case "Exit":
                    CurrentAction = InterfaceAction.Exit;
                    break;
            }
        }
        /// <summary>
        /// Called each time we simulate a day if isSimulating == true.
        /// This is where we handle daily events (transfers, listings, match days, etc.).
        /// </summary>
        private void ProcessDailyEvents()
        {
            Random rnd = new Random();

            // Transfer & listing probabilities
            double transferProbability = 0.3;
            double listingProbability = 0.3;

            var clubs = _league.Clubs;

            // Simulate possible random transfers
            if (rnd.NextDouble() < transferProbability)
            {
                SimulateRandomTransfer(rnd);
            }

            // Simulate possible random listing
            if (rnd.NextDouble() < listingProbability)
            {
                SimulateRandomListing(rnd);
            }

            // Update weekly finances every 7 days
            if ((_currentDate - _userClub.LastFinanceUpdateDate).Days >= 7)
            {
                UpdateWeeklyFinances();
                _userClub.LastFinanceUpdateDate = _currentDate;
            }

            // Check if the season ended
            if (_league.CurrentDate > _league.SeasonEndDate)
            {
                _league.StartNewSeason();
                Console.Clear();
                Console.WriteLine("A new season has started!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }

            // Check for fixtures today
            var todaysFixtures = _league.Fixtures
                .Where(f => f.Date.Date == _league.CurrentDate.Date && !f.Played)
                .ToList();

            // Is user’s club playing today?
            var userFixture = todaysFixtures.FirstOrDefault(f =>
                f.HomeTeam == _userClub || f.AwayTeam == _userClub);

            if (userFixture != null)
            {
                // Pause sim
                isSimulating = false;

                Console.Clear();
                var opponentName = (userFixture.HomeTeam == _userClub)
                    ? userFixture.AwayTeam.Name
                    : userFixture.HomeTeam.Name;

                Console.WriteLine($"You have a match today against {opponentName}.");
                Console.WriteLine("Would you like to simulate the match live? (Y/N)");
                var input = Console.ReadKey(true).Key;

                if (input == ConsoleKey.Y)
                {
                    // Call the new LiveMatchSimulation class instead
                    Program.StartNoNullSimulation(userFixture, _league, _userClub);
                }
                else
                {
                    // Quick auto-resolve
                    SimulateQuickMatch(userFixture);
                }
            }

            // Auto-resolve all other fixtures not involving the user
            foreach (var fix in todaysFixtures)
            {
                if (!fix.Played && fix != userFixture)
                {
                    SimulateQuickMatch(fix);
                }
            }
        }

        private void SimulateRandomListing(Random rnd)
        {
            var clubs = _league.Clubs
                .Where(c => c != _userClub && c.Players.Count > 0) // exclude user’s club for listing
                .ToList();

            if (clubs.Count == 0) return;

            var club = clubs[rnd.Next(clubs.Count)];
            if (club.Players.Count == 0) return;

            var player = club.Players[rnd.Next(club.Players.Count)];
            if (player.AvailableForTransfer) return;

            // random price
            double minPrice = player.Value * 0.8;
            double maxPrice = player.Value * 1.2;
            double transferPrice = Math.Round(minPrice + rnd.NextDouble() * (maxPrice - minPrice), 2);

            player.AvailableForTransfer = true;
            player.TransferPrice = transferPrice;

            Listing listing = new Listing
            {
                Player = player,
                Club = club,
                Date = _currentDate
            };
            _recentListings.AddFirst(listing);
            if (_recentListings.Count > MaxRecentItems)
                _recentListings.RemoveLast();
        }

        private void SimulateRandomTransfer(Random rnd)
        {
            var clubs = _league.Clubs;
            var fromClubCandidates = clubs.Where(c => c.Players.Any(p => p.AvailableForTransfer)).ToList();
            if (fromClubCandidates.Count == 0) return;

            var fromClub = fromClubCandidates[rnd.Next(fromClubCandidates.Count)];
            var toClubCandidates = clubs
                .Where(c => c != fromClub && c.Balance > 0)
                .ToList();
            if (toClubCandidates.Count == 0) return;

            var toClub = toClubCandidates[rnd.Next(toClubCandidates.Count)];
            var availablePlayers = fromClub.Players
                .Where(p => p.AvailableForTransfer).ToList();
            if (availablePlayers.Count == 0) return;

            var player = availablePlayers[rnd.Next(availablePlayers.Count)];
            double fee = player.TransferPrice;
            double transferFee = fee * 1_000_000; // actual amount

            if (toClub.Balance < transferFee) return;

            // Transfer
            fromClub.Players.Remove(player);
            toClub.Players.Add(player);
            player.CurrentClub = toClub;
            player.AvailableForTransfer = false;
            player.TransferPrice = 0;

            fromClub.Balance += transferFee;
            toClub.Balance -= transferFee;

            fromClub.TotalTransfersOut += transferFee;
            toClub.TotalTransfersIn += transferFee;

            Transfer transfer = new Transfer
            {
                Player = player,
                FromClub = fromClub,
                ToClub = toClub,
                Fee = fee,
                Date = _currentDate
            };
            _recentTransfers.AddFirst(transfer);
            if (_recentTransfers.Count > MaxRecentItems)
                _recentTransfers.RemoveLast();
        }

        private void SimulateQuickMatch(Fixture fixture)
        {
            Random rnd = new Random();
            int homeGoals = rnd.Next(0, 5);
            int awayGoals = rnd.Next(0, 5);

            fixture.Played = true;
            fixture.HomeGoals = homeGoals;
            fixture.AwayGoals = awayGoals;
            fixture.Score = $"{homeGoals} : {awayGoals}";

            // Update league table
            var homeStats = fixture.HomeTeam.Stats;
            var awayStats = fixture.AwayTeam.Stats;
            homeStats.GoalsFor += homeGoals;
            homeStats.GoalsAgainst += awayGoals;
            awayStats.GoalsFor += awayGoals;
            awayStats.GoalsAgainst += homeGoals;

            if (homeGoals > awayGoals)
            {
                homeStats.Wins++;
                homeStats.Points += 3;
                awayStats.Losses++;
            }
            else if (homeGoals < awayGoals)
            {
                awayStats.Wins++;
                awayStats.Points += 3;
                homeStats.Losses++;
            }
            else
            {
                homeStats.Draws++;
                homeStats.Points++;
                awayStats.Draws++;
                awayStats.Points++;
            }
        }

        private void UpdateWeeklyFinances()
        {
            double totalWages = _userClub.Players.Sum(p => p.Wage * 1000);
            _userClub.Balance -= totalWages;
            _userClub.WagesPaidThisWeek = totalWages;
        }

        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }

        private void DrawMenu()
        {
            int menuX = _rectangle.X + 2;
            int menuY = _rectangle.Y + 4;

            Console.SetCursorPosition(menuX, menuY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Menu:");
            Console.ResetColor();
            menuY++;

            for (int i = 0; i < _menuOptions.Count; i++)
            {
                Console.SetCursorPosition(menuX, menuY + i);
                if (i == _activeMenuIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {_menuOptions[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {_menuOptions[i]}");
                }
            }
        }

        private void DrawCurrentBalanceAndDate()
        {
            int balanceX = _rectangle.X + 30;
            int balanceY = _rectangle.Y + 4;

            // Current Balance
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("+------------------------+");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("| Current Balance        |");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("+------------------------+");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine($"| £{_userClub.Balance:n0}".PadRight(24) + "|");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("+------------------------+");

            // Date
            balanceY += 2;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("+------------------------+");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("| Date                   |");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("+------------------------+");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine($"| {_currentDate.ToLongDateString()}".PadRight(24) + "|");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("+------------------------+");
            balanceY++;
            Console.SetCursorPosition(balanceX, balanceY);
            Console.WriteLine("Press Spacebar to toggle simulation.");
        }

        private void DrawRecentTransfers()
        {
            int transferX = _rectangle.X + 2;
            int transferY = _rectangle.Y + 15;

            Console.SetCursorPosition(transferX, transferY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("+------------------ Recent Transfers ------------------+");
            transferY++;

            if (_recentTransfers.Count == 0)
            {
                Console.SetCursorPosition(transferX, transferY);
                Console.WriteLine("| No recent transfers".PadRight(54) + "|");
                transferY++;
            }
            else
            {
                foreach (var transfer in _recentTransfers)
                {
                    if (transferY > _rectangle.Y + _rectangle.Height - 2) break;

                    Console.SetCursorPosition(transferX, transferY);
                    string info = $"{transfer.Player.Name} from {transfer.FromClub.Name} to {transfer.ToClub.Name} for £{transfer.Fee}M";
                    if (info.Length > 54) info = info.Substring(0, 51) + "...";
                    Console.WriteLine($"| {info}".PadRight(54) + "|");
                    transferY++;
                }
            }

            Console.SetCursorPosition(transferX, transferY);
            Console.WriteLine("+------------------------------------------------------+");
        }

        private void DrawRecentListings()
        {
            int listingX = _rectangle.X + 2;
            int listingY = _rectangle.Y + 25;

            Console.SetCursorPosition(listingX, listingY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("+------------------ Recent Listings -------------------+");
            listingY++;

            if (_recentListings.Count == 0)
            {
                Console.SetCursorPosition(listingX, listingY);
                Console.WriteLine("| No recent listings".PadRight(54) + "|");
                listingY++;
            }
            else
            {
                foreach (var listing in _recentListings)
                {
                    if (listingY > _rectangle.Y + _rectangle.Height - 2) break;

                    Console.SetCursorPosition(listingX, listingY);
                    string info = $"{listing.Player.Name} from {listing.Club.Name} listed for £{listing.Player.TransferPrice}M";
                    if (info.Length > 54) info = info.Substring(0, 51) + "...";
                    Console.WriteLine($"| {info}".PadRight(54) + "|");
                    listingY++;
                }
            }

            Console.SetCursorPosition(listingX, listingY);
            Console.WriteLine("+------------------------------------------------------+");
        }

        private void DrawFinances()
        {
            int financeX = _rectangle.X + 70;
            int financeY = _rectangle.Y + 4;

            Console.SetCursorPosition(financeX, financeY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("+------------------- Finances -------------------+");
            financeY++;
            Console.SetCursorPosition(financeX, financeY);
            Console.WriteLine($"| Wages Paid This Week: £{_userClub.WagesPaidThisWeek:n0}".PadRight(48) + "|");
            financeY++;
            Console.SetCursorPosition(financeX, financeY);
            Console.WriteLine($"| Transfers Out: £{_userClub.TotalTransfersOut:n0}".PadRight(48) + "|");
            financeY++;
            Console.SetCursorPosition(financeX, financeY);
            Console.WriteLine($"| Transfers In: £{_userClub.TotalTransfersIn:n0}".PadRight(48) + "|");
            financeY++;
            Console.SetCursorPosition(financeX, financeY);
            Console.WriteLine($"| Total Profit (Transfers): £{_userClub.TotalProfit:n0}".PadRight(48) + "|");
            financeY++;
            Console.SetCursorPosition(financeX, financeY);
            Console.WriteLine("+------------------------------------------------+");
        }

        private void DrawRecentAndUpcomingFixtures()
        {
            int fixturesX = _rectangle.X + 70;
            int fixturesY = _rectangle.Y + 15;

            Console.SetCursorPosition(fixturesX, fixturesY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("+--------- Recent & Upcoming Fixtures ----------+");
            fixturesY++;

            // Recent
            var allClubFixtures = _league.Fixtures
                .Where(f => f.HomeTeam == _userClub || f.AwayTeam == _userClub)
                .OrderBy(f => f.Date)
                .ToList();

            var recentMatches = allClubFixtures
                .Where(f => f.Date <= _league.CurrentDate && f.Played)
                .OrderByDescending(f => f.Date)
                .Take(3)
                .OrderBy(f => f.Date)
                .ToList();

            if (recentMatches.Count == 0)
            {
                Console.SetCursorPosition(fixturesX, fixturesY);
                Console.WriteLine("| No recent results".PadRight(46) + "|");
                fixturesY++;
            }
            else
            {
                foreach (var fix in recentMatches)
                {
                    Console.SetCursorPosition(fixturesX, fixturesY);
                    string text = $"{fix.Date:dd MMM} ";
                    if (fix.HomeTeam == _userClub) text += "(H) ";
                    else text += "(A) ";

                    text += $"{fix.HomeTeam.Name} vs {fix.AwayTeam.Name} ";
                    text += $"({fix.HomeGoals}:{fix.AwayGoals})";

                    if (text.Length > 44)
                        text = text.Substring(0, 41) + "...";

                    Console.WriteLine($"| {text}".PadRight(46) + "|");
                    fixturesY++;
                }
            }

            // divider
            Console.SetCursorPosition(fixturesX, fixturesY);
            Console.WriteLine("|----------------------------------------------|");
            fixturesY++;

            // Upcoming
            var upcoming = allClubFixtures
                .Where(f => f.Date >= _league.CurrentDate && !f.Played)
                .OrderBy(f => f.Date)
                .Take(3)
                .ToList();

            if (upcoming.Count == 0)
            {
                Console.SetCursorPosition(fixturesX, fixturesY);
                Console.WriteLine("| No upcoming fixtures".PadRight(46) + "|");
                fixturesY++;
            }
            else
            {
                foreach (var fix in upcoming)
                {
                    Console.SetCursorPosition(fixturesX, fixturesY);
                    string text = $"{fix.Date:dd MMM} ";
                    if (fix.HomeTeam == _userClub) text += "(H) ";
                    else text += "(A) ";

                    text += $"{fix.HomeTeam.Name} vs {fix.AwayTeam.Name} - TBD";

                    if (text.Length > 44)
                        text = text.Substring(0, 41) + "...";

                    Console.WriteLine($"| {text}".PadRight(46) + "|");
                    fixturesY++;
                }
            }

            Console.SetCursorPosition(fixturesX, fixturesY);
            Console.WriteLine("+----------------------------------------------+");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FootballManager
{
    public class TransferMarketWindow : Window
    {
        private Club _userClub;
        private League _league;
        private List<Player> _transferListedPlayers;
        private int _selectedPlayerIndex;

        public TransferMarketWindow(Club userClub, League league, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _userClub = userClub;
            _league = league;
            _transferListedPlayers = new List<Player>();
            _selectedPlayerIndex = 0;
        }

        public override void Update()
        {
            var key = UserInterface.Input.Key;

            if (key == ConsoleKey.Escape)
            {
                // Return to main menu
                CurrentAction = InterfaceAction.ReturnToMainMenu;
                return;
            }

            if (key == ConsoleKey.UpArrow)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    _selectedPlayerIndex = (_selectedPlayerIndex - 1 + _transferListedPlayers.Count) % _transferListedPlayers.Count;
                }
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    _selectedPlayerIndex = (_selectedPlayerIndex + 1) % _transferListedPlayers.Count;
                }
            }
            else if (key == ConsoleKey.Enter)
            {
                if (_transferListedPlayers.Count > 0)
                {
                    var selectedPlayer = _transferListedPlayers[_selectedPlayerIndex];

                    // Check if the player belongs to the user's club
                    if (selectedPlayer.CurrentClub != _userClub)
                    {
                        // Attempt to buy player
                        BuyPlayer(selectedPlayer);
                    }
                    else
                    {
                        // Can't buy your own player
                        Console.Clear();
                        Console.WriteLine("You cannot buy your own player.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                    }
                }
            }
        }

        public override void Draw(bool active)
        {
            // Clear the window area only
            ClearWindowArea();

            base.Draw(active);

            // Refresh the list of transfer-listed players
            RefreshTransferListedPlayers();

            int x = _rectangle.X + 2;
            int y = _rectangle.Y + 4;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Transfer Market:");
            Console.ResetColor();
            y++;

            // Draw table headers
            Console.SetCursorPosition(x, y);
            Console.WriteLine("Index  Name                Age Pos  Rating  Value   Club              Status");
            y++;

            for (int i = 0; i < _transferListedPlayers.Count && y < _rectangle.Y + _rectangle.Height - 4; i++)
            {
                Console.SetCursorPosition(x, y);

                var player = _transferListedPlayers[i];
                string indexIndicator = (i == _selectedPlayerIndex) ? ">" : " ";

                if (i == _selectedPlayerIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                // Display player details with value in 'M'
                Console.WriteLine($"{indexIndicator} {i + 1,-5} {player.Name,-18} {player.Age,-3} {player.Position,-4} {player.Rating,-7} £{player.Value}M  {player.CurrentClub.Name,-16} {player.SquadStatus}");

                Console.ResetColor();
                y++;
            }

            if (_transferListedPlayers.Count == 0)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine("No players are currently listed for transfer.");
            }

            // Instructions
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Use Up/Down arrows to navigate, Enter to buy, ESC to return.");
        }

        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }

        private void RefreshTransferListedPlayers()
        {
            // Include players from the user's club but prevent buying them
            _transferListedPlayers = _league.GetAllPlayers()
                .Where(p => p.AvailableForTransfer)
                .ToList();

            if (_selectedPlayerIndex >= _transferListedPlayers.Count)
            {
                _selectedPlayerIndex = 0;
            }
        }

        private void BuyPlayer(Player player)
        {
            Console.Clear();
            Console.WriteLine($"Do you want to buy {player.Name} from {player.CurrentClub.Name} for £{player.TransferPrice}M? (Y/N)");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Y)
            {
                double transferFee = player.TransferPrice * 1_000_000; // Convert to actual amount

                if (_userClub.Balance >= transferFee)
                {
                    // Transfer the player
                    player.CurrentClub.Players.Remove(player);
                    _userClub.Players.Add(player);

                    // Update balances
                    _userClub.Balance -= transferFee;
                    player.CurrentClub.Balance += transferFee;

                    // Update transfer totals
                    _userClub.TotalTransfersIn += transferFee;
                    player.CurrentClub.TotalTransfersOut += transferFee;

                    // Update player's club
                    player.CurrentClub = _userClub;
                    player.AvailableForTransfer = false;
                    player.TransferPrice = 0;

                    // Prompt for new contract details
                    SetNewPlayerContractDetails(player);

                    Console.WriteLine($"{player.Name} has joined {_userClub.Name}.");
                }
                else
                {
                    Console.WriteLine("Not enough funds to complete the transfer.");
                }
            }
            else
            {
                Console.WriteLine("Transfer canceled.");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private void SetNewPlayerContractDetails(Player player)
        {
            Console.Clear();
            Console.WriteLine($"Set contract details for {player.Name}:");

            // Set Squad Status
            Console.WriteLine("Select squad status:");
            Console.WriteLine("1. First Team Member");
            Console.WriteLine("2. Backup Player");
            Console.WriteLine("3. Youngster");
            var statusKey = Console.ReadKey(true).KeyChar;
            switch (statusKey)
            {
                case '1':
                    player.SquadStatus = "First Team Member";
                    break;
                case '2':
                    player.SquadStatus = "Backup Player";
                    break;
                case '3':
                    player.SquadStatus = "Youngster";
                    break;
                default:
                    Console.WriteLine("Invalid selection. Defaulting to 'Backup Player'.");
                    player.SquadStatus = "Backup Player";
                    break;
            }

            // Set Contract Length
            Console.WriteLine("Enter contract length in years:");
            string lengthInput = Console.ReadLine();
            if (int.TryParse(lengthInput, out int newLength))
            {
                player.ContractLength = newLength;
            }
            else
            {
                Console.WriteLine("Invalid input. Defaulting to 3 years.");
                player.ContractLength = 3;
            }

            // Set Wage
            Console.WriteLine("Enter wage in K per week:");
            string wageInput = Console.ReadLine();
            if (double.TryParse(wageInput, out double newWage))
            {
                player.Wage = newWage;
            }
            else
            {
                Console.WriteLine("Invalid input. Defaulting to £50K per week.");
                player.Wage = 50;
            }

            Console.WriteLine("Contract details have been set.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;

namespace FootballManager
{
    public class PlayersWindow : Window
    {
        private Club _userClub;
        private int _selectedPlayerIndex;
        private bool _viewingPlayerDetails;

        public PlayersWindow(Club userClub, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _userClub = userClub;
            _selectedPlayerIndex = 0;
            _viewingPlayerDetails = false;
        }

        public override void Update()
        {
            var key = UserInterface.Input.Key;

            if (key == ConsoleKey.Escape)
            {
                if (_viewingPlayerDetails)
                {
                    _viewingPlayerDetails = false;
                }
                else
                {
                    // Return to main menu
                    CurrentAction = InterfaceAction.ReturnToMainMenu;
                    return;
                }
            }

            if (_userClub.Players.Count == 0)
                return;

            if (_viewingPlayerDetails)
            {
                if (key == ConsoleKey.LeftArrow)
                {
                    _viewingPlayerDetails = false;
                }
                else if (key == ConsoleKey.T)
                {
                    ToggleTransferStatus(_userClub.Players[_selectedPlayerIndex]);
                }
            }
            else
            {
                if (key == ConsoleKey.UpArrow)
                {
                    _selectedPlayerIndex = (_selectedPlayerIndex - 1 + _userClub.Players.Count) % _userClub.Players.Count;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    _selectedPlayerIndex = (_selectedPlayerIndex + 1) % _userClub.Players.Count;
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    _viewingPlayerDetails = true;
                }
                else if (key == ConsoleKey.T)
                {
                    ToggleTransferStatus(_userClub.Players[_selectedPlayerIndex]);
                }
            }
        }

        public override void Draw(bool active)
        {
            // Clear the window area only
            ClearWindowArea();

            base.Draw(active);

            int x = _rectangle.X + 2;
            int y = _rectangle.Y + 4;

            if (_viewingPlayerDetails)
            {
                DrawPlayerDetails(x, y);
            }
            else
            {
                DrawPlayerList(x, y);
            }

            // Instructions
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Use Up/Down arrows to navigate, Right Arrow to view details, T to toggle transfer status, ESC to return.");
        }

        private void DrawPlayerList(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Your Players:");
            Console.ResetColor();
            y++;

            for (int i = 0; i < _userClub.Players.Count && y < _rectangle.Y + _rectangle.Height - 4; i++)
            {
                Console.SetCursorPosition(x, y);

                var player = _userClub.Players[i];

                if (i == _selectedPlayerIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {player.Name} ({player.Position}) - Rating: {player.Rating} - Transfer Listed: {(player.AvailableForTransfer ? "Yes" : "No")}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {player.Name} ({player.Position}) - Rating: {player.Rating} - Transfer Listed: {(player.AvailableForTransfer ? "Yes" : "No")}");
                }
                y++;
            }

            if (_userClub.Players.Count == 0)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine("| No players available.".PadRight(_rectangle.Width - 2) + "|");
                y++;
            }
        }

        private void DrawPlayerDetails(int x, int y)
        {
            var player = _userClub.Players[_selectedPlayerIndex];

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Name: {player.Name}");
            Console.ResetColor();
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Age: {player.Age}");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Positions: {player.Position}");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Rating: {player.Rating}");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Potential: {player.Potential}");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Value: £{player.Value}M");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Wage: £{player.Wage}K / Week");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Contract Length: {player.ContractLength} Years Left");
            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Squad Status: {player.SquadStatus}");
            y++;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Player Statistics:");
            Console.ResetColor();
            y++;

            foreach (var stat in player.Statistics)
            {
                Console.SetCursorPosition(x, y);
                Console.WriteLine($"- {stat.Key}: {stat.Value}");
                y++;
            }

            // Instructions
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Press Left Arrow to go back, T to toggle transfer status, ESC to return.");
        }

        private void ToggleTransferStatus(Player player)
        {
            player.AvailableForTransfer = !player.AvailableForTransfer;

            if (player.AvailableForTransfer)
            {
                // Prompt for transfer price
                Console.Clear();
                Console.WriteLine($"Enter transfer price for {player.Name} in millions:");
                string input = Console.ReadLine();

                if (double.TryParse(input, out double price))
                {
                    player.TransferPrice = price;
                    Console.WriteLine($"{player.Name} is now listed for £{price}M.");
                }
                else
                {
                    player.AvailableForTransfer = false;
                    Console.WriteLine("Invalid price entered. Player not listed.");
                }
            }
            else
            {
                player.TransferPrice = 0;
                Console.WriteLine($"{player.Name} is no longer listed for transfer.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }
    }
}

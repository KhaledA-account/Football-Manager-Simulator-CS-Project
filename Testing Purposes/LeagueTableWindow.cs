using System;
using System.Drawing;
using System.Linq;

namespace FootballManager
{
    public class LeagueTableWindow : Window
    {
        private League _league;
        private Club _userTeam;

        public LeagueTableWindow(League league, string title, Rectangle rectangle, bool visible, Club userTeam)
            : base(title, rectangle, visible)
        {
            _league = league;
            _userTeam = userTeam;
        }

        public override void Update()
        {
            var key = UserInterface.Input.Key;

            if (key == ConsoleKey.Escape)
            {
                // Return to main menu
                CurrentAction = InterfaceAction.ReturnToMainMenu;
            }
        }

        public override void Draw(bool active)
        {
            // Clear the window area only
            ClearWindowArea();

            base.Draw(active);

            int x = Math.Min(_rectangle.X + 2, Console.WindowWidth - 1);
            int y = Math.Min(_rectangle.Y + 4, Console.WindowHeight - 1);

            Console.SetCursorPosition(x, y);
            Console.WriteLine("League Table:");

            y++;
            Console.SetCursorPosition(x, y);
            Console.WriteLine("Pos Club                      P   W   D   L   GF  GA  GD  Pts");
            y++;

            var sortedClubs = _league.Clubs
                .OrderByDescending(c => c.Stats.Points)
                .ThenByDescending(c => c.Stats.GoalDifference)
                .ThenByDescending(c => c.Stats.GoalsFor)
                .ToList();

            for (int i = 0; i < sortedClubs.Count; i++)
            {
                var club = sortedClubs[i];
                Console.SetCursorPosition(x, y);

                if (club == _userTeam)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }

                Console.WriteLine($"{i + 1,-3} {club.Name,-25} {club.Stats.Played,-3} {club.Stats.Wins,-3} {club.Stats.Draws,-3} {club.Stats.Losses,-3} {club.Stats.GoalsFor,-3} {club.Stats.GoalsAgainst,-3} {club.Stats.GoalDifference,-3} {club.Stats.Points,-3}");

                Console.ResetColor();
                y++;
            }

            // Instructions
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Press ESC to return to the main menu.");
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

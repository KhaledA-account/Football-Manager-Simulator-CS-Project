using System;
using System.Drawing;
using System.Linq;

namespace FootballManager
{
    public class FixturesWindow : Window
    {
        private League _league;
        private Club _userClub;
        private int _scrollOffset;
        private int _visibleLines;

        public FixturesWindow(League league, Club userClub, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _league = league;
            _userClub = userClub;
            _scrollOffset = 0;
            _visibleLines = rectangle.Height - 8; // Adjusted for header and footer
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
            else if (key == ConsoleKey.UpArrow)
            {
                if (_scrollOffset > 0)
                    _scrollOffset--;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                _scrollOffset++;
            }
        }

        public override void Draw(bool active)
        {
            // Clear the window area only
            ClearWindowArea();

            base.Draw(active);

            int x = _rectangle.X + 2;
            int y = _rectangle.Y + 4;

            Console.SetCursorPosition(x, y);
            Console.WriteLine("Your Club's Fixtures:");
            y++;

            var fixtures = _league.Fixtures
                .Where(f => f.HomeTeam == _userClub || f.AwayTeam == _userClub)
                .OrderBy(f => f.Date)
                .ToList();

            // Handle scrolling
            int totalFixtures = fixtures.Count;
            int maxOffset = Math.Max(0, totalFixtures - _visibleLines);
            if (_scrollOffset > maxOffset)
                _scrollOffset = maxOffset;
            if (_scrollOffset < 0)
                _scrollOffset = 0;

            fixtures = fixtures.Skip(_scrollOffset).Take(_visibleLines).ToList();

            foreach (var fixture in fixtures)
            {
                Console.SetCursorPosition(x, y);
                string fixtureText = $"{fixture.Date:dd MMMM yyyy} ";

                if (fixture.HomeTeam == _userClub)
                {
                    fixtureText += "(Home) - ";
                }
                else if (fixture.AwayTeam == _userClub)
                {
                    fixtureText += "(Away) - ";
                }

                fixtureText += $"{fixture.HomeTeam.Name} vs {fixture.AwayTeam.Name}";

                if (fixture.Played)
                {
                    fixtureText += $" - {fixture.HomeGoals} : {fixture.AwayGoals}";
                }
                else
                {
                    fixtureText += " - TBD";
                }

                Console.WriteLine(fixtureText);
                y++;

                if (y >= _rectangle.Y + _rectangle.Height - 2)
                    break;
            }

            // Instructions
            Console.SetCursorPosition(x, _rectangle.Y + _rectangle.Height - 2);
            Console.WriteLine("Use Up/Down arrows to scroll, ESC to return to the main menu.");
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

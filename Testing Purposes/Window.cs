using System;
using System.Drawing;

namespace FootballManager
{
    public enum InterfaceAction
    {
        None,
        ViewTable,
        ViewPlayers,
        ManageSquad,
        ViewMatches,
        ViewTransfers,
        ViewFixtures,
        MatchSimulation,
        ReturnToMainMenu,
        Exit
    }

    public abstract class Window
    {
        protected string _title;
        protected Rectangle _rectangle;
        protected bool _visible;
        public InterfaceAction CurrentAction { get; set; }

        protected Window(string title, Rectangle rectangle, bool visible)
        {
            _title = title;
            _rectangle = rectangle;
            _visible = visible;
            CurrentAction = InterfaceAction.None;
        }

        public virtual void Update()
        {
            // Base update method
        }

        public virtual void Draw(bool active)
        {
            if (!_visible)
                return;

            // Draw window borders and title with colors
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y);
            Console.Write("╔" + new string('═', _rectangle.Width - 2) + "╗");
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y + 1);
            Console.Write("║" + _title.PadRight(_rectangle.Width - 2) + "║");
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y + 2);
            Console.Write("╠" + new string('═', _rectangle.Width - 2) + "╣");
            Console.ResetColor();
        }

        public void SetVisibility(bool visible)
        {
            _visible = visible;
        }
    }
}

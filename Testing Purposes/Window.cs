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
        private string cachedBorder;

        protected Window(string title, Rectangle rectangle, bool visible)
        {
            _title = title;
            _rectangle = rectangle;
            _visible = visible;
            CurrentAction = InterfaceAction.None;
            BuildCachedBorder();
        }

        private void BuildCachedBorder()
        {
            var top = "╔" + new string('═', _rectangle.Width - 2) + "╗";
            var titleLine = "║" + _title.PadRight(_rectangle.Width - 2) + "║";
            var separator = "╠" + new string('═', _rectangle.Width - 2) + "╣";
            cachedBorder = top + "\n" + titleLine + "\n" + separator;
        }

        // Added virtual Update() method so derived classes can override it.
        public virtual void Update()
        {
            // Empty base implementation.
        }

        public virtual void Draw(bool active)
        {
            if (!_visible)
                return;

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(_rectangle.X, _rectangle.Y);
            Console.Write(cachedBorder);
            Console.ResetColor();
        }

        public void SetVisibility(bool visible)
        {
            _visible = visible;
        }
    }
}

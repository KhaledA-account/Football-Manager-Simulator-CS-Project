using System;

namespace FootballManager
{
    // ------------------- Enums -------------------
    public enum MainMenuSection
    {
        LiveMatch,
        MatchSettings
    }

    public enum MatchSettingsSubMenu
    {
        None,
        GameSpeed,
        FormationChange,
        ModifyTactics,
        PlayerRoles
    }

    // ------------------- ConsoleCell Struct -------------------
    public struct ConsoleCell
    {
        public char Character;
        public ConsoleColor ForegroundColor;
        public ConsoleColor BackgroundColor;

        public ConsoleCell(char character, ConsoleColor foreground, ConsoleColor background)
        {
            Character = character;
            ForegroundColor = foreground;
            BackgroundColor = background;
        }

        public static ConsoleCell Empty => new ConsoleCell(' ', ConsoleColor.Black, ConsoleColor.Black);
    }

    // ------------------- Box Class -------------------
    public class Box
    {
        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }
        public string Title { get; }

        public Box(int left, int top, int width, int height, string title)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Title = title;
        }
    }

    // ------------------- SimPlayer Class -------------------
    public class SimPlayer
    {
        public string Name { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public bool HasBall { get; set; }
        public bool IsGoalkeeper { get; set; }
        public double SkillLevel { get; set; }

        public SimPlayer(string name, int x, int y, bool isGoalkeeper = false, double skillLevel = 0.5)
        {
            Name = name;
            XPosition = x;
            YPosition = y;
            HasBall = false;
            IsGoalkeeper = isGoalkeeper;
            SkillLevel = skillLevel;
        }
    }
}

using System;

namespace FootballManager
{
    public static class UserInterface
    {
        public static InputState Input { get; private set; } = new InputState();

        public static void ReadInput()
        {
            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);
                Input.Key = keyInfo.Key;
                Input.KeyInfo = keyInfo;
            }
            else
            {
                Input.Key = ConsoleKey.NoName;
                Input.KeyInfo = default(ConsoleKeyInfo);
            }
        }
    }

    public class InputState
    {
        public ConsoleKey Key { get; set; }
        public ConsoleKeyInfo KeyInfo { get; set; }
    }
}

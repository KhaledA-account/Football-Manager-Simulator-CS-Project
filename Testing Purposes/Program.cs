using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace FootballManager
{
    class Program
    {
        private static List<Window> windows = new List<Window>();

        static void Main(string[] args)
        {
            // Set console colors and sizes
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear(); // Apply background color to entire console

            try
            {
                Console.SetWindowSize(120, 40);
                Console.SetBufferSize(120, 1000);
            }
            catch
            {
                // Ignore if not supported
            }

            Console.ResetColor();
            Console.CursorVisible = false;

            // Create the league (ensure League.cs and other required files are in the project)
            League league = new League();

            // Club selection screen
            int selectedClubIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Select your club:\n");

                for (int i = 0; i < league.Clubs.Count; i++)
                {
                    if (i == selectedClubIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {league.Clubs[i].Name}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {league.Clubs[i].Name}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedClubIndex = (selectedClubIndex - 1 + league.Clubs.Count) % league.Clubs.Count;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedClubIndex = (selectedClubIndex + 1) % league.Clubs.Count;
                }

            } while (key != ConsoleKey.Enter);

            Club userClub = league.Clubs[selectedClubIndex];

            // Initialize all game windows
            MainMenuWindow mainMenu = new MainMenuWindow(userClub, league, "Main Menu", new Rectangle(0, 0, 120, 40), true);
            LeagueTableWindow leagueTableWindow = new LeagueTableWindow(league, "League Table", new Rectangle(0, 0, 120, 40), false, userClub);
            SquadManagementWindow squadManagementWindow = new SquadManagementWindow(userClub, "Squad Management", new Rectangle(0, 0, 120, 40), false);
            PlayersWindow playersWindow = new PlayersWindow(userClub, "Players", new Rectangle(0, 0, 120, 40), false);
            TransferMarketWindow transferMarketWindow = new TransferMarketWindow(userClub, league, "Transfer Market", new Rectangle(0, 0, 120, 40), false);
            FixturesWindow fixturesWindow = new FixturesWindow(league, userClub, "Fixtures", new Rectangle(0, 0, 120, 40), false);

            windows.Add(mainMenu);
            windows.Add(leagueTableWindow);
            windows.Add(squadManagementWindow);
            windows.Add(playersWindow);
            windows.Add(transferMarketWindow);
            windows.Add(fixturesWindow);

            Window currentWindow = mainMenu;

            // Main game loop (approximately 30 FPS)
            while (true)
            {
                UserInterface.ReadInput();
                currentWindow.Update();
                currentWindow.Draw(true);

                if (currentWindow.CurrentAction != InterfaceAction.None)
                {
                    // Hide all windows
                    foreach (var window in windows)
                    {
                        window.SetVisibility(false);
                    }

                    // Switch to the appropriate window based on the action
                    switch (currentWindow.CurrentAction)
                    {
                        case InterfaceAction.ViewTable:
                            currentWindow = leagueTableWindow;
                            break;
                        case InterfaceAction.ViewPlayers:
                            currentWindow = playersWindow;
                            break;
                        case InterfaceAction.ManageSquad:
                            currentWindow = squadManagementWindow;
                            break;
                        case InterfaceAction.ViewTransfers:
                            currentWindow = transferMarketWindow;
                            break;
                        case InterfaceAction.ViewFixtures:
                            currentWindow = fixturesWindow;
                            break;
                        case InterfaceAction.ReturnToMainMenu:
                            currentWindow = mainMenu;
                            break;
                        case InterfaceAction.Exit:
                            return;
                    }

                    currentWindow.SetVisibility(true);
                    currentWindow.CurrentAction = InterfaceAction.None;
                }

                // Sleep for approximately 33ms (~30 FPS) for smooth updates
                Thread.Sleep(30);
            }
        }

        // Called to start the live match simulation
        public static void StartNoNullSimulation(Fixture fixture, League league, Club userClub)
        {
            var simulation = new LiveMatchSimulation(fixture, league, userClub);
            simulation.StartSimulation();
        }

        public static void AddWindow(Window window)
        {
            windows.Add(window);
        }
    }
}

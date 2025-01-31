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

            // Try resizing
            try
            {
                Console.SetWindowSize(120, 40);
                Console.SetBufferSize(120, 1000);
            }
            catch
            {
                // ignore if not supported
            }

            Console.ResetColor();

            League league = new League();

            // Club selection
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

            // Initialize windows
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

            while (true)
            {
                UserInterface.ReadInput();
                currentWindow.Update();
                currentWindow.Draw(true);

                // Handle window switching
                if (currentWindow.CurrentAction != InterfaceAction.None)
                {
                    // Hide all windows
                    foreach (var window in windows)
                    {
                        window.SetVisibility(false);
                    }

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

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Previously referenced "NoNullFootballSimulation". 
        /// We now call the new "LiveMatchSimulation" class instead.
        /// </summary>
        public static void StartNoNullSimulation(Fixture fixture, League league, Club userClub)
        {
            // Launch the partial-based "LiveMatchSimulation"
            var simulation = new LiveMatchSimulation(fixture, league, userClub);
            simulation.StartSimulation();
            // Once done, the final result is in 'fixture' and the league table is updated.
        }

        public static void AddWindow(Window window)
        {
            windows.Add(window);
        }
    }
}

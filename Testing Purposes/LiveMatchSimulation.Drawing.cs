using System;
using System.Collections.Generic;

namespace FootballManager
{
    // This file has the PARTIAL class with the drawing logic for the match simulation
    public partial class LiveMatchSimulation
    {
        // -------------------------------------------------------------
        // Rendering Methods
        // -------------------------------------------------------------

        /// <summary>
        /// Renders all UI components to the buffer and updates the console.
        /// </summary>
        private void RenderAll()
        {
            ClearBuffer();

            DrawBox(liveMatchBox, "Live Match", true);
            DrawBox(settingsBox, "Match Settings", true);
            DrawBox(informationBox, "Match Information", true);
            DrawBox(benchBox, "Bench", true);
            DrawBox(startingBox, "Starting 11", true);

            DrawMatchInformation();
            DrawBench();
            DrawStartingPlayers();
            DrawMatchSettingsMenu();

            DrawPitch();

            RenderBufferToConsole();
        }

        /// <summary>
        /// Clears the buffer before rendering.
        /// </summary>
        private void ClearBuffer()
        {
            for (int row = 0; row < ConsoleHeight; row++)
            {
                for (int col = 0; col < ConsoleWidth; col++)
                {
                    buffer[row, col] = ConsoleCell.Empty;
                }
            }
        }

        /// <summary>
        /// Draws a UI box with borders and an optional title.
        /// </summary>
        /// <param name="box">The Box object defining position and size.</param>
        /// <param name="title">Title of the box.</param>
        /// <param name="includeTitle">Whether to include the title.</param>
        private void DrawBox(Box box, string title, bool includeTitle)
        {
            for (int y = 0; y < box.Height; y++)
            {
                for (int x = 0; x < box.Width; x++)
                {
                    int absoluteX = box.Left + x;
                    int absoluteY = box.Top + y;

                    if (y == 0 || y == box.Height - 1)
                    {
                        PlaceChar(absoluteX, absoluteY, '-', ConsoleColor.White, ConsoleColor.Black);
                    }
                    else if (x == 0 || x == box.Width - 1)
                    {
                        PlaceChar(absoluteX, absoluteY, '|', ConsoleColor.White, ConsoleColor.Black);
                    }
                    else
                    {
                        PlaceChar(absoluteX, absoluteY, ' ', ConsoleColor.Black, ConsoleColor.Black);
                    }
                }
            }

            if (includeTitle && !string.IsNullOrEmpty(title))
            {
                PlaceString(box.Left + 2, box.Top, title, ConsoleColor.White, ConsoleColor.Black);
            }
        }

        /// <summary>
        /// Draws the football pitch, including boundaries, midfield marker, goals, and players.
        /// </summary>
        private void DrawPitch()
        {
            // Draw pitch boundaries
            for (int y = pitchTop; y <= pitchBottom; y++)
            {
                PlaceChar(pitchLeft, y, '|', ConsoleColor.Green, ConsoleColor.Green);
                PlaceChar(pitchRight, y, '|', ConsoleColor.Green, ConsoleColor.Green);
            }

            for (int x = pitchLeft; x <= pitchRight; x++)
            {
                PlaceChar(x, pitchTop, '-', ConsoleColor.Green, ConsoleColor.Green);
                PlaceChar(x, pitchBottom, '-', ConsoleColor.Green, ConsoleColor.Green);
            }

            // Draw midfield circle or marker
            int midfieldX = (pitchLeft + pitchRight) / 2;
            int centerY = (pitchTop + pitchBottom) / 2;
            for (int dy = -3; dy <= 3; dy++)
            {
                for (int dx = -3; dx <= 3; dx++)
                {
                    PlaceChar(midfieldX + dx, centerY + dy, ' ', ConsoleColor.White, ConsoleColor.White);
                }
            }
            PlaceChar(midfieldX, centerY, ' ', ConsoleColor.Green, ConsoleColor.Green);

            DrawGoals(midfieldX, centerY);
            DrawPlayers(teamAPlayers, ConsoleColor.Magenta);
            DrawPlayers(teamBPlayers, ConsoleColor.Cyan);
        }

        /// <summary>
        /// Draws the goals on both ends of the pitch.
        /// </summary>
        private void DrawGoals(int midfieldX, int centerY)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                PlaceChar(pitchLeft, centerY + dy, ' ', ConsoleColor.DarkGray, ConsoleColor.DarkGray);
                PlaceChar(pitchLeft + 1, centerY + dy, ' ', ConsoleColor.DarkGray, ConsoleColor.DarkGray);
                PlaceChar(pitchRight - 1, centerY + dy, ' ', ConsoleColor.DarkGray, ConsoleColor.DarkGray);
                PlaceChar(pitchRight, centerY + dy, ' ', ConsoleColor.DarkGray, ConsoleColor.DarkGray);
            }
        }

        /// <summary>
        /// Draws players on the pitch with appropriate colors.
        /// </summary>
        /// <param name="team">List of players in the team.</param>
        /// <param name="teamColor">Color representing the team.</param>
        private void DrawPlayers(List<SimPlayer> team, ConsoleColor teamColor)
        {
            foreach (var p in team)
            {
                ConsoleColor color = teamColor;
                if (p.IsGoalkeeper) color = ConsoleColor.DarkYellow;
                if (p.HasBall) color = ConsoleColor.Red;
                PlaceChar(p.XPosition, p.YPosition, ' ', color, color);
            }
        }

        /// <summary>
        /// Draws the match settings menu.
        /// </summary>
        private void DrawMatchSettingsMenu()
        {
            if (IsMatchSettingsActive())
            {
                int sx = settingsBox.Left + 1;
                int sy = settingsBox.Top + 1;
                for (int i = 0; i < settingsOptions.Count; i++)
                {
                    ConsoleColor c = (i == settingsMenuIndex) ? ConsoleColor.Yellow : ConsoleColor.Gray;
                    string menuItem = $"{i + 1}) {settingsOptions[i]}";
                    PlaceString(sx, sy + (2 * i), menuItem, c, ConsoleColor.Black);
                }
            }
        }

        /// <summary>
        /// Draws match information such as time and score.
        /// </summary>
        private void DrawMatchInformation()
        {
            int sx = informationBox.Left + 1;
            int sy = informationBox.Top + 1;
            PlaceString(sx, sy, $"Time: {currentMinute}' Minutes", ConsoleColor.White, ConsoleColor.Black);
            sy++;
            PlaceString(sx, sy, "Team A vs Team B", ConsoleColor.White, ConsoleColor.Black);
            sy++;
            PlaceString(sx, sy, $"Score: {teamAScore} - {teamBScore}", ConsoleColor.White, ConsoleColor.Black);
        }

        /// <summary>
        /// Draws bench players.
        /// </summary>
        private void DrawBench()
        {
            int sx = benchBox.Left + 1;
            int sy = benchBox.Top + 1;

            var benchPlayersInfo = new List<string>
            {
                "GK - Player A 93%",
                "RB - Player B 90%",
                "LB - Player C 89%",
                "CB - Player D 85%",
                "CM - Player E 91%",
                "ST - Player F 95%"
            };

            for (int i = 0; i < benchPlayersInfo.Count && i < 8; i++)
            {
                PlaceString(sx, sy + i, benchPlayersInfo[i], ConsoleColor.White, ConsoleColor.Black);
            }
        }

        /// <summary>
        /// Draws starting players.
        /// </summary>
        private void DrawStartingPlayers()
        {
            int sx = startingBox.Left + 1;
            int sy = startingBox.Top + 1;

            var startingPlayersInfo = new List<string>
            {
                "GK - Player G 93%",
                "RB - Player H 90%",
                "LB - Player I 89%",
                "CB - Player J 85%",
                "CM - Player K 91%",
                "ST - Player L 95%"
            };

            for (int i = 0; i < startingPlayersInfo.Count; i++)
            {
                PlaceString(sx, sy + i, startingPlayersInfo[i], ConsoleColor.White, ConsoleColor.Black);
            }
        }

        /// <summary>
        /// Places a single character on the buffer.
        /// </summary>
        private void PlaceChar(int x, int y, char ch, ConsoleColor fg, ConsoleColor bg)
        {
            if (x < 0 || x >= ConsoleWidth || y < 0 || y >= ConsoleHeight) return;
            buffer[y, x] = new ConsoleCell(ch, fg, bg);
        }

        /// <summary>
        /// Places a string on the buffer starting at (x, y).
        /// </summary>
        private void PlaceString(int x, int y, string text, ConsoleColor fg, ConsoleColor bg)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int cx = x + i;
                if (cx < 0 || cx >= ConsoleWidth || y < 0 || y >= ConsoleHeight) break;
                buffer[y, cx] = new ConsoleCell(text[i], fg, bg);
            }
        }

        /// <summary>
        /// Renders the buffer to the console.
        /// </summary>
        private void RenderBufferToConsole()
        {
            Console.SetCursorPosition(0, 0);
            for (int row = 0; row < ConsoleHeight; row++)
            {
                for (int col = 0; col < ConsoleWidth; col++)
                {
                    var cell = buffer[row, col];
                    Console.ForegroundColor = cell.ForegroundColor;
                    Console.BackgroundColor = cell.BackgroundColor;
                    Console.Write(cell.Character);
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);
        }

        /// <summary>
        /// Draws the ball at a specific position on the pitch.
        /// </summary>
        /// <param name="x">X position of the ball.</param>
        /// <param name="y">Y position of the ball.</param>
        private void DrawBallAt(int x, int y)
        {
            RenderAll();
            if (IsPositionWithinPitch(x, y))
            {
                Console.SetCursorPosition(x, y);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write('o');
                Console.ResetColor();
                Console.SetCursorPosition(0, 0);
            }
        }

        // You can add more drawing methods as needed
    }
}

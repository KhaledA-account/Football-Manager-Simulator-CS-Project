using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace FootballManager
{
    // This file has the PARTIAL class with the core logic for the match simulation
    public partial class LiveMatchSimulation
    {
        // --- Fields referencing your real game classes ---
        private Fixture _fixture;
        private League _league;
        private Club _userClub;

        // --- Console / UI dimensions ---
        private const int ConsoleWidth = 150;
        private const int ConsoleHeight = 40;

        // We'll create this array in the constructor; the drawing half will use it
        private ConsoleCell[,] buffer;

        // --- UI boxes (defined in the constructor) ---
        private Box liveMatchBox;
        private Box settingsBox;
        private Box informationBox;
        private Box benchBox;
        private Box startingBox;

        // --- Pitch boundaries ---
        private int pitchLeft;
        private int pitchTop;
        private int pitchRight;
        private int pitchBottom;

        // --- Navigation ---
        private MainMenuSection currentMainSection;
        private MatchSettingsSubMenu currentSubMenu;

        // --- States ---
        private bool isRunning;
        private bool isPaused;
        private bool hasMatchEnded;

        // --- Match details ---
        private bool isFirstHalf;
        private bool isTeamAAttackingRight;

        private int currentMinute;
        private int gameSpeed;
        private int teamAScore;
        private int teamBScore;

        private int settingsMenuIndex;
        private List<string> settingsOptions;

        // --- Player lists for each team ---
        private List<SimPlayer> teamAPlayers;
        private List<SimPlayer> teamBPlayers;

        private Random randomGenerator;

        // -------------------------------------------------------------
        // Constructor(s)
        // -------------------------------------------------------------
        public LiveMatchSimulation(Fixture fixture, League league, Club userClub)
        {
            _fixture = fixture;
            _league = league;
            _userClub = userClub;
            InitializeSimulation();
        }

        // Optional parameterless constructor
        public LiveMatchSimulation()
        {
            InitializeSimulation();
        }

        // -------------------------------------------------------------
        // Initialization
        // -------------------------------------------------------------
        private void InitializeSimulation()
        {
            currentMainSection = MainMenuSection.LiveMatch;
            currentSubMenu = MatchSettingsSubMenu.None;
            isRunning = true;
            isPaused = false;
            hasMatchEnded = false;

            isFirstHalf = true;
            isTeamAAttackingRight = true;

            currentMinute = 0;
            gameSpeed = 100; // 1..300 range
            teamAScore = 0;
            teamBScore = 0;

            settingsMenuIndex = 0;
            settingsOptions = new List<string>
            {
                "Game Speed",
                "Formation Change",
                "Modify Tactics",
                "Player Roles"
            };

            InitializeBoxes();
            InitializeBuffer();
            InitializePitch();
            InitializeTeams();
        }

        private void InitializeBoxes()
        {
            liveMatchBox = new Box(0, 0, 80, 14, "Live Match");
            settingsBox = new Box(80, 0, 40, 14, "Match Settings");
            informationBox = new Box(0, 14, 50, 6, "Match Information");
            benchBox = new Box(0, 20, 50, 9, "Bench");
            startingBox = new Box(50, 14, 70, 15, "Starting 11");
        }

        private void InitializeBuffer()
        {
            buffer = new ConsoleCell[ConsoleHeight, ConsoleWidth];
            for (int row = 0; row < ConsoleHeight; row++)
            {
                for (int col = 0; col < ConsoleWidth; col++)
                {
                    buffer[row, col] = ConsoleCell.Empty;
                }
            }
        }

        private void InitializePitch()
        {
            pitchLeft = liveMatchBox.Left + 1;
            pitchTop = liveMatchBox.Top + 1;
            pitchRight = liveMatchBox.Left + liveMatchBox.Width - 2;
            pitchBottom = liveMatchBox.Top + liveMatchBox.Height - 2;
        }

        private void InitializeTeams()
        {
            randomGenerator = new Random();

            // Initialize Team A Players
            teamAPlayers = new List<SimPlayer>
            {
                new SimPlayer("A1", 10, 5, true, 0.6),
                new SimPlayer("A2", 15, 8, false, 0.7),
                // Add more players as needed
            };

            // Initialize Team B Players
            teamBPlayers = new List<SimPlayer>
            {
                new SimPlayer("B1", 70, 5, true, 0.6),
                new SimPlayer("B2", 65, 8, false, 0.7),
                // Add more players as needed
            };

            // Assign the ball to a player (e.g., the first player of Team A)
            if (teamAPlayers.Count > 0)
            {
                AssignBallToPlayer(teamAPlayers[0]);
            }
        }

        // -------------------------------------------------------------
        // Public entry point for the simulation
        // -------------------------------------------------------------
        public void StartSimulation()
        {
            try
            {
                try
                {
                    Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
                    Console.SetBufferSize(ConsoleWidth, ConsoleHeight);
                }
                catch
                {
                    // Some terminals disallow resizing
                }

                Console.CursorVisible = false;

                RenderAll();

                while (isRunning)
                {
                    if (!isPaused && !hasMatchEnded && IsLiveMatchActive())
                    {
                        SimulateTick();
                    }

                    HandleUserInput();
                    RenderAll();
                    ControlFrameRate();
                }

                EndMatch();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.CursorVisible = true;
            }
        }

        // -------------------------------------------------------------
        // Simulation logic methods (no direct console drawing)
        // -------------------------------------------------------------
        private bool IsMatchSettingsActive()
        {
            return (currentMainSection == MainMenuSection.MatchSettings &&
                    currentSubMenu == MatchSettingsSubMenu.None);
        }

        private bool IsLiveMatchActive()
        {
            return (currentMainSection == MainMenuSection.LiveMatch &&
                    currentSubMenu == MatchSettingsSubMenu.None);
        }

        private void SimulateTick()
        {
            if (currentMinute >= 90)
            {
                hasMatchEnded = true;
                return;
            }

            if (isFirstHalf && currentMinute >= 45)
            {
                HandleHalfTime();
            }

            currentMinute++;

            bool[,] occupiedPositions = GetOccupiedPositions();

            var ballHolder = GetBallHolder();
            int ballX = -1;
            int ballY = -1;
            if (ballHolder != null)
            {
                ballX = ballHolder.XPosition;
                ballY = ballHolder.YPosition;
            }

            UpdateTeamPositions(teamAPlayers, occupiedPositions, ballX, ballY, isTeamAAttackingRight);
            UpdateTeamPositions(teamBPlayers, occupiedPositions, ballX, ballY, !isTeamAAttackingRight);

            if (ballHolder != null)
            {
                AttemptTackle(ballHolder);
            }

            if (ballHolder != null && !isPaused)
            {
                PerformPlayerAction(ballHolder);
            }

            // If no one holds the ball, assign it to the closest player
            if (GetBallHolder() == null && ballX >= 0 && ballY >= 0)
            {
                SimPlayer closestPlayer = FindClosestPlayer(ballX, ballY);
                if (closestPlayer != null)
                {
                    AssignBallToPlayer(closestPlayer);
                }
            }
        }

        private void HandleHalfTime()
        {
            isPaused = true;
            Console.Clear();
            Console.SetCursorPosition(50, 15);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("=== HALF-TIME ===");
            Console.SetCursorPosition(50, 17);
            Console.WriteLine("Press any key to begin second half...");
            Console.ResetColor();
            Console.ReadKey(true);

            SwitchAttackingDirection();
            MirrorTeams();

            isFirstHalf = false;
            currentMinute = 45;
            isPaused = false;
        }

        private void SwitchAttackingDirection()
        {
            isTeamAAttackingRight = !isTeamAAttackingRight;
        }

        private void MirrorTeams()
        {
            MirrorTeamPositions(teamAPlayers);
            MirrorTeamPositions(teamBPlayers);
        }

        private void MirrorTeamPositions(List<SimPlayer> team)
        {
            int centerX = (pitchLeft + pitchRight) / 2;
            foreach (var p in team)
            {
                int dx = p.XPosition - centerX;
                p.XPosition = centerX - dx;
            }
        }

        private bool[,] GetOccupiedPositions()
        {
            bool[,] occupied = new bool[liveMatchBox.Height, liveMatchBox.Width];
            foreach (var p in teamAPlayers)
            {
                MarkPositionAsOccupied(occupied, p.XPosition, p.YPosition);
            }
            foreach (var p in teamBPlayers)
            {
                MarkPositionAsOccupied(occupied, p.XPosition, p.YPosition);
            }
            return occupied;
        }

        private void MarkPositionAsOccupied(bool[,] occupied, int x, int y)
        {
            int lx = x - liveMatchBox.Left;
            int ly = y - liveMatchBox.Top;
            if (lx >= 0 && lx < liveMatchBox.Width && ly >= 0 && ly < liveMatchBox.Height)
            {
                occupied[ly, lx] = true;
            }
        }

        private void UnmarkPositionAsOccupied(bool[,] occupied, int x, int y)
        {
            int lx = x - liveMatchBox.Left;
            int ly = y - liveMatchBox.Top;
            if (lx >= 0 && lx < liveMatchBox.Width && ly >= 0 && ly < liveMatchBox.Height)
            {
                occupied[ly, lx] = false;
            }
        }

        private SimPlayer GetBallHolder()
        {
            foreach (var p in teamAPlayers)
                if (p.HasBall) return p;
            foreach (var p in teamBPlayers)
                if (p.HasBall) return p;
            return null;
        }

        /// <summary>
        /// Updates the positions of players in a team based on BFS pathfinding towards the attacking direction.
        private void UpdateTeamPositions(
            List<SimPlayer> team,
            bool[,] occupied,
            int ballX,
            int ballY,
            bool attackingRight)
        {
            if (team == null)
            {
                Console.WriteLine("[DEBUG] UpdateTeamPositions called with null 'team'??");
                return;
            }

            foreach (var player in team)
            {
                if (!player.HasBall && !player.IsGoalkeeper)
                {
                    UnmarkPositionAsOccupied(occupied, player.XPosition, player.YPosition);

                    // Determine target position based on attacking direction
                    int targetX = attackingRight ? pitchRight : pitchLeft;
                    int targetY = player.YPosition; // Keep Y the same for straightforward attacking

                    // Find path using BFS
                    var path = FindPath(player.XPosition, player.YPosition, targetX, targetY, occupied);

                    if (path.Count > 0)
                    {
                        var nextStep = path[0];
                        player.XPosition = nextStep.x;
                        player.YPosition = nextStep.y;
                    }

                    MarkPositionAsOccupied(occupied, player.XPosition, player.YPosition);
                }
            }
        }

        /// <summary>
        /// Finds the shortest path from (startX, startY) to (targetX, targetY) using BFS.
        private List<(int x, int y)> FindPath(int startX, int startY, int targetX, int targetY, bool[,] occupied)
        {
            var path = new List<(int x, int y)>();
            var visited = new HashSet<(int x, int y)>();
            var queue = new Queue<(int x, int y)>();
            var parents = new Dictionary<(int x, int y), (int x, int y)>();

            var start = (startX, startY);
            var target = (targetX, targetY);

            queue.Enqueue(start);
            visited.Add(start);

            // Directions: Up, Down, Left, Right
            var directions = new (int dx, int dy)[]
            {
                (0, -1), // Up
                (0, 1),  // Down
                (-1, 0), // Left
                (1, 0)   // Right
            };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == target)
                {
                    // Reconstruct path
                    var temp = current;
                    while (parents.ContainsKey(temp))
                    {
                        path.Add(temp);
                        temp = parents[temp];
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var (dx, dy) in directions)
                {
                    int newX = current.x + dx;
                    int newY = current.y + dy;
                    var neighbor = (newX, newY);

                    // Check boundaries
                    if (!IsPositionWithinPitch(newX, newY))
                        continue;

                    // Check if occupied or already visited
                    if (occupied[newY - pitchTop, newX - pitchLeft] || visited.Contains(neighbor))
                        continue;

                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    parents[neighbor] = current;
                }
            }

            // No path found
            return path;
        }

        /// <summary>
        /// Attempts a tackle on the player holding the ball.
        /// </summary>
        private void AttemptTackle(SimPlayer holder)
        {
            var allies = teamAPlayers.Contains(holder) ? teamAPlayers : teamBPlayers;
            var opponents = (allies == teamAPlayers) ? teamBPlayers : teamAPlayers;

            foreach (var opp in opponents)
            {
                if (!opp.HasBall && !opp.IsGoalkeeper)
                {
                    int dx = opp.XPosition - holder.XPosition;
                    int dy = opp.YPosition - holder.YPosition;
                    if (Math.Abs(dx) <= 2 && Math.Abs(dy) <= 2)
                    {
                        double successChance = opp.SkillLevel * 0.5;
                        if (randomGenerator.NextDouble() < successChance)
                        {
                            AssignBallToPlayer(opp);
                            break;
                        }
                    }
                }
            }
        }
        /// Performs an action (pass, shot, dribble) for the player holding the ball.
        /// </summary>
        private void PerformPlayerAction(SimPlayer player)
        {
            int actionChance = randomGenerator.Next(100);
            if (actionChance < 10)
            {
                AttemptPass(player);
            }
            else if (actionChance < 15)
            {
                AttemptShot(player);
            }
            // Else, the player dribbles (no action needed)
        }
        /// <param name="passer">The player attempting to pass.</param>
        private void AttemptPass(SimPlayer passer)
        {
            var sameTeam = teamAPlayers.Contains(passer) ? teamAPlayers : teamBPlayers;
            var potentialTargets = new List<SimPlayer>(sameTeam);
            potentialTargets.Remove(passer); // Cannot pass to oneself
            if (potentialTargets.Count == 0)
                return;

            var target = potentialTargets[randomGenerator.Next(potentialTargets.Count)];
            bool isStolen;
            AnimatePass(passer.XPosition, passer.YPosition, target.XPosition, target.YPosition, out isStolen);

            if (!isStolen)
            {
                AssignBallToPlayer(target);
            }
        }
        /// Animates the pass between two players and checks for interception.
        private void AnimatePass(int sx, int sy, int ex, int ey, out bool isStolen)
        {
            isStolen = false;
            int steps = 10;
            double dx = (ex - sx) / (double)steps;
            double dy = (ey - sy) / (double)steps;
            double ballX = sx;
            double ballY = sy;

            // Remove the ball from all players during the pass
            foreach (var p in teamAPlayers) p.HasBall = false;
            foreach (var p in teamBPlayers) p.HasBall = false;

            for (int step = 1; step <= steps; step++)
            {
                ballX += dx;
                ballY += dy;
                int cbx = (int)Math.Round(ballX);
                int cby = (int)Math.Round(ballY);

                if (CheckInterception(cbx, cby))
                {
                    isStolen = true;
                    var interceptor = FindClosestOpponent(cbx, cby);
                    if (interceptor != null)
                        AssignBallToPlayer(interceptor);
                    return;
                }

                // The actual "DrawBallAt" method is in the partial drawing file
                DrawBallAt(cbx, cby);

                if (isPaused)
                    break;

                int sleepTime = 300 / (gameSpeed > 0 ? gameSpeed : 1);
                Thread.Sleep(sleepTime);
            }
        }
        /// Attempts to take a shot on goal with a chance to score.
        private void AttemptShot(SimPlayer shooter)
        {
            // Remove the ball from all players during the shot
            foreach (var p in teamAPlayers) p.HasBall = false;
            foreach (var p in teamBPlayers) p.HasBall = false;

            double shotChance = randomGenerator.NextDouble();
            if (shotChance < 0.3) // 30% chance to score
            {
                if (teamAPlayers.Contains(shooter))
                    teamAScore++;
                else
                    teamBScore++;
            }
            // If the shot misses, no score is added
        }

        /// <summary>
        /// Assigns the ball to a specific player and removes it from others.
        /// </summary>
        /// <param name="newHolder">The player to assign the ball to.</param>
        private void AssignBallToPlayer(SimPlayer newHolder)
        {
            foreach (var p in teamAPlayers) p.HasBall = false;
            foreach (var p in teamBPlayers) p.HasBall = false;
            newHolder.HasBall = true;
        }

        /// <summary>
        /// Ends the match and displays the final score.
        /// </summary>
        private void EndMatch()
        {
            Console.Clear();
            Console.SetCursorPosition(50, 20);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Match Ended!");
            Console.WriteLine($"Final Score: Team A {teamAScore} - Team B {teamBScore}");
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);

            if (_fixture != null)
            {
                _fixture.Played = true;
                _fixture.HomeGoals = teamAScore;
                _fixture.AwayGoals = teamBScore;
                _fixture.Score = $"{teamAScore} : {teamBScore}";

                var homeStats = _fixture.HomeTeam.Stats;
                var awayStats = _fixture.AwayTeam.Stats;
                homeStats.GoalsFor += teamAScore;
                homeStats.GoalsAgainst += teamBScore;
                awayStats.GoalsFor += teamBScore;
                awayStats.GoalsAgainst += teamAScore;

                if (teamAScore > teamBScore)
                {
                    homeStats.Wins++;
                    homeStats.Points += 3;
                    awayStats.Losses++;
                }
                else if (teamAScore < teamBScore)
                {
                    awayStats.Wins++;
                    awayStats.Points += 3;
                    homeStats.Losses++;
                }
                else
                {
                    homeStats.Draws++;
                    homeStats.Points++;
                    awayStats.Draws++;
                    awayStats.Points++;
                }
            }

            hasMatchEnded = true;
            isRunning = false;
        }

        // -------------------------------------------------------------
        // Input handling
        // -------------------------------------------------------------
        private void HandleUserInput()
        {
            if (!Console.KeyAvailable) return;
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                isPaused = !isPaused;
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                isRunning = false;
            }
            else
            {
                if (currentSubMenu == MatchSettingsSubMenu.None)
                    HandleMainMenuInput(keyInfo);
                else
                    HandleSubMenuInput(keyInfo);
            }
        }

        private void HandleMainMenuInput(ConsoleKeyInfo keyInfo)
        {
            if (currentMainSection == MainMenuSection.LiveMatch)
            {
                if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    currentMainSection = MainMenuSection.MatchSettings;
                }
                else
                {
                    MoveTeamAPlayer(keyInfo);
                }
            }
            else if (currentMainSection == MainMenuSection.MatchSettings)
            {
                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    currentMainSection = MainMenuSection.LiveMatch;
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    settingsMenuIndex--;
                    if (settingsMenuIndex < 0)
                        settingsMenuIndex = settingsOptions.Count - 1; // Wrap around to the last option
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    settingsMenuIndex++;
                    if (settingsMenuIndex >= settingsOptions.Count)
                        settingsMenuIndex = 0; // Wrap around to the first option
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    SelectSettingsOption(); // Open the selected settings sub-menu
                }
            }
        }

        private void MoveTeamAPlayer(ConsoleKeyInfo keyInfo)
        {
            if (teamAPlayers.Count == 0)
            {
                Console.WriteLine("[DEBUG] No players in Team A??");
                return;
            }

            SimPlayer userPlayer = teamAPlayers[0]; // Assuming the first player is controlled by the user
            switch (keyInfo.Key)
            {
                case ConsoleKey.W:
                    userPlayer.YPosition--;
                    break;
                case ConsoleKey.S:
                    userPlayer.YPosition++;
                    break;
                case ConsoleKey.A:
                    userPlayer.XPosition--;
                    break;
                case ConsoleKey.D:
                    userPlayer.XPosition++;
                    break;
            }

            ClampPlayerPosition(userPlayer); // Ensure the player stays within the pitch
        }

        private void ClampPlayerPosition(SimPlayer player)
        {
            if (player.XPosition < pitchLeft) player.XPosition = pitchLeft;
            if (player.XPosition > pitchRight) player.XPosition = pitchRight;
            if (player.YPosition < pitchTop) player.YPosition = pitchTop;
            if (player.YPosition > pitchBottom) player.YPosition = pitchBottom;
        }

        private void HandleSubMenuInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                currentSubMenu = MatchSettingsSubMenu.None; // Close the sub-menu
                return;
            }

            if (currentSubMenu == MatchSettingsSubMenu.GameSpeed && keyInfo.Key == ConsoleKey.E)
            {
                IncreaseGameSpeed(); // Increase game speed when 'E' is pressed
            }
            // Additional sub-menu actions can be handled here
        }

        private void IncreaseGameSpeed()
        {
            gameSpeed += 10;
            if (gameSpeed > 300)
                gameSpeed = 300;
        }

        private void SelectSettingsOption()
        {
            switch (settingsMenuIndex)
            {
                case 0:
                    currentSubMenu = MatchSettingsSubMenu.GameSpeed;
                    break;
                case 1:
                    currentSubMenu = MatchSettingsSubMenu.FormationChange;
                    break;
                case 2:
                    currentSubMenu = MatchSettingsSubMenu.ModifyTactics;
                    break;
                case 3:
                    currentSubMenu = MatchSettingsSubMenu.PlayerRoles;
                    break;
            }
        }

        private void ControlFrameRate()
        {
            if (!isPaused)
            {
                int sleepDuration = 1000 / (gameSpeed > 0 ? gameSpeed : 1);
                Thread.Sleep(sleepDuration);
            }
            else
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Finds the closest player to a given position on the pitch.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <returns>The closest player or null if no players are present.</returns>
        private SimPlayer FindClosestPlayer(int x, int y)
        {
            SimPlayer closestPlayer = null;
            double minDistance = double.MaxValue;

            foreach (var player in teamAPlayers)
            {
                double distance = CalculateDistance(player.XPosition, player.YPosition, x, y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPlayer = player;
                }
            }

            foreach (var player in teamBPlayers)
            {
                double distance = CalculateDistance(player.XPosition, player.YPosition, x, y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }
        /// Calculates the Euclidean distance between two points.
        private double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        // -------------------------------------------------------------
        // Added Missing Methods to Resolve Compilation Errors
        // -------------------------------------------------------------

        /// <summary>
        /// Checks if the given (x, y) position is within the pitch boundaries.
        /// </summary>
        /// <param name="x">X position to check.</param>
        /// <param name="y">Y position to check.</param>
        /// <returns>True if within pitch; otherwise, false.</returns>
        private bool IsPositionWithinPitch(int x, int y)
        {
            return x >= pitchLeft && x <= pitchRight && y >= pitchTop && y <= pitchBottom;
        }

        /// <summary>
        /// Checks if a pass at the given (x, y) position can be intercepted by an opponent.
        /// </summary>
        /// <param name="x">X position of the ball during the pass.</param>
        /// <param name="y">Y position of the ball during the pass.</param>
        /// <returns>True if the pass is intercepted; otherwise, false.</returns>
        private bool CheckInterception(int x, int y)
        {
            var ballHolder = GetBallHolder();
            if (ballHolder == null)
                return false;

            var allies = teamAPlayers.Contains(ballHolder) ? teamAPlayers : teamBPlayers;
            var opponents = (allies == teamAPlayers) ? teamBPlayers : teamAPlayers;

            foreach (var opp in opponents)
            {
                if (!opp.IsGoalkeeper)
                {
                    int dx = opp.XPosition - x;
                    int dy = opp.YPosition - y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance <= 2) // interception range, adjustable
                    {
                        double interceptionChance = opp.SkillLevel * 0.5; // example formula
                        if (randomGenerator.NextDouble() < interceptionChance)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the closest opponent to the given (x, y) position.
        /// </summary>
        /// <param name="x">X position to find the closest opponent to.</param>
        /// <param name="y">Y position to find the closest opponent to.</param>
        /// <returns>The closest opponent player or null if no opponents are present.</returns>
        private SimPlayer FindClosestOpponent(int x, int y)
        {
            var ballHolder = GetBallHolder();
            if (ballHolder == null)
                return null;

            var allies = teamAPlayers.Contains(ballHolder) ? teamAPlayers : teamBPlayers;
            var opponents = (allies == teamAPlayers) ? teamBPlayers : teamAPlayers;

            SimPlayer closestOpponent = null;
            double minDistance = double.MaxValue;

            foreach (var opp in opponents)
            {
                double distance = CalculateDistance(opp.XPosition, opp.YPosition, x, y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestOpponent = opp;
                }
            }

            return closestOpponent;
        }

        // -------------------------------------------------------------
        // Rendering Methods are in the separate partial class
        // -------------------------------------------------------------
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace FootballManager
{
    // This file contains the core logic for the live match simulation.
    // In this redesigned version, A* pathfinding is used for dynamic player movement,
    // and the simulation is structured to update game state separately from drawing.
    public partial class LiveMatchSimulation
    {
        // --- Fields referencing your game classes ---
        private Fixture _fixture;
        private League _league;
        private Club _userClub;

        // --- Console / UI dimensions ---
        private const int ConsoleWidth = 150;
        private const int ConsoleHeight = 40;
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
            gameSpeed = 100; // Range 1..300 (adjustable)
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
            teamAPlayers = new List<SimPlayer>();
            teamBPlayers = new List<SimPlayer>();

            // Create 11 players for each team
            for (int i = 0; i < 11; i++)
            {
                // For Team A, place players randomly in the left half of the pitch
                int xA = randomGenerator.Next(pitchLeft, (pitchLeft + pitchRight) / 2);
                int yA = randomGenerator.Next(pitchTop, pitchBottom);
                bool isGK_A = (i == 0);
                double skillA = 0.6 + randomGenerator.NextDouble() * 0.2;
                teamAPlayers.Add(new SimPlayer($"A{i + 1}", xA, yA, isGK_A, skillA));

                // For Team B, place players in the right half
                int xB = randomGenerator.Next((pitchLeft + pitchRight) / 2, pitchRight);
                int yB = randomGenerator.Next(pitchTop, pitchBottom);
                bool isGK_B = (i == 0);
                double skillB = 0.6 + randomGenerator.NextDouble() * 0.2;
                teamBPlayers.Add(new SimPlayer($"B{i + 1}", xB, yB, isGK_B, skillB));
            }

            // Initially, assign the ball to the first player of Team A.
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
        // Simulation logic methods
        // -------------------------------------------------------------
        private bool IsMatchSettingsActive()
        {
            return currentMainSection == MainMenuSection.MatchSettings &&
                   currentSubMenu == MatchSettingsSubMenu.None;
        }

        private bool IsLiveMatchActive()
        {
            return currentMainSection == MainMenuSection.LiveMatch &&
                   currentSubMenu == MatchSettingsSubMenu.None;
        }

        // Main simulation tick – updates game state (player movement, ball actions, etc.)
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

            // Create an occupancy grid for pathfinding.
            bool[,] occupied = GetOccupiedPositions();

            // Determine current ball holder; if none, assign it to a random player.
            SimPlayer ballHolder = GetBallHolder();
            if (ballHolder == null)
            {
                ballHolder = teamAPlayers.Concat(teamBPlayers)
                                         .OrderBy(p => CalculateDistance(p.XPosition, p.YPosition, pitchLeft, pitchTop))
                                         .First();
                AssignBallToPlayer(ballHolder);
            }

            // Update positions of players using A* pathfinding.
            UpdateTeamPositions(teamAPlayers, occupied, ballHolder.XPosition, ballHolder.YPosition, isTeamAAttackingRight);
            UpdateTeamPositions(teamBPlayers, occupied, ballHolder.XPosition, ballHolder.YPosition, !isTeamAAttackingRight);

            // Let the ball holder perform an action (pass, shoot, or dribble) based on probabilities.
            if (!isPaused)
            {
                PerformPlayerAction(ballHolder);
            }
        }

        // -------------------------------------------------------------
        // A* Pathfinding Implementation
        // -------------------------------------------------------------
        private List<(int x, int y)> FindPathAStar(int startX, int startY, int targetX, int targetY, bool[,] occupied)
        {
            var openList = new List<Node>();
            var closedSet = new HashSet<(int, int)>();

            Node startNode = new Node(startX, startY, null, 0, Heuristic(startX, startY, targetX, targetY));
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // Get node with lowest f score.
                openList = openList.OrderBy(n => n.f).ToList();
                Node current = openList[0];
                openList.RemoveAt(0);
                closedSet.Add((current.x, current.y));

                if (current.x == targetX && current.y == targetY)
                {
                    // Reconstruct path.
                    var path = new List<(int x, int y)>();
                    Node node = current;
                    while (node.parent != null)
                    {
                        path.Add((node.x, node.y));
                        node = node.parent;
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var (dx, dy) in new (int, int)[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
                {
                    int nx = current.x + dx;
                    int ny = current.y + dy;

                    if (!IsPositionWithinPitch(nx, ny))
                        continue;
                    int gridX = nx - pitchLeft;
                    int gridY = ny - pitchTop;
                    if (occupied[gridY, gridX])
                        continue;
                    if (closedSet.Contains((nx, ny)))
                        continue;

                    double gCost = current.g + 1;
                    double hCost = Heuristic(nx, ny, targetX, targetY);
                    Node neighbor = new Node(nx, ny, current, gCost, hCost);

                    // If a node with the same coordinates and a lower f already exists, skip.
                    if (openList.Any(n => n.x == nx && n.y == ny && n.f <= neighbor.f))
                        continue;

                    openList.Add(neighbor);
                }
            }

            // No path found.
            return new List<(int x, int y)>();
        }

        private double Heuristic(int x, int y, int targetX, int targetY)
        {
            // Using Manhattan distance for simplicity.
            return Math.Abs(x - targetX) + Math.Abs(y - targetY);
        }

        private class Node
        {
            public int x;
            public int y;
            public Node parent;
            public double g;
            public double h;
            public double f => g + h;
            public Node(int x, int y, Node parent, double g, double h)
            {
                this.x = x;
                this.y = y;
                this.parent = parent;
                this.g = g;
                this.h = h;
            }
        }

        // -------------------------------------------------------------
        // Updated Player Movement using A* Pathfinding
        // -------------------------------------------------------------
        private void UpdateTeamPositions(List<SimPlayer> team, bool[,] occupied, int ballX, int ballY, bool attackingRight)
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

                    // Determine a dynamic target position.
                    int targetX = attackingRight ? pitchRight : pitchLeft;
                    // Allow some vertical variation for natural movement.
                    int targetY = player.YPosition + randomGenerator.Next(-1, 2);

                    // Use A* to compute the path.
                    var path = FindPathAStar(player.XPosition, player.YPosition, targetX, targetY, occupied);

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

        // -------------------------------------------------------------
        // Ball and Player Actions
        // -------------------------------------------------------------
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

        private void PerformPlayerAction(SimPlayer player)
        {
            // Decide on an action based on a random chance.
            int actionChance = randomGenerator.Next(100);
            if (actionChance < 10)
            {
                AttemptPass(player);
            }
            else if (actionChance < 15)
            {
                AttemptShot(player);
            }
            // Else: dribble (movement continues naturally)
        }

        private void AttemptPass(SimPlayer passer)
        {
            var sameTeam = teamAPlayers.Contains(passer) ? teamAPlayers : teamBPlayers;
            var potentialTargets = new List<SimPlayer>(sameTeam);
            potentialTargets.Remove(passer); // Cannot pass to oneself
            if (potentialTargets.Count == 0)
                return;

            // Choose the best target (e.g., the nearest one)
            SimPlayer target = potentialTargets.OrderBy(p => CalculateDistance(passer.XPosition, passer.YPosition, p.XPosition, p.YPosition)).First();
            bool isStolen;
            AnimatePass(passer.XPosition, passer.YPosition, target.XPosition, target.YPosition, out isStolen);

            if (!isStolen)
            {
                AssignBallToPlayer(target);
            }
        }

        private void AnimatePass(int sx, int sy, int ex, int ey, out bool isStolen)
        {
            isStolen = false;
            int steps = 10;
            double dx = (ex - sx) / (double)steps;
            double dy = (ey - sy) / (double)steps;
            double currentX = sx;
            double currentY = sy;

            // Remove the ball from all players during the pass
            foreach (var p in teamAPlayers) p.HasBall = false;
            foreach (var p in teamBPlayers) p.HasBall = false;

            for (int step = 1; step <= steps; step++)
            {
                currentX += dx;
                currentY += dy;
                int posX = (int)Math.Round(currentX);
                int posY = (int)Math.Round(currentY);

                if (CheckInterception(posX, posY))
                {
                    isStolen = true;
                    var interceptor = FindClosestOpponent(posX, posY);
                    if (interceptor != null)
                        AssignBallToPlayer(interceptor);
                    return;
                }

                DrawBallAt(posX, posY);

                if (isPaused)
                    break;

                int sleepTime = 300 / (gameSpeed > 0 ? gameSpeed : 1);
                Thread.Sleep(sleepTime);
            }
        }

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
        }

        private void AssignBallToPlayer(SimPlayer newHolder)
        {
            foreach (var p in teamAPlayers) p.HasBall = false;
            foreach (var p in teamBPlayers) p.HasBall = false;
            newHolder.HasBall = true;
        }

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
        // Input Handling
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
                        settingsMenuIndex = settingsOptions.Count - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    settingsMenuIndex++;
                    if (settingsMenuIndex >= settingsOptions.Count)
                        settingsMenuIndex = 0;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    SelectSettingsOption();
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

            SimPlayer userPlayer = teamAPlayers[0]; // Assume user controls the first player
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

            ClampPlayerPosition(userPlayer);
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
                currentSubMenu = MatchSettingsSubMenu.None;
                return;
            }

            if (currentSubMenu == MatchSettingsSubMenu.GameSpeed && keyInfo.Key == ConsoleKey.E)
            {
                IncreaseGameSpeed();
            }
            // Additional submenu handling can be added here.
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

        private double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

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
        // Added Missing Methods to Resolve Compilation Errors
        // -------------------------------------------------------------

        /// <summary>
        /// Checks if the given (x, y) position is within the pitch boundaries.
        /// </summary>
        private bool IsPositionWithinPitch(int x, int y)
        {
            return x >= pitchLeft && x <= pitchRight && y >= pitchTop && y <= pitchBottom;
        }

        /// <summary>
        /// Handles half-time by pausing the game, displaying a message, switching attacking direction,
        /// mirroring team positions, and resuming the game.
        /// </summary>
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

        /// <summary>
        /// Checks if a pass at the given (x, y) position can be intercepted by an opponent.
        /// </summary>
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
                    if (distance <= 2) // interception range
                    {
                        double interceptionChance = opp.SkillLevel * 0.5;
                        if (randomGenerator.NextDouble() < interceptionChance)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

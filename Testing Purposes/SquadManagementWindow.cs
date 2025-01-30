using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FootballManager
{
    public class SquadManagementWindow : Window
    {
        private Club _userTeam;
        private List<Formation> _formations;
        private int _activeFormationIndex;
        private Formation _selectedFormation;
        private Dictionary<int, Player> positionAssignments;
        private int activePositionIndex;
        private Player selectedPlayer;

        private bool inFormationSelection = true;
        private bool inStarting11Selection = false;

        public SquadManagementWindow(Club userTeam, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _userTeam = userTeam;
            InitializeFormations();

            if (_userTeam.SelectedFormation != null)
            {
                _selectedFormation = _userTeam.SelectedFormation;
                _activeFormationIndex = _formations.FindIndex(f => f.Name == _selectedFormation.Name);
                if (_activeFormationIndex == -1) _activeFormationIndex = 0;
            }
            else
            {
                _activeFormationIndex = 0;
                _selectedFormation = _formations[_activeFormationIndex];
            }

            if (_userTeam.PositionAssignments != null)
            {
                positionAssignments = _userTeam.PositionAssignments;
            }
            else
            {
                positionAssignments = new Dictionary<int, Player>();
            }

            activePositionIndex = 0;
            selectedPlayer = null;
        }

        private void InitializeFormations()
        {
            _formations = new List<Formation>
            {
                new Formation("3-5-2", new List<FormationPosition>
                {
                    new FormationPosition("ST", 40, 3),
                    new FormationPosition("ST", 80, 3),
                    new FormationPosition("CAM", 60, 5),
                    new FormationPosition("LM", 20, 7),
                    new FormationPosition("RM", 100, 7),
                    new FormationPosition("CDM", 40, 9),
                    new FormationPosition("CDM", 80, 9),
                    new FormationPosition("CB", 60, 12),
                    new FormationPosition("CB", 40, 14),
                    new FormationPosition("CB", 80, 14),
                    new FormationPosition("GK", 60, 17)
                }),
                new Formation("3-4-1-2", new List<FormationPosition>
                {
                    new FormationPosition("ST", 40, 3),
                    new FormationPosition("ST", 80, 3),
                    new FormationPosition("CAM", 60, 5),
                    new FormationPosition("LM", 20, 7),
                    new FormationPosition("RM", 100, 7),
                    new FormationPosition("CM", 40, 9),
                    new FormationPosition("CM", 80, 9),
                    new FormationPosition("CB", 60, 12),
                    new FormationPosition("CB", 40, 14),
                    new FormationPosition("CB", 80, 14),
                    new FormationPosition("GK", 60, 17)
                }),
                new Formation("4-5-1 Attack", new List<FormationPosition>
                {
                    new FormationPosition("ST", 60, 3),
                    new FormationPosition("CAM", 40, 5),
                    new FormationPosition("CAM", 80, 5),
                    new FormationPosition("LM", 20, 7),
                    new FormationPosition("CM", 60, 7),
                    new FormationPosition("RM", 100, 7),
                    new FormationPosition("LB", 20, 9),
                    new FormationPosition("RB", 100, 9),
                    new FormationPosition("CB", 40, 9),
                    new FormationPosition("CB", 80, 9),
                    new FormationPosition("GK", 60, 14)
                }),
                new Formation("4-1-2-1-2 Wide", new List<FormationPosition>
                {
                    new FormationPosition("ST", 40, 3),
                    new FormationPosition("ST", 80, 3),
                    new FormationPosition("CAM", 60, 5),
                    new FormationPosition("LM", 20, 7),
                    new FormationPosition("RM", 100, 7),
                    new FormationPosition("CDM", 60, 9),
                    new FormationPosition("LB", 20, 11),
                    new FormationPosition("RB", 100, 11),
                    new FormationPosition("CB", 40, 13),
                    new FormationPosition("CB", 80, 13),
                    new FormationPosition("GK", 60, 16)
                }),
                new Formation("4-1-2-1-2 Narrow", new List<FormationPosition>
                {
                    new FormationPosition("ST", 40, 3),
                    new FormationPosition("ST", 80, 3),
                    new FormationPosition("CAM", 60, 5),
                    new FormationPosition("CM", 40, 7),
                    new FormationPosition("CM", 80, 7),
                    new FormationPosition("CDM", 60, 9),
                    new FormationPosition("LB", 20, 11),
                    new FormationPosition("RB", 100, 11),
                    new FormationPosition("CB", 40, 13),
                    new FormationPosition("CB", 80, 13),
                    new FormationPosition("GK", 60, 16)
                }),
                new Formation("5-4-1 Flat", new List<FormationPosition>
                {
                    new FormationPosition("ST", 60, 3),
                    new FormationPosition("LM", 20, 5),
                    new FormationPosition("RM", 100, 5),
                    new FormationPosition("CM", 40, 5),
                    new FormationPosition("CM", 80, 5),
                    new FormationPosition("LB", 10, 7),
                    new FormationPosition("RB", 110, 7),
                    new FormationPosition("CB", 30, 7),
                    new FormationPosition("CB", 60, 9),
                    new FormationPosition("CB", 90, 7),
                    new FormationPosition("GK", 60, 12)
                }),
                new Formation("5-3-2 Holding", new List<FormationPosition>
                {
                    new FormationPosition("ST", 40, 3),
                    new FormationPosition("ST", 80, 3),
                    new FormationPosition("CM", 40, 5),
                    new FormationPosition("CM", 80, 5),
                    new FormationPosition("CDM", 60, 7),
                    new FormationPosition("LB", 20, 9),
                    new FormationPosition("CB", 40, 9),
                    new FormationPosition("CB", 60, 11),
                    new FormationPosition("CB", 80, 9),
                    new FormationPosition("RB", 100, 9),
                    new FormationPosition("GK", 60, 14)
                }),
                new Formation("4-2-3-1 Wide", new List<FormationPosition>
                {
                    new FormationPosition("ST", 60, 3),
                    new FormationPosition("CAM", 60, 5),
                    new FormationPosition("LM", 20, 5),
                    new FormationPosition("RM", 100, 5),
                    new FormationPosition("CDM", 40, 7),
                    new FormationPosition("CDM", 80, 7),
                    new FormationPosition("LB", 20, 9),
                    new FormationPosition("RB", 100, 9),
                    new FormationPosition("CB", 40, 9),
                    new FormationPosition("CB", 80, 9),
                    new FormationPosition("GK", 60, 12)
                }),
                new Formation("4-3-3 Attack", new List<FormationPosition>
                {
                    new FormationPosition("ST", 60, 3),
                    new FormationPosition("LW", 20, 3),
                    new FormationPosition("RW", 100, 3),
                    new FormationPosition("CAM", 60, 5),
                    new FormationPosition("CM", 40, 7),
                    new FormationPosition("CM", 80, 7),
                    new FormationPosition("LB", 20, 9),
                    new FormationPosition("RB", 100, 9),
                    new FormationPosition("CB", 40, 9),
                    new FormationPosition("CB", 80, 9),
                    new FormationPosition("GK", 60, 12)
                }),
                new Formation("4-4-2 Flat", new List<FormationPosition>
                {
                    new FormationPosition("ST", 40, 3),
                    new FormationPosition("ST", 80, 3),
                    new FormationPosition("LM", 20, 5),
                    new FormationPosition("RM", 100, 5),
                    new FormationPosition("CM", 40, 5),
                    new FormationPosition("CM", 80, 5),
                    new FormationPosition("LB", 20, 7),
                    new FormationPosition("RB", 100, 7),
                    new FormationPosition("CB", 40, 7),
                    new FormationPosition("CB", 80, 7),
                    new FormationPosition("GK", 60, 10)
                }),
            };
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

            if (inFormationSelection)
            {
                if (key == ConsoleKey.UpArrow)
                {
                    _activeFormationIndex = (_activeFormationIndex - 1 + _formations.Count) % _formations.Count;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    _activeFormationIndex = (_activeFormationIndex + 1) % _formations.Count;
                }
                else if (key == ConsoleKey.Enter)
                {
                    _selectedFormation = _formations[_activeFormationIndex];
                    _userTeam.SelectedFormation = _selectedFormation;
                    // Reset position assignments
                    positionAssignments = new Dictionary<int, Player>();
                    _userTeam.PositionAssignments = positionAssignments;
                    activePositionIndex = 0;
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    if (_selectedFormation.Positions.Count > 0)
                    {
                        inFormationSelection = false;
                        inStarting11Selection = true;
                        selectedPlayer = positionAssignments.ContainsKey(activePositionIndex)
                            ? positionAssignments[activePositionIndex]
                            : null;
                    }
                }
            }
            else if (inStarting11Selection)
            {
                if (key == ConsoleKey.UpArrow)
                {
                    activePositionIndex = (activePositionIndex - 1 + _selectedFormation.Positions.Count) % _selectedFormation.Positions.Count;
                    selectedPlayer = positionAssignments.ContainsKey(activePositionIndex)
                        ? positionAssignments[activePositionIndex]
                        : null;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    activePositionIndex = (activePositionIndex + 1) % _selectedFormation.Positions.Count;
                    selectedPlayer = positionAssignments.ContainsKey(activePositionIndex)
                        ? positionAssignments[activePositionIndex]
                        : null;
                }
                else if (key == ConsoleKey.Enter)
                {
                    // Assign player to position
                    string position = _selectedFormation.Positions[activePositionIndex].PositionName;
                    Player selected = SelectPlayerForPosition(position);
                    if (selected != null)
                    {
                        positionAssignments[activePositionIndex] = selected;
                        _userTeam.PositionAssignments = positionAssignments;
                        selectedPlayer = selected;
                    }
                }
                else if (key == ConsoleKey.LeftArrow)
                {
                    inStarting11Selection = false;
                    inFormationSelection = true;
                }
                else if (key == ConsoleKey.C)
                {
                    // Change contract
                    ChangeContract(selectedPlayer);
                }
            }
        }

        public override void Draw(bool active)
        {
            if (!_visible)
                return;

            // Clear only the window area
            ClearWindowArea();

            // Draw the window border and title
            base.Draw(active);

            // Draw formations list
            DrawFormations();

            // Draw starting 11
            DrawStarting11();

            // Draw Player Information
            DrawPlayerInformation();
        }

        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }

        private void DrawFormations()
        {
            int formationListX = _rectangle.X + 2;
            int formationListY = _rectangle.Y + 4;

            Console.SetCursorPosition(formationListX, formationListY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Formations:");
            Console.ResetColor();
            formationListY++;

            for (int i = 0; i < _formations.Count && formationListY < _rectangle.Y + _rectangle.Height - 20; i++)
            {
                Console.SetCursorPosition(formationListX, formationListY);
                if (inFormationSelection && i == _activeFormationIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"> {_formations[i].Name}");
                }
                else
                {
                    Console.Write($"  {_formations[i].Name}");
                }
                Console.ResetColor();
                formationListY++;
            }

            // Instructions
            formationListY++;
            Console.SetCursorPosition(formationListX, formationListY);
            Console.WriteLine("Use Up/Down to select formation");
            formationListY++;
            Console.SetCursorPosition(formationListX, formationListY);
            Console.WriteLine("Press Enter to confirm");
            formationListY++;
            Console.SetCursorPosition(formationListX, formationListY);
            Console.WriteLine("Press Right Arrow to assign players");
        }

        private void DrawStarting11()
        {
            if (_selectedFormation == null)
                return;

            int fieldOriginX = _rectangle.X + 30;
            int fieldOriginY = _rectangle.Y + 4;

            // Draw field (optional)
            DrawField(fieldOriginX, fieldOriginY);

            // Draw Formation Positions
            for (int i = 0; i < _selectedFormation.Positions.Count; i++)
            {
                var formationPosition = _selectedFormation.Positions[i];

                // Calculate position within the field
                int posX = fieldOriginX + formationPosition.XOffset / 2; // Adjust scaling if needed
                int posY = fieldOriginY + formationPosition.YOffset;

                string positionName = formationPosition.PositionName;
                Player assignedPlayer = positionAssignments.ContainsKey(i) ? positionAssignments[i] : null;
                string playerName = assignedPlayer != null ? assignedPlayer.Name : "Empty";

                // Highlight the active position
                if (inStarting11Selection && i == activePositionIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.SetCursorPosition(posX, posY);
                Console.Write($"{positionName}: {playerName}");

                Console.ResetColor();
            }

            // Instructions
            int instructionY = fieldOriginY + 20;
            Console.SetCursorPosition(fieldOriginX, instructionY);
            Console.WriteLine("Use Up/Down to select position");
            instructionY++;
            Console.SetCursorPosition(fieldOriginX, instructionY);
            Console.WriteLine("Press Enter to assign player");
            instructionY++;
            Console.SetCursorPosition(fieldOriginX, instructionY);
            Console.WriteLine("Press C to change contract");
            instructionY++;
            Console.SetCursorPosition(fieldOriginX, instructionY);
            Console.WriteLine("Press Left Arrow to go back");
        }

        private void DrawField(int originX, int originY)
        {
            int fieldWidth = 60;
            int fieldHeight = 20;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            for (int y = 0; y <= fieldHeight; y++)
            {
                Console.SetCursorPosition(originX, originY + y);
                Console.Write(new string(' ', fieldWidth));
            }
            Console.ResetColor();
        }

        private void DrawPlayerInformation()
        {
            if (selectedPlayer == null)
                return;

            int infoX = _rectangle.X + 95;
            int infoY = _rectangle.Y + 4;

            Console.SetCursorPosition(infoX, infoY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Player Information:");
            Console.ResetColor();
            infoY++;

            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Name: {selectedPlayer.Name}");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Age: {selectedPlayer.Age}");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Positions: {selectedPlayer.Position}");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Rating: {selectedPlayer.Rating}");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Potential: {selectedPlayer.Potential}");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Value: £{selectedPlayer.Value}M");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Wage: £{selectedPlayer.Wage}K / Week");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Contract Length: {selectedPlayer.ContractLength} Years Left");
            infoY++;
            Console.SetCursorPosition(infoX, infoY);
            Console.WriteLine($"Squad Status: {selectedPlayer.SquadStatus}");
        }

        private Player SelectPlayerForPosition(string position)
        {
            // Get set of assigned players
            HashSet<Player> assignedPlayers = new HashSet<Player>(positionAssignments.Values);

            // Filter players by position and exclude assigned players
            List<Player> availablePlayers = _userTeam.Players
                .Where(p => p.Position.Contains(position) && !assignedPlayers.Contains(p))
                .ToList();

            if (availablePlayers.Count == 0)
            {
                Console.Clear();
                Console.WriteLine($"No players available for position {position}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return null;
            }

            int selectedIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Select player for position {position}:\n");
                for (int i = 0; i < availablePlayers.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ResetColor();
                    }

                    var player = availablePlayers[i];
                    Console.WriteLine($"{(i == selectedIndex ? "> " : "  ")}{player.Name} (Rating: {player.Rating}, Age: {player.Age})");
                }
                Console.ResetColor();
                Console.WriteLine("\nPress Enter to select, Up/Down to navigate, S to show player details, Esc to cancel.");

                var input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex - 1 + availablePlayers.Count) % availablePlayers.Count;
                }
                else if (input.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex + 1) % availablePlayers.Count;
                }
                else if (input.Key == ConsoleKey.Enter)
                {
                    return availablePlayers[selectedIndex];
                }
                else if (input.Key == ConsoleKey.Escape)
                {
                    return null;
                }
                else if (input.Key == ConsoleKey.S)
                {
                    ShowPlayerDetails(availablePlayers[selectedIndex]);
                }
            }
        }

        private void ShowPlayerDetails(Player player)
        {
            Console.Clear();
            Console.WriteLine($"Name: {player.Name}");
            Console.WriteLine($"Age: {player.Age}");
            Console.WriteLine($"Positions: {player.Position}");
            Console.WriteLine($"Rating: {player.Rating}");
            Console.WriteLine($"Potential: {player.Potential}");
            Console.WriteLine($"Value: £{player.Value}M");
            Console.WriteLine($"Wage: £{player.Wage}K / Week");
            Console.WriteLine($"Contract Length: {player.ContractLength} Years Left");
            Console.WriteLine($"Squad Status: {player.SquadStatus}");
            Console.WriteLine("Player Statistics:");
            foreach (var stat in player.Statistics)
            {
                Console.WriteLine($"- {stat.Key}: {stat.Value}");
            }
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }

        private void ChangeContract(Player player)
        {
            if (player == null)
                return;

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine($"Change contract details for {player.Name}:");
                Console.WriteLine("1. Change Contract Length");
                Console.WriteLine("2. Change Squad Status");
                Console.WriteLine("3. Change Wage");
                Console.WriteLine("4. Exit");
                Console.WriteLine("Press the number to select an option.");

                var key = Console.ReadKey(true).KeyChar;
                switch (key)
                {
                    case '1':
                        Console.WriteLine("Enter new contract length in years:");
                        string lengthInput = Console.ReadLine();
                        if (int.TryParse(lengthInput, out int newLength))
                        {
                            player.ContractLength = newLength;
                            Console.WriteLine($"Contract length updated to {newLength} years.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid input.");
                        }
                        break;
                    case '2':
                        Console.WriteLine("Select new squad status:");
                        Console.WriteLine("1. First Team Member");
                        Console.WriteLine("2. Backup Player");
                        Console.WriteLine("3. Youngster");
                        var statusKey = Console.ReadKey(true).KeyChar;
                        switch (statusKey)
                        {
                            case '1':
                                player.SquadStatus = "First Team Member";
                                break;
                            case '2':
                                player.SquadStatus = "Backup Player";
                                break;
                            case '3':
                                player.SquadStatus = "Youngster";
                                break;
                            default:
                                Console.WriteLine("Invalid selection.");
                                break;
                        }
                        Console.WriteLine($"Squad status updated to {player.SquadStatus}.");
                        break;
                    case '3':
                        Console.WriteLine("Enter new wage in K per week:");
                        string wageInput = Console.ReadLine();
                        if (double.TryParse(wageInput, out double newWage))
                        {
                            player.Wage = newWage;
                            Console.WriteLine($"Wage updated to £{newWage}K per week.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid input.");
                        }
                        break;
                    case '4':
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }

    public class Formation
    {
        public string Name { get; }
        public List<FormationPosition> Positions { get; }

        public Formation(string name, List<FormationPosition> positions)
        {
            Name = name;
            Positions = positions;
        }
    }

    public class FormationPosition
    {
        public string PositionName { get; }
        public int XOffset { get; }
        public int YOffset { get; }

        public FormationPosition(string positionName, int xOffset, int yOffset)
        {
            PositionName = positionName;
            XOffset = xOffset;
            YOffset = yOffset;
        }
    }
}

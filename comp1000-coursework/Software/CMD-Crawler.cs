using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace GameDev
{
    /**
     * The main class of the Dungeon Game Application
     * 
     * You may add to your project other classes which are referenced.
     * Complete the templated methods and fill in your code where it says "Your code here".
     * Do not rename methods or variables which already eDist or change the method parameters.
     * You can do some checks if your project still aligns with the spec by running the tests in UnitTest1
     * 
     * For Questions do contact us!
     */
    public class Game
    {
        /**
         * use the following to store and control the movement 
         */
        public enum PlayerActions { NOTHING, NORTH, EAST, SOUTH, WEST, PICKUP, ATTACK, DROP, QUIT };
        private PlayerActions action = PlayerActions.NOTHING;
        public enum GameState { UNKOWN, STOP, RUN, START, INIT };
        private GameState status = GameState.INIT;


        //maps 
        private char[][] originalMap = new char[0][];
        private char[][] workingMap = new char[0][];

        //Coins
        int coins = 0;

        //Monsters Killed
        int kills = 0;

        //HP
        int playerHP = 2;

        /**
        * tracks if the game is running
        */
        private bool advanced = false;

        private string currentMap = null;

        private bool finish = false;

        /**
         * Reads user input from the Console
         * 
         * Please use and implement this method to read the user input.
         * 
         * Return the input as string to be further processed
         * 
         */
        private string ReadUserInput()
        {
            string inputRead = string.Empty;
            if (status == GameState.INIT | status == GameState.START | status == GameState.STOP)
                inputRead = Console.ReadLine();
            else if (status == GameState.RUN)
                //Reads key presses without enter when game is running
                inputRead = Console.ReadKey(true).Key.ToString();

            return inputRead;
        }

        private int counter = -1;

        /// <summary>
        /// Returns the number of steps a player made on the current map. The counter only counts steps, not actions.
        /// </summary>
        public int GetStepCounter()
        {
            return counter;
        }

        /**
         * Processed the user input string
         * 
         * takes apart the user input and does control the information flow
         *  * initializes the map ( you must call InitializeMap)
         *  * starts the game when user types in Play
         *  * sets the correct playeraction which you will use in the Update
         *  
         *  DO NOT read any information from command line in here but only act upon what the method receives.
         */
        public void ProcessUserInput(string input)
        {
            //Checks game isnt running and the user tried to change advanced mode on/off
            if (status != GameState.RUN && input == "advanced" && currentMap != "Advanced.map") //If advanced map is loaded advanced mode has to be on
            {
                //Switches between advanced mode on/off
                if (advanced == false)
                    advanced = true;
                else
                    advanced = false;
            }
            else
            {
                switch (status)
                {
                    case GameState.STOP:    //Game has finished. Player has reached end or died
                        switch (input)
                        {
                            case "restart": //Choose a different map or game mode (advanced)
                                //Reset all back to starting values
                                finish = false;
                                status = GameState.INIT;
                                advanced = false;
                                counter = 0;
                                coins = 0;
                                kills = 0;
                                playerHP = 2;
                                action = PlayerActions.NOTHING;
                                currentMap = null;
                                if (!Console.IsOutputRedirected)
                                    Console.Clear();
                                break;
                            case "replay":  //Replay the current map
                                //Reset all back to starting values but keep current map and start it up
                                finish = false;
                                status = GameState.INIT;
                                LoadMapFromFile(currentMap);
                                status = GameState.RUN;
                                counter = 0;
                                counter = 0;
                                coins = 0;
                                kills = 0;
                                playerHP = 2;
                                if (!Console.IsOutputRedirected)
                                    Console.Clear();

                                PrintMapToConsole();
                                PrintExtraInfo();
                                break;
                            case "quit":    //Exit the game
                                finish = false;
                                break;
                        }
                        break;
                    case GameState.RUN: //Game is running
                        //Take the player input and give the corresponding PlayerAction
                        switch (input)
                        {
                            case "start":
                                action = PlayerActions.NOTHING;
                                counter = 0;
                                break;
                            case "W":   //Movement
                                action = PlayerActions.NORTH;
                                counter++;
                                break;
                            case "A":
                                action = PlayerActions.WEST;
                                counter++;
                                break;
                            case "S":
                                action = PlayerActions.SOUTH;
                                counter++;
                                break;
                            case "D":
                                action = PlayerActions.EAST;
                                counter++;
                                break;
                            case "Z":   //Pickup Coin
                                action = PlayerActions.PICKUP;
                                break;
                            case "Q":   //Attack Monster
                                action = PlayerActions.ATTACK;
                                break;
                            default:
                                action = PlayerActions.NOTHING;
                                break;
                        }
                        break;
                    case GameState.START:   //Map has been loaded
                    case GameState.INIT:    //No map loaded
                        if (input == "start" && this.currentMap != null)    //If a map has been loaded start the game
                        {
                            this.status = GameState.RUN;
                            counter = 0;
                        }
                        else
                        {
                            //See if user is trying to load a map
                            string[] inputSplit = input.Split(' ');
                            //inputSplit[0] is "load" inputSplit[1] is the map the user is trying to load
                            if (inputSplit[0].Length > 1 && inputSplit[0] == "load")
                                switch (inputSplit[1])
                                {
                                    case "Simple.map":
                                        LoadMapFromFile("Simple.map");
                                        break;
                                    case "Simple2.map":
                                        LoadMapFromFile("Simple2.map");
                                        break;
                                    case "Advanced.map":
                                        if (advanced == true)
                                        {
                                            LoadMapFromFile("Advanced.map");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Cannot load advanced map as you arn't in advanced mode");
                                        }
                                        break;
                                }
                        }
                        break;
                }
            }
        }

        /**
         * The Main Game Loop. 
         * It updates the game state.
         * 
         * This is the method where you implement your game logic and alter the state of the map/game
         * use playeraction to determine how the character should move/act
         * the input should tell the loop if the game is active and the state should advance
         * 
         * Returns true if the game could be updated and is ongoing
         */
        public bool Update(GameState status)
        {
            if (this.status == GameState.RUN)   //Game is running
            {
                int[] playerLocation = GetPlayerPosition(); //Find the player

                char movingTo;

                switch (action)
                {
                    case PlayerActions.NORTH:   //Move player North
                        movingTo = this.workingMap[playerLocation[0] - 1][playerLocation[1]];
                        if (movingTo == '.' | movingTo == 'C' | movingTo == 'D' | movingTo == 'T')
                        {
                            this.workingMap[playerLocation[0] - 1][playerLocation[1]] = '@';
                            this.workingMap[playerLocation[0]][playerLocation[1]] = this.originalMap[playerLocation[0]][playerLocation[1]];
                            if (movingTo == 'D')    //Has the player reached the finish
                            {
                                this.status = GameState.STOP;
                                finish = true;
                            }
                        }
                        break;
                    case PlayerActions.EAST:    //Move player East
                        movingTo = this.workingMap[playerLocation[0]][playerLocation[1] + 1];
                        if (movingTo == '.' | movingTo == 'C' | movingTo == 'D' | movingTo == 'T')
                        {
                            this.workingMap[playerLocation[0]][playerLocation[1] + 1] = '@';
                            this.workingMap[playerLocation[0]][playerLocation[1]] = this.originalMap[playerLocation[0]][playerLocation[1]];
                            if (movingTo == 'D')
                            {
                                this.status = GameState.STOP;
                                finish = true;
                            }
                        }
                        break;
                    case PlayerActions.SOUTH:   //Move player South
                        movingTo = this.workingMap[playerLocation[0] + 1][playerLocation[1]];
                        if (movingTo == '.' | movingTo == 'C' | movingTo == 'D' | movingTo == 'T')
                        {
                            this.workingMap[playerLocation[0] + 1][playerLocation[1]] = '@';
                            this.workingMap[playerLocation[0]][playerLocation[1]] = this.originalMap[playerLocation[0]][playerLocation[1]];
                            if (movingTo == 'D')
                            {
                                this.status = GameState.STOP;
                                finish = true;
                            }
                        }
                        break;
                    case PlayerActions.WEST:    //Move player West
                        movingTo = this.workingMap[playerLocation[0]][playerLocation[1] - 1];
                        if (movingTo == '.' | movingTo == 'C' | movingTo == 'D' | movingTo == 'T')
                        {
                            this.workingMap[playerLocation[0]][playerLocation[1] - 1] = '@';
                            this.workingMap[playerLocation[0]][playerLocation[1]] = this.originalMap[playerLocation[0]][playerLocation[1]];
                            if (movingTo == 'D')
                            {
                                this.status = GameState.STOP;
                                finish = true;
                            }
                        }
                        break;
                    case PlayerActions.PICKUP:  //User tries to pickup coin
                        if (this.originalMap[playerLocation[0]][playerLocation[1]] == 'C')  //If the user is ontop of a coin
                        {
                            this.originalMap[playerLocation[0]][playerLocation[1]] = '.';   //Remove coin as player has picked it up
                            //Increase player coin count and HP
                            coins++;
                            playerHP++;
                        }
                        else if (this.originalMap[playerLocation[0]][playerLocation[1]] == 'T') //Treasure is dropped by Boss Monsters (worth 2 coins)
                        {
                            this.originalMap[playerLocation[0]][playerLocation[1]] = '.';   //Remove treasure as player has picked it up
                            //Increase player coin count and Hp by 2
                            coins += 2;
                            playerHP += 2;
                        }
                        break;
                    case PlayerActions.ATTACK:  //Player atacks in the 4 cardinal directions (only in advanced mode)
                        if (advanced == true)
                        {
                            //North
                            if (this.workingMap[playerLocation[0] - 1][playerLocation[1]] == 'M')    //Monster is to the North
                            {
                                //Remove the monster and drop a coin in its location (monseters have 1 HP so 1 hit kills them)
                                this.workingMap[playerLocation[0] - 1][playerLocation[1]] = 'C';
                                this.originalMap[playerLocation[0] - 1][playerLocation[1]] = 'C';
                                kills++;    //Increase kill count
                            }
                            else if (this.workingMap[playerLocation[0] - 1][playerLocation[1]] == 'B')  //Boss monster is to the North
                            {
                                //Remove boss monster and drop treasure in its location (Boss monsters have 1 HP and have picked up a coin)
                                this.workingMap[playerLocation[0] - 1][playerLocation[1]] = 'T';
                                this.originalMap[playerLocation[0] - 1][playerLocation[1]] = 'T';
                                kills++;
                            }
                            //East
                            if (this.workingMap[playerLocation[0]][playerLocation[1] + 1] == 'M')
                            {
                                this.workingMap[playerLocation[0]][playerLocation[1] + 1] = 'C';
                                this.originalMap[playerLocation[0]][playerLocation[1] + 1] = 'C';
                                kills++;
                            }
                            else if (this.workingMap[playerLocation[0]][playerLocation[1] + 1] == 'B')
                            {
                                this.workingMap[playerLocation[0]][playerLocation[1] + 1] = 'T';
                                this.originalMap[playerLocation[0]][playerLocation[1] + 1] = 'T';
                                kills++;
                            }
                            //South
                            if (this.workingMap[playerLocation[0] + 1][playerLocation[1]] == 'M')
                            {
                                this.workingMap[playerLocation[0] + 1][playerLocation[1]] = 'C';
                                this.originalMap[playerLocation[0] + 1][playerLocation[1]] = 'C';
                                kills++;
                            }
                            else if (this.workingMap[playerLocation[0] + 1][playerLocation[1]] == 'B')
                            {
                                this.workingMap[playerLocation[0] + 1][playerLocation[1]] = 'T';
                                this.originalMap[playerLocation[0] + 1][playerLocation[1]] = 'T';
                                kills++;
                            }
                            //West
                            if (this.workingMap[playerLocation[0]][playerLocation[1] - 1] == 'M')
                            {
                                this.workingMap[playerLocation[0]][playerLocation[1] - 1] = 'C';
                                this.originalMap[playerLocation[0]][playerLocation[1] - 1] = 'C';
                                kills++;
                            }
                            else if (this.workingMap[playerLocation[0]][playerLocation[1] - 1] == 'B')
                            {
                                this.workingMap[playerLocation[0]][playerLocation[1] - 1] = 'T';
                                this.originalMap[playerLocation[0]][playerLocation[1] - 1] = 'T';
                                kills++;
                            }
                        }
                        break;
                }

                //Monsters only act when in advanced mode
                if (advanced == true && status != GameState.STOP)   //Gamestate is STOP when player reaches finish and game ends
                {
                    DoMonsterActions();
                }

                if (playerHP <= 0)  //Ends the game if the player has died
                {
                    this.status = GameState.STOP;
                    finish = true;
                }

                if (this.status != GameState.STOP)  //Returns true if game is still running
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * The Main Visual Output element. 
         * It draws the new map after the player did something onto the screen.
         * 
         * This is the method where you implement your the code to draw the map ontop the screen
         * and show the move to the user. 
         * 
         * The method returns true if the game is running and it can draw something, false otherwise.
        */
        public bool PrintMapToConsole()
        {
            if (status == GameState.RUN)
            {
                if (!Console.IsOutputRedirected)    //Checks if console doesn't exists and the output isn't going elsewhere
                {
                    Console.CursorVisible = false;  //Hides cursor while game is running
                    Console.SetCursorPosition(0, 4);
                }

                for (int y = 0; y < this.workingMap.Length; y++)    //Iterrates through each line
                {
                    for (int x = 0; x < this.workingMap[y].Length; x++) //Itterrates through each character in the line and prints it
                    {
                        //Prints the character in its specified colour
                        switch (workingMap[y][x])
                        {
                            case '#':
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.Write(workingMap[y][x]);
                                break;
                            case '.':
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write(workingMap[y][x]);
                                break;
                            case 'D':
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(workingMap[y][x]);
                                break;
                            case '@':
                            case 'P':
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write('@');
                                break;
                            case 'C':
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.Write(workingMap[y][x]);
                                break;
                            case 'T':
                                //Treasure is displayed as a coin with a brighter yellow colour
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write('C');
                                break;
                            case 'M':
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(workingMap[y][x]);
                                break;
                            case 'B':
                                //Boss monsters are displayed as a monster with a darker red colour
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.Write('M');
                                break;
                            default:
                                Console.Write(workingMap[y][x]);
                                break;
                        }
                        Console.ResetColor();
                    }
                Console.Write(Environment.NewLine);
                }
            }
            return true;
        }
        /**
         * Additional Visual Output element. 
         * It draws the flavour texts and additional information after the map has been printed.
         * 
         * This is the method does not need to be used unless you want to output somethign else after the map onto the screen.
         * 
         */
        public bool PrintExtraInfo()
        {
            if (!Console.IsOutputRedirected)
            {
                if (this.currentMap != null)    //If a map has been loaded
                {
                    if (status == GameState.START)
                        Console.Clear();
                    
                    if (status == GameState.RUN | status == GameState.START)
                    {
                        //Displays currently loaded map and whether or not advanced mode is on/off
                        Console.SetCursorPosition(0, 3);
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("Currently loaded map: " + this.currentMap);

                        if (advanced == true)
                        {
                            Console.Write("      Advanced Mode Active");
                        }

                        Console.WriteLine();
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Clear();
                    }
                }

                if (status == GameState.RUN)
                {
                    //Displays player stats while game is running
                    Console.SetCursorPosition(0, workingMap.Length + 4);
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;

                    Console.Write("Health: ");
                    Console.Write(playerHP);

                    Console.Write("   Coins: ");
                    Console.Write(coins);

                    Console.Write("   Monster Kills: ");
                    Console.Write(kills);

                    Console.Write("   Steps: ");
                    Console.Write(GetStepCounter());
                    Console.ResetColor();

                    Console.WriteLine();
                    return true;
                }
                else if (status == GameState.STOP)
                {
                    //Displays the final player stats when the game has ended
                    Console.SetCursorPosition(0, workingMap.Length + 4);
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;

                    Console.Write("Final health: ");
                    Console.Write(playerHP);

                    if (playerHP < 0)
                        Console.Write(" overkill!");
                    
                    Console.Write("   Final coin total: ");
                    Console.Write(coins);

                    Console.Write("   Final Monster Kills: ");
                    Console.Write(kills);

                    Console.Write("   Final step count: ");
                    Console.Write(GetStepCounter());
                    Console.ResetColor();

                    Console.WriteLine();

                    if (playerHP <= 0)  //Checks how the game ended
                    {
                        //If player died display loseing message
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("YOU LOSE!");
                    }
                    else
                    {
                        //If player reached end display winning message
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Congradulations!");
                    }

                    Console.ResetColor();

                    //Display the ening menu
                    Console.WriteLine("Menu:");
                    Console.WriteLine("Type \"replay\" to replay map");
                    Console.WriteLine("Type \"restart\" to load new game");
                    Console.WriteLine("Type \"quit\" to quit playing");

                    Console.SetCursorPosition(0, workingMap.Length + 11);
                        
                    while (finish == true)  //Makes sure a valid command has been entered and only continues when it has
                    {
                        Console.Write("Your Command: ");
                        ProcessUserInput(ReadUserInput());
                    }
                }
            }
            return true;
        }

        /**
        * Map and GameState get initialized
        * mapName references a file name 
        * Do not use abosolute paths but use the files which are relative to the eDecutable.
        * 
        * Create a private object variable for storing the map in Game and using it in the game.
        */
        public bool LoadMapFromFile(String mapName)
        {
            //Reset the original and working map to empty
            this.originalMap = new char[0][];
            this.workingMap = new char[0][];

            if (status == GameState.INIT | status == GameState.START)
            {
                List<string> lines;
                try
                {
                    lines = File.ReadAllLines("maps" + Path.DirectorySeparatorChar + mapName).ToList(); //Load the file and store each line in a list
                    for (int i = lines.Count - 1; i > 0 ; i--)  //Itterate through list and remove blank lines
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                            lines.RemoveAt(i);
                    }
                }
                catch
                {
                    //File couldnt be loaded or file was empty
                    Console.WriteLine(mapName + " cannot be loaded");
                    return false;   //Dont continue
                }

                //Converts list of lines to 2D array of characters
                char[][] originalMap = new char[lines.Count][];

                for (int y = 0; y < lines.Count; y++)
                {
                    char[] characters = lines[y].ToCharArray(); //Converts the line into an array of characters

                    originalMap[y] = new char[characters.Length];   //Redefines the 2D array to fit the charater array

                    for (int x = 0; x < characters.Length; x++)
                    {
                        originalMap[y][x] = characters[x];  //Fills the 2D array with character array
                    }
                }

                //Does a deep copy of the original map to working map
                char[][] workingMap = new char[originalMap.Length][];
                for (int i = 0; i < workingMap.Length; i++)
                {
                    workingMap[i] = (char[])originalMap[i].Clone();
                }

                this.originalMap = originalMap;
                this.workingMap = workingMap;

                this.currentMap = mapName;

                this.status = GameState.START;  //Map has been loaded

                return true;
            }
            return false;
        }

        /**
         * Returns a representation of the currently loaded map
         * before any move was made.
         * This map should not change when the player moves
         */
        public char[][] GetOriginalMap()
        {
            return this.originalMap;
        }

        /*
         * Returns the current map state and contains the player's move
         * without altering it 
         */
        public char[][] GetCurrentMapState()
        {
            return this.workingMap;
        }

        /**
         * Returns the current position of the player on the map
         * 
         * The first value is the y coordinate and the second is the x coordinate on the map
         */
        public int[] GetPlayerPosition()
        {
            int y;
            int x;
            int[] playerPosition = new int[2];

            //Itterates through the working map and finds the location of the player
            for (y = 0; y < this.workingMap.Length; y++)
            {
                for (x = 0; x < this.workingMap[y].Length; x++)
                {
                    if (this.workingMap[y][x] == '@')
                    {
                        playerPosition[0] = y;
                        playerPosition[1] = x;
                        break;
                    }
                    else if (this.workingMap[y][x] == 'P')  //Replaces the P (player starting location) with active player (@)
                    {
                        playerPosition[0] = y;
                        playerPosition[1] = x;
                        this.originalMap[y][x] = '.';
                        this.workingMap[y][x] = '@';
                        break;
                    }
                }
            }
            return playerPosition;
        }

        /**
        * Returns the next player action
        * 
        * This method does not alter any internal state
        */
        public int GetPlayerAction()
        {
            return (int)action;
        }

        public List<int[]> GetMonsterPositions()
        {
            int y;
            int x;
            List<int[]> monsterLocations = new List<int[]>();
            //Itterates through the working map and finds the location of all the monsters
            for (y = 0; y < this.workingMap.Length; y++)
            {
                for (x = 0; x < this.workingMap[y].Length; x++)
                {
                    if (this.workingMap[y][x] == 'M')   //Monster found
                    {
                        this.originalMap[y][x] = '.';
                        int[] location = { y, x };
                        monsterLocations.Add(location);
                    }
                    else if (this.workingMap[y][x] == 'B')  //Boss Monster found
                    {
                        int[] location = { y, x, 1 };   //The 1 signifies the monster is a boss
                        monsterLocations.Add(location);
                    }
                }
            }
            return monsterLocations;
        }

        public void DoMonsterActions()
        {
            List<int[]> monsterLocations = GetMonsterPositions();   //Stores the locations of the monsters
            //Goes through each monster 1 by 1 and performs its action
            for (int i = 0; i < monsterLocations.Count; i++)
            {
                //Is the player within reach(both cardinal and ordinal directions). If so attack the player
                if (this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] == '@' |    //North
                    this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] == '@' |    //East
                    this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] == '@' |    //South
                    this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] == '@' |    //West
                    this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1] + 1] == '@' |    //North East
                    this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1] + 1] == '@' |    //South East
                    this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1] - 1] == '@' |    //North West
                    this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1] - 1] == '@') //South West
                {
                    if (monsterLocations[i].Length == 3)    //Boss Monsters deal more damage (boss monsters location stored [y,x,1])
                        playerHP -= 3;
                    else
                        playerHP--;
                }
                else
                {
                    bool hasMoved = false;
                    //Sees if coin is next to monster (in cardinal directions). If so move ontop of the coin and eat it
                    if (monsterLocations[i].Length != 3)    //Only regular monsters can eat coins
                    {
                        if (this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] == 'C') //North
                        {
                            this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] = 'B';  //Turn the monster into a Boss monster
                            this.originalMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] = '.'; //Remove the coin as the monster has ate it
                            hasMoved = true;    //The monster has performed its action
                        }
                        else if (this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] == 'C')    //East
                        {
                            this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] = 'B';
                            this.originalMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] = '.';
                            hasMoved = true;
                        }
                        else if (this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] == 'C')    //South
                        {
                            this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] = 'B';
                            this.originalMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] = '.';
                            hasMoved = true;
                        }
                        else if (this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] == 'C')    //West
                        {
                            this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] = 'B';
                            this.originalMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] = '.';
                            hasMoved = true;
                        }
                    }
                    if (hasMoved == true)
                    {
                        //Fill in the space the monster has just moved from
                        this.workingMap[monsterLocations[i][0]][monsterLocations[i][1]] = this.originalMap[monsterLocations[i][0]][monsterLocations[i][1]];
                    }
                    else
                    {
                        //Find out what direction the monster can move in
                        List<char> canMoveTo = new List<char>();
                        
                        if (this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] == '.') //North (Monster can move north as it is an empty space)
                            canMoveTo.Add('N');

                        if (this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] == '.') //East
                            canMoveTo.Add('E');

                        if (this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] == '.') //South
                            canMoveTo.Add('S');
                        
                        if (this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] == '.') //West
                            canMoveTo.Add('W');

                        if (canMoveTo.Count > 0)    //There is a valid space for the monster to move to
                        {
                            Random rnd = new Random();
                            //Randomly pick from the available directions (with equal odds)
                            switch (canMoveTo[rnd.Next(canMoveTo.Count)])
                            {
                                case 'N':   //North
                                    //If monster is a boss put a B in the moved to location else M
                                    if (monsterLocations[i].Length == 3)
                                        this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] = 'B';
                                    else
                                        this.workingMap[monsterLocations[i][0] - 1][monsterLocations[i][1]] = 'M';
                                    break;
                                case 'E':   //East
                                    if (monsterLocations[i].Length == 3)
                                        this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] = 'B';
                                    else
                                        this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] + 1] = 'M';
                                    break;
                                case 'S':   //South
                                    if (monsterLocations[i].Length == 3)
                                        this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] = 'B';
                                    else
                                        this.workingMap[monsterLocations[i][0] + 1][monsterLocations[i][1]] = 'M';
                                    break;
                                case 'W':   //West
                                    if (monsterLocations[i].Length == 3)
                                        this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] = 'B';
                                    else
                                        this.workingMap[monsterLocations[i][0]][monsterLocations[i][1] - 1] = 'M';
                                    break;
                            }
                            //Monster has moved so fill in the space it moved from
                            this.workingMap[monsterLocations[i][0]][monsterLocations[i][1]] = this.originalMap[monsterLocations[i][0]][monsterLocations[i][1]];
                        }
                    }
                }
            }
        }

        public GameState GameIsRunning()
        {
            return status;
        }

        /**
         * Main method and Dntry point to the program
         * ####
         * Do not change! 
        */
        static void Main(string[] args)
        {
            Game crawler = new Game();

            string input = string.Empty;
            Console.WriteLine("Welcome to the Commandline Dungeon!" + Environment.NewLine +
                "May your Quest be filled with riches!" + Environment.NewLine);

            // Loops through the input and determines when the game should quit
            while (crawler.GameIsRunning() != GameState.STOP && crawler.GameIsRunning() != GameState.UNKOWN)
            {
                Console.Write("Your Command: ");
                input = crawler.ReadUserInput();
                Console.WriteLine(Environment.NewLine);
                crawler.ProcessUserInput(input);
                crawler.Update(crawler.GameIsRunning());
                crawler.PrintMapToConsole();
                crawler.PrintExtraInfo();
            }

            Console.WriteLine("See you again" + Environment.NewLine +
                "In the CMD Dungeon! ");
        }
    }
}
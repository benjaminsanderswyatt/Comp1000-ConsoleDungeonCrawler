using GameDev;
using Xunit;

namespace Tests
{
    public class Tests
    {
        Game crawler;

        private void Setup()
        {
            crawler = new Game();
        }

        [Fact]
        public void InitialCheckTestInit()
        {
            Setup();

            Assert.True(crawler.GameIsRunning() == Game.GameState.INIT,"The game should not be running as we have not loaded the map yet");
            Assert.True(crawler.GetPlayerAction() == 0, "No action should have been triggered yet");

            int[] pos = crawler.GetPlayerPosition();
            Assert.True( pos[1]== 0 && pos[0] == 0, "The player should still be on [0,0]");
            char[][] map = crawler.GetOriginalMap();
            Assert.True(map.Length == 0, "The map should still be empty ");
        }

        [Fact]
        public void InitialCheckTestInput()
        {
            Setup();

            crawler.ProcessUserInput("lod Simple.Map");
            crawler.ProcessUserInput("lod Simple.Mp");
            crawler.ProcessUserInput("play Simple.Map");
            crawler.ProcessUserInput("load play");
            Assert.True(crawler.GameIsRunning() == Game.GameState.INIT, "The game should not be running as we have not loaded the map correctly");

        }

        [Fact]
        public void InitialCheckTestMapLoading()
        {
            Setup();
            bool result = crawler.LoadMapFromFile("Simple.map");
            int yDim = crawler.GetOriginalMap().Length;
            Assert.True(result && yDim == 10, "Map loading is not working: The y dimension for the simple map shoudl be 10 but is " + yDim);
            int DDim = crawler.GetOriginalMap()[0].Length;
            Assert.True(result && DDim == 31, "Map loading is not working: The D dimension for the simple map shoudl be 31 but is "+DDim);
        }

        [Fact]
        public void TestMapStorage()
        {
            Setup();
            bool result = crawler.LoadMapFromFile("Simple.map");
            Assert.True(result, "The Simple.map should be loadable from the maps folder");
            Assert.False(crawler.GetOriginalMap().Equals(crawler.GetCurrentMapState()), "Map loading is not working: Original and Current Map should not use the same underlying object or shared data.");
        }

        [Fact]
        public void InitialCheckTestGameInit()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            char[][] orig = crawler.GetOriginalMap();
            Assert.True(crawler.GetOriginalMap().Length == 10, "Map loading is not working unsing the load command ");
            Assert.True(crawler.GetOriginalMap()[0].Length == 31, "Map loading is not working unsing the load command ");
            Assert.True(crawler.GameIsRunning() == Game.GameState.START, "The game should not be running as we have a map and the user command play was used.");
            char[][] curr = crawler.GetCurrentMapState();
            for (int y = 0; y < orig.Length; y++)
                for (int D = 0; D < orig[0].Length; D++)
                    Assert.True(orig[y][D] == curr[y][D], $"The current map is not correctly showing tile [{y},{D}] which is {curr[y][D]} but should be {orig[y][D]}. Before the game starts the player character is not placed on the map yet.");
        }

        [Fact]
        public void InitialCheckTestGameStart()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            char[][] orig = crawler.GetOriginalMap();
            Assert.True(crawler.GetOriginalMap().Length == 10, "Map loading is not working unsing the load command ");
            Assert.True(crawler.GetOriginalMap()[0].Length == 31, "Map loading is not working unsing the load command ");
            Assert.True(crawler.GameIsRunning() == Game.GameState.START, "The game should not be running as we have a map and the user command play was not used.");
            crawler.ProcessUserInput("start");
            Assert.True(crawler.GameIsRunning() == Game.GameState.RUN, "The game should now be running as we have a map and the user command play was used.");
        }

        [Fact]
        public void TestUpdatedPlayerPosition()
        {
            Setup();
            crawler.LoadMapFromFile("Simple.map");
            int[] pos = crawler.GetPlayerPosition();
            Assert.True(pos[1] == 1 && pos[0] == 8, "Player position is not set correctly!");
            char player = crawler.GetCurrentMapState()[pos[0]][pos[1]];
            Assert.True(player == '@' || player == 'P' , "Player position is not set correctly!");
        }

        [Fact]
        public void TestPlayerActions()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "No player action should be received yet.");
            crawler.ProcessUserInput("W");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "Dven though player used a movement command the game is not active. " +
                "thus, no playeraction should be triggered.");
            crawler.ProcessUserInput("start");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "No player action should be received yet.");
            Assert.True(crawler.GameIsRunning() == Game.GameState.RUN, "The game should be running as we have a map and the user command play was used.");
            crawler.ProcessUserInput("D");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.EAST, "Game is Active and player triggered moving using <D> but not the correct action was triggered");
            crawler.ProcessUserInput("W");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NORTH, "Game is Active and player triggered moving using <W> but not the correct action was triggered");
            crawler.ProcessUserInput("A");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.WEST, "Game is Active and player triggered moving using <A> but not the correct action was triggered");
            crawler.ProcessUserInput("Z");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.PICKUP, "Game is Active and player triggered pickup using <Z> but not the correct action was triggered");
            crawler.ProcessUserInput("Q");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.ATTACK, "Game is Active and player triggered pickup using <Q> but not the correct action was triggered");
            crawler.ProcessUserInput("S");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.SOUTH, "Game is Active and player triggered moving using <S> but not the correct action was triggered");
            crawler.ProcessUserInput("start");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "Game is Active and player typed in play again which should do nothing.");
        }


        [Fact]
        public void TestGameWithWrongOrder()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            char[][] orig = crawler.GetOriginalMap();
            Assert.True(crawler.GetOriginalMap().Length == 10, "Map loading is not working unsing the load command ");
            Assert.True(crawler.GetOriginalMap()[0].Length == 31, "Map loading is not working unsing the load command ");
            //crawler.ProcessUserInput("start");
            int[] pos = crawler.GetPlayerPosition();
            Assert.False(crawler.GameIsRunning() == Game.GameState.RUN, "The game should not be running as we have a map and the user command play was not used.");
            crawler.ProcessUserInput("W");
            int[] pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0], $"The player is moving in the right direction, but the game was not started yet. The player should be at [{pos[1]},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            char[][] curr = crawler.GetCurrentMapState();
            char player = crawler.GetCurrentMapState()[pos[0]][pos[1]];
            Assert.True(player == '@' || player == 'P', "Player position is not set correctly!");
        }

        [Fact]
        public void TestGamePlayMoving()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            char[][] orig = crawler.GetOriginalMap();
            Assert.True(crawler.GetOriginalMap().Length == 10, "Map loading is not working unsing the load command ");
            Assert.True(crawler.GetOriginalMap()[0].Length == 31, "Map loading is not working unsing the load command ");
            crawler.ProcessUserInput("start");

            //first move
            int[] pos = (int[])crawler.GetPlayerPosition().Clone();
            Assert.True(crawler.GameIsRunning() == Game.GameState.RUN, "The game should now be running as we have a map and the user command play was used.");
            crawler.ProcessUserInput("W");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            int[] pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0] + 1, $"The player is not moving in the right direction. The player should be at [{pos[1]},{pos[0] - 1}] but is at [{pos2[1]},{pos2[0]}]");
            char[][] curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]]== '.', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos2[0]][pos2[1]] == '@', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos2[0]][pos2[1]]}.");
            
            //second move
            pos = (int[])crawler.GetPlayerPosition().Clone();
            Assert.True(crawler.GameIsRunning() == Game.GameState.RUN, "The game should now be running as we have a map and the user command play was used.");
            crawler.ProcessUserInput("W");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0] + 1, $"The player is not moving in the right direction. The player should be at [{pos[1]},{pos[0] - 1}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]] == '.', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos[0]+1][pos[1]] == '.', $"The current map is not correctly showing an empty tile under the position 2 moves ago but shows {curr[pos[0]+1][pos[1]]}.");
            Assert.True(curr[pos2[0]][pos2[1]] == '@', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos2[0]][pos2[1]]}.");

            //third move
            pos = (int[])crawler.GetPlayerPosition().Clone();
            crawler.ProcessUserInput("D");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1]+1 == pos2[1] && pos[0] == pos2[0], $"The player is not moving in the right direction. The player should be at [{pos[1]+1},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]] == '.', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos2[0]][pos2[1]] == '@', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos2[0]][pos2[1]]}.");

            //forth move
            pos = (int[])crawler.GetPlayerPosition().Clone();
            crawler.ProcessUserInput("A");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] - 1 == pos2[1] && pos[0] == pos2[0], $"The player is not moving in the right direction. The player should be at [{pos[1] - 1},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]] == '.', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos2[0]][pos2[1]] == '@', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos2[0]][pos2[1]]}.");
        }

        [Fact]
        public void TestGamePlayRespectingWalls()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            crawler.ProcessUserInput("start");

            //first move
            int[] pos = (int[])crawler.GetPlayerPosition().Clone();
            crawler.ProcessUserInput("A");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            int[] pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0], $"The player is moving in the right direction but should not be able to move onto a wall. " +
                $"The player should be at [{pos[1]},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            char[][] curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]] == '@', $"The current map is not correctly showing the player standing still but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos[0]][pos[1]-1] == '#', $"The current map is not correctly showing the player standing still in front of the wall but shows {curr[pos[0]][pos[1]-1]} in the wall.");


            //second move
            pos = (int[])crawler.GetPlayerPosition().Clone();
            crawler.ProcessUserInput("S");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0], $"The player is moving in the right direction but should not be able to move onto a wall. " +
                $"The player should be at [{pos[1]},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]] == '@', $"The current map is not correctly showing the player standing still but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos[0]+1][pos[1]] == '#', $"The current map is not correctly showing the player standing still in front of the wall but shows {curr[pos[0] + 1][pos[1]]} in the wall.");

            //series of moves
            pos = (int[])crawler.GetPlayerPosition().Clone();
            for (int D = 1; D < 17; D++)
            {
                crawler.ProcessUserInput("D");
                crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            }
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1]+16 == pos2[1] && pos[0] == pos2[0], $"The player is moving to far, it should not be able to move onto a wall. " +
                $"The player should be at [{pos[1]+16},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            //advanced Assert.True(curr[pos[0]+16][pos[1]] == '@', $"The current map is not correctly showing the player standing still but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos2[0] + 1][pos[1]] == '#', $"The current map is not correctly showing the player standing infront of a wall.");
        }

        [Fact]
        public void TestMovementCounter()
        {
            Setup();
            Assert.True(crawler.GetStepCounter() == -1, "Game is not running so step counter should be -1");
            crawler.ProcessUserInput("load Simple2.map");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "No player action should be received yet.");
            crawler.ProcessUserInput("W");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "Dven though player used a movement command the game is not active. " +
                "thus, no playeraction should be triggered.");
            crawler.ProcessUserInput("start");
            Assert.True(crawler.GetStepCounter() == 0,"Game has just started; zero steps were made.");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "No player action should be received yet.");
            Assert.True(crawler.GameIsRunning() == Game.GameState.RUN, "The game should be running as we have a map and the user command play was used.");
            crawler.ProcessUserInput("D");
            Assert.True(crawler.GetStepCounter() == 1, "One step was made not "+crawler.GetStepCounter());
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.EAST, "Game is Active and player triggered moving using <D> but not the correct action was triggered");
            crawler.ProcessUserInput("W");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NORTH, "Game is Active and player triggered moving using <W> but not the correct action was triggered");
            crawler.ProcessUserInput("A");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.WEST, "Game is Active and player triggered moving using <A> but not the correct action was triggered");
            Assert.True(crawler.GetStepCounter() == 3, "Three step were made not " + crawler.GetStepCounter());
            crawler.ProcessUserInput("Z");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.PICKUP, "Game is Active and player triggered pickup using <Z> but not the correct action was triggered");
            Assert.True(crawler.GetStepCounter() == 3, "Picking up is not a step only 3 steps made not " + crawler.GetStepCounter());
            crawler.ProcessUserInput("S");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.SOUTH, "Game is Active and player triggered moving using <S> but not the correct action was triggered");
            crawler.ProcessUserInput("load Simple.map");
            crawler.ProcessUserInput("start");
            Assert.True(crawler.GetPlayerAction() == (int)Game.PlayerActions.NOTHING, "Game is Active and player typed in play again which should do nothing.");
            Assert.True(crawler.GetStepCounter() == 0, "Step counter and Game were restarted with start, zero step were made not " + crawler.GetStepCounter());
        }

        [Fact]
        public void TestGamePlayFinish()
        {
            Setup();
            crawler.ProcessUserInput("load Simple.map");
            crawler.ProcessUserInput("start");
            char[][] orig = crawler.GetOriginalMap();
            //first move
            int[] pos = (int[])crawler.GetPlayerPosition().Clone();
            Assert.True(crawler.GameIsRunning() == Game.GameState.RUN, "The game should now be running as we have a map and the user command play was used.");
            crawler.ProcessUserInput("W");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            int[] pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0] + 1, $"The player is not moving in the right direction. The player should be at [{pos[1]},{pos[0] - 1}] but is at [{pos2[1]},{pos2[0]}]");
            char[][] curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]] == '.', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos[0]][pos[1]]}.");
            Assert.True(curr[pos2[0]][pos2[1]] == '@', $"The current map is not correctly showing an empty tile under the previous player pos but shows {curr[pos2[0]][pos2[1]]}.");

            //moving North
            pos = (int[])crawler.GetPlayerPosition().Clone();
            for (int y = 0; y < 4; y++)
            {
                crawler.ProcessUserInput("W");
                crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            }
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0] == pos2[0]+4, $"The player is moving wrong. " +
                $"The player should be at [{pos[1]},{pos[0]-4}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0] - 4][pos[1]] == '@', $"The current map is not correctly showing the player but shows {curr[pos[0] - 4][pos[1]]}.");
        
            //moving Dast
            pos = (int[])crawler.GetPlayerPosition().Clone();
            for (int D = 0; D < 21; D++)
            {
                crawler.ProcessUserInput("D");
                crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            }
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1]+21 == pos2[1] && pos[0] == pos2[0], $"The player is moving wrong. " +
                $"The player should be at [{pos[1]+21},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1]+21] == '@', $"The current map is not correctly showing the player but shows {curr[pos[0]][pos[1]+21]}.");

            //moving South
            pos = (int[])crawler.GetPlayerPosition().Clone();
            for (int y = 0; y < 3; y++)
            {
                crawler.ProcessUserInput("S");
                crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            }
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0]+3 == pos2[0], $"The player is moving wrong. " +
                $"The player should be at [{pos[1]},{pos[0]+3}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0] + 3][pos[1]] == '@', $"The current map is not correctly showing the player but shows {curr[pos[0] + 3][pos[1]]}.");

            //moving West
            pos = (int[])crawler.GetPlayerPosition().Clone();
            for (int D = 0; D < 5; D++)
            {
                crawler.ProcessUserInput("A");
                crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            }
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1]-5 == pos2[1] && pos[0] == pos2[0], $"The player is moving wrong. " +
                $"The player should be at [{pos[1] -5},{pos[0]}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]][pos[1] - 5] == '@', $"The current map is not correctly showing the player but shows {curr[pos[0]][pos[1] - 5]}.");

            //moving North
            pos = (int[])crawler.GetPlayerPosition().Clone();
            crawler.ProcessUserInput("W");
            crawler.Update(Game.GameState.RUN); crawler.PrintMapToConsole();
            pos2 = crawler.GetPlayerPosition();
            Assert.True(pos[1] == pos2[1] && pos[0]-1 == pos2[0], $"The player is moving wrong. " +
                $"The player should be at [{pos[1]},{pos[0] - 1}] but is at [{pos2[1]},{pos2[0]}]");
            curr = crawler.GetCurrentMapState();
            Assert.True(curr[pos[0]-1][pos[1]] == 'D' || curr[pos[0] - 1][pos[1]] == '@', $"The current map is not correctly showing the player but shows {curr[pos[0]-1][pos[1]]}.");
            Assert.True(orig[pos2[0]][pos2[1]] == 'D' , $"The original map is not correctly showing the unchanged map with the DDit but shows {curr[pos[0] - 1][pos[1]]}.");
            //reaching the DDit
            Assert.True(crawler.GameIsRunning() == Game.GameState.STOP, "The game should finished as the player reached the DDit.");

        }
    }
}
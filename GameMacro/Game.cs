﻿using MongoDB.Bson.Serialization.Attributes;

namespace Dungeon_Crawler.GameMacro
{
    internal class Game
    {
        [BsonId]
        public int Id { get; set; } = 0;
        [BsonIgnore]
        public bool IsRunning = false;
        public Hero Hero { get; set; }

        public LevelData CurrentLevel { get; set; }

        public List<LevelData> Levels { get; set; } = [];

        public Settings Settings { get; set; }
        public Log GameLog { get; set; } = new();

        private AppData AppData;

        public void CreateNewGame(Settings settings, AppData appData)
        {
            Levels.Clear();
            //TODO: Load all levels ;-)
            Settings = settings;

            LevelData levelOne = new LevelData(1, this);

            Hero = new Hero(new Position(), false, "", this);

            levelOne.Load(settings, Hero);

            Levels.Add(levelOne);
            CurrentLevel = levelOne;

            AppData = appData;
        }
        public void RunGameLoop()
        {
            Hero.HasExitedGame = false;
            IsRunning = true;
            while (IsRunning)
            {
                CurrentLevel.NewTurn();
                if (Hero.HasExitedGame)
                {
                    return;
                }
                CurrentLevel.DrawLevel();
                if (!Hero.IsAlive)
                {
                    IsRunning = false;

                    if (Id != 0)
                    {
                        AppData.SavedGames[Id - 1] = null;
                        DataBaseHandler.DeleteGameFromDatabase(this);
                    }
                }
            }
        }

        public void DeMongoGame(AppData appData)
        {
            foreach (var level in Levels)
            {
                level.DeMongoLevel(this);
            }
            var currentLevelNumber = CurrentLevel.levelNumber;

            CurrentLevel = Levels[currentLevelNumber - 1];

            Hero = CurrentLevel.Hero;

            Hero.SetGame(this);

            AppData = appData;

        }
        public void levelElement_LogMessageSent(object sender, LogMessageSentEventArgs e)
        {

            GameLog.AddLogMessage(e.LogMessage);

            if (GameLog.LogMessages.Count <= 1)
            {
                ConsoleWriter.WriteInGameLogMessages(e.LogMessage, null);
            }
            else
            {
                var mostRecentLogMessage = GameLog.LogMessages[GameLog.LogMessages.Count - 2];

                ConsoleWriter.WriteInGameLogMessages(e.LogMessage, mostRecentLogMessage);
            }


            if (!(sender is Hero && e.LogMessage.MessageType == MessageType.Movement))
            {
                Thread.Sleep(400);
            }
        }
        internal void SaveGame()
        {
            if (AppData.SavedGames.Any(g => g?.Id == Id))
            {
                DataBaseHandler.SaveGameToDataBase(this);
            }
            else
            {
                Id = AppData.SelectSaveFileForSaving();

                if (Id != 0)
                {
                    DataBaseHandler.SaveGameToDataBase(this);
                    AppData.SavedGames[Id - 1] = this;
                }
            }

            CurrentLevel.DrawLevel();
        }

        internal void RunInGameMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;

            int currentLogIndex = GameLog.LogMessages.Count;

            ConsoleKeyInfo input = new();

            while (input.Key != ConsoleKey.Escape && input.Key != ConsoleKey.E)
            {
                Console.SetCursorPosition(0, 0);
                Console.Clear();
                Console.WriteLine("########## Menu ##########");
                Console.WriteLine("#                        #");
                Console.WriteLine("# \"L\": Show Log          #");
                Console.WriteLine("#                        #");
                Console.WriteLine("# \"S\": Save Game         #");
                Console.WriteLine("#                        #");
                Console.WriteLine("# \"Escape\": Exit Menu    #");
                Console.WriteLine("#                        #");
                Console.WriteLine("# \"E\": Go to main menu   #");
                Console.WriteLine("#                        #");
                Console.WriteLine("##########################");


                input = Console.ReadKey(true);

                if (input.Key == ConsoleKey.S)
                {
                    SaveGame();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(0, 0);
                    Console.Clear();
                    Console.WriteLine("########## Menu ##########");
                    Console.WriteLine("#                        #");
                    Console.WriteLine("#                        #");
                    Console.WriteLine("#       Game saved!      #");
                    Console.WriteLine("#                        #");
                    Console.WriteLine("#      Press any key     #");
                    Console.WriteLine("#       to continue.     #");
                    Console.WriteLine("#                        #");
                    Console.WriteLine("#                        #");
                    Console.WriteLine("#                        #");
                    Console.WriteLine("##########################");
                    Console.WriteLine();

                    Console.ReadKey();


                    GameLog.AddLogMessage(new LogMessage(Hero, "Game saved", MessageType.Save));
                }
                else if (input.Key == ConsoleKey.E)
                {
                    IsRunning = false;
                    Hero.HasExitedGame = true;
                }
                else if (input.Key == ConsoleKey.L)
                {
                    while (input.Key != ConsoleKey.Backspace)
                    {
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        ConsoleWriter.WriteMaximumTenInMenuLogMessages(GameLog.LogMessages, currentLogIndex);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Press \"UP/DOWN\" to navigate log.");
                        Console.WriteLine("Press \"Backspace\" to go back.");

                        input = Console.ReadKey(true);

                        if (input.Key == ConsoleKey.UpArrow)
                        {
                            if (currentLogIndex - 10 <= 10)
                            {
                                currentLogIndex = 10;
                            }
                            else
                            {
                                currentLogIndex -= 10;
                            }
                        }
                        else if (input.Key == ConsoleKey.DownArrow)
                        {
                            if (currentLogIndex + 10 >= GameLog.LogMessages.Count)
                            {
                                currentLogIndex = GameLog.LogMessages.Count;
                            }
                            else
                            {
                                currentLogIndex += 10;
                            }
                        }
                    }
                }

            }

            Console.Clear();

            if (!Hero.HasExitedGame)
            {
                CurrentLevel.DrawLevel();
            }
        }
    }
}

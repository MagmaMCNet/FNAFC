using GameJolt;
using GameJolt.Objects;
using GameJolt.Services;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;
using System;

#pragma warning disable CS1998
[SupportedOSPlatform("Windows")]
internal class Program
{

    protected static int GameID = 545682;
    protected static string PrivateGameKey = "c281531c10202764e8ab8cf4060eaebf";
	
	public static string AssetLocation = "Assets/"

    static GameJoltApi API = new GameJoltApi(GameID, PrivateGameKey);
    static Response<Credentials> User;

    static string GameState = "Menu"; // Menu, Game, Dead
    static GameData? Config = new GameData();


    class GameData
    {
        public int Night { get; set; }

        public string? UserName { get; set; }
        public string? Token { get; set; }

        public int UpdateDelay { get; set; }
        public string __Comment__UpdateDelay = "Changing This May Make It Break or Become Unstable - Default: 500";

        public bool Debug = true;
        public string __Comment__Debug = "Prints Debug Info Used In Development Process";
    
    }

    static void Debug(string text)
    {
        if (Config.Debug)
            AnsiConsole.Markup("[purple]Debug: [/][darkblue]" + text + "[/]");
    }

    static class WindowScale
    {
        public static async void Game()
        {
            Console.SetWindowSize(
                Math.Min(225, Console.LargestWindowWidth), // Width
                Math.Min(65, Console.LargestWindowHeight)  // Hieght
            );
            Console.Title = "FNAF - C";

            Console.Clear();
            Debug("Changing Window Scale");
            Thread.Sleep(300);
            Console.Clear();

        }

        public static async void Input()
        {
            Console.SetWindowSize(
                Math.Min(120, Console.LargestWindowWidth), // Width
                Math.Min(30, Console.LargestWindowHeight)  // Hieght
            );
            Console.Title = "Input Sreen";

            Console.Clear();
            Debug("Changing Window Scale");
            Thread.Sleep(300);
            Console.Clear();

        }
    }

    static string UserName()
    {
        AnsiConsole.Write(new FigletText("Gamejolt API Login").Color(Color.DarkBlue).Centered());
        return "" + AnsiConsole.Ask<string>("[lime]Enter Gamejolt Username:[/] ") + "";
    }
    static string UserToken()
    {
        return "" + AnsiConsole.Ask<string>("[lime]Enter Gamejolt Token:[/] ") + "";
    }

    public static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        WindowScale.Input();
        if (!File.Exists("FNAFC.json"))
        {
            var name = UserName();
            var token = UserToken();
            Debug("GameJolt API Logging in;");
            User = await API.Users.AuthAsync(name, token);

            Config.UserName = name;
            Config.Token = token;
            Config.UpdateDelay = 500;
            Config.Debug = false;
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true
            };
            File.WriteAllText("FNAFC.json", JsonSerializer.Serialize(Config, options));

        } 
        else
        {
            
            // Config Exists
            Config = JsonSerializer.Deserialize<GameData>(File.ReadAllText("FNAFC.json"));
            Debug("GameJolt API Logging in;");
            User = await API.Users.AuthAsync(Config.UserName, Config.Token);
        }


        

        Debug("GameJolt API Geting Scores;");
        var response = await API.Scores.FetchAsync();

        // Game //
        WindowScale.Game();
        if (User.Success)
        {

            //API.Trophies.SetAchieved(User.Data, 172860);

            AnsiConsole.Markup("[green]" + File.ReadAllText(AssetLocation + @"Menu/Developer.txt") + "[/]");
            Thread.Sleep(2000);
            Console.Clear();
            AnsiConsole.Markup("[green]" + File.ReadAllText(AssetLocation + @"Menu/Publisher.txt") + "[/]");
            Thread.Sleep(2000);
            Console.Clear();

            DrawAbles.Top_Menu();
            AnsiConsole.Markup("[green]" + File.ReadAllText(AssetLocation + @"Menu/Menu_0.txt") + "[/]");
            DrawAbles.Bottom_Menu();
            while (true)
            {
                await Task.Run(Draw);
            }
        }

    }


    static int Slected = 1;
    static string LastFrame = null;
    static Stopwatch DeltaTime = new Stopwatch();
    
    static void KeyPressed(ConsoleKey Key)
    {
		
        if (GameState == "Menu")
        {
            if (Key == ConsoleKey.S || Key == ConsoleKey.DownArrow)
            {
                if (Slected < 2)
                    Slected++;

            }
            if (Key == ConsoleKey.W || Key == ConsoleKey.UpArrow)
            {
                if (Slected > 1)
                    Slected--;
            }
        }

    }
	public static System.Media.SoundPlayer PlayAudio(string AssetAudioLocation, bool loop = false) {
		System.Media.SoundPlayer AudioPlayer = new System.Media.SoundPlayer(AssetLocation + "Audio/" + AssetAudioLocation + @".wav");
		if (loop)
			AudioPlayer.PlayLooping()
		else
			AudioPlayer.Play();

	}
    public class DrawAbles
    {

        public static async void Frame() 
        {
            Console.Clear();
            
        }
        public static async void Menu_0()
        {
            DrawAbles.Top_Menu();
            AnsiConsole.Markup("[green]" + File.ReadAllText( AssetLocation + @"Menu/Menu_0.txt") + "[/]");
            DrawAbles.Bottom_Menu();
        }
		public static class MainMenu() 
        {
            public static async void Top()
            {
                AnsiConsole.Markup(("[cyan]User: [blue]" + User.Data.Name + "[/][/] "));
            }
            public static async void Bottom()
            {
                AnsiConsole.MarkupLine($"[blue]{ (Slected == 1 ? ">" : "") }Press 1 - Start[/]");
                AnsiConsole.MarkupLine($"[blue]{ (Slected == 2 ? ">" : "") }Press 2 - Controls[/]");
            }
            public static class Frames {
                public static string Static() {
                    DrawAbles.MainMenu.Top();
                    AnsiConsole.Markup("[green]" + File.ReadAllText( AssetLocation + @"Menu/Menu_0.txt") + "[/]");
                    DrawAbles.MainMenu.Bottom();
                    return "Menu_0";
                }
                
                public static string Glitch() {
                    Console.Clear();
                    DrawAbles.MainMenu.Top();
                    AnsiConsole.Markup("[green]" + File.ReadAllText( AssetLocation + @"Menu/Menu_1.txt") + "[/]");
                    DrawAbles.MainMenu.Bottom();

                    Thread.sleep(90);
                    Console.Clear();

                    DrawAbles.MainMenu.Top();
                    AnsiConsole.Markup("[green]" + File.ReadAllText( AssetLocation + @"Menu/Menu_2.txt") + "[/]");
                    DrawAbles.MainMenu.Bottom();

                    Thread.sleep(90);
                    Console.Clear();

                    return "Menu_Glitch";
                }
            }
        }
		// Night
		public static class Night 
        {
			public static class Stats {
				public static float Time;
				
				public static float Difficulty_1 = 0.1f;
				public static float Difficulty_2 = 0f;
				public static float	LD_DeathTime = 0;
				public static float	RD_DeathTime = 0;
				
				public static bool LD_Here;
				public static bool RD_Here;
			}
			public static class Posistions {
				// 1, 2
				public static string Stage = "1 2";
				
				public static string PartyRoom = "";
				
				public static string L_Path = "";
				public static string R_Path = "";
				
				public static string L_Door = "";
				public static string R_Door = "";
			}
			public static async void Office() {
				
			}
			
			public static async void Start_Night1()
			{
				Night.Stats.Time = 0;
				
				Night.Stats.Difficulty_1 = 0.5f;
				Night.Stats.Difficulty_2 = 0;
				
				Night.Stats.LD_DeathTime = 0;
				Night.Stats.RD_DeathTime = 0;
				
				Night.Stats.LD_Here = false;
				Night.Stats.RD_Here = false;
				PlayAudio("Night_1");
			}
		} 
    }
	
	static async void Update() {
		float dt = DeltaTime.Elapsed.TotalSeconds;
        DeltaTime.Restart();
        
	}
	
    static async void Draw()
    {
        // Draw
        if (GameState == "Menu")
        {
            Random random = new Random();
            if (random.Next(0, 16) == 1)
            {
                Thread.Sleep(80); // Let Text Write To Screen/FPS
                Console.Clear(); // Clear Screen For Next Frame

                DrawAbles.Top_Menu();
                AnsiConsole.Markup("[green]" + File.ReadAllText(AssetLocation + @"Menu/Menu_1.txt") + "[/]");
                DrawAbles.Bottom_Menu();

                Thread.Sleep(80);
                Console.Clear();

                DrawAbles.Top_Menu();
                AnsiConsole.Markup("[green]" + File.ReadAllText( AssetLocation + @"Menu/Menu_2.txt") + "[/]");
                DrawAbles.Bottom_Menu();

                Thread.Sleep(80);
                Console.Clear(); // Clear Screen For Next Frame

                // Normal
                DrawAbles.Menu_0();



            } else
                // Sleep
                Thread.Sleep(Config.UpdateDelay);

            // Key Wait
            
            while (Console.KeyAvailable)
            {
                if (Console.KeyAvailable) // Non-blocking Input
                {
                    var key = Console.ReadKey(true).Key;
                    // process key
                    KeyPressed(key);
                    DrawAbles.Menu_0();
                }


            }
        }
		else if (GameState == "Game")
		{

			DrawAbles.Night.Office();
		}

    }

}
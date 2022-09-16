using GameJolt;
using GameJolt.Objects;
using GameJolt.Services;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;

#pragma warning disable CS1998
[SupportedOSPlatform("Windows")]
internal class Program
{

    protected static int GameID = 545682;
    protected static string PrivateGameKey = "c281531c10202764e8ab8cf4060eaebf";

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
    }

    static class WindowScale
    {
        public static void Game()
        {
            Console.SetWindowSize(
            Math.Min(225, Console.LargestWindowWidth), // Width
            Math.Min(65, Console.LargestWindowHeight) // Hieght
            );
            Console.Title = "FNAF - C";
        }

        public static void Input()
        {
            Console.SetWindowSize(
            Math.Min(120, Console.LargestWindowWidth), // Width
            Math.Min(30, Console.LargestWindowHeight) // Hieght
            );
            Console.Title = "Input Sreen";
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
            User = await API.Users.AuthAsync(UserName(), UserToken());
            Config.UserName = User.Data.Name;
            Config.Token = User.Data.Token;
            Config.UpdateDelay = 500;
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
            User = await API.Users.AuthAsync(Config.UserName, Config.Token);
        }


        

        var response = await API.Scores.FetchAsync();

        // Game //
        WindowScale.Game();
        Thread.Sleep(500); // Let Window Scale Load
        if (User.Success)
        {

            //API.Trophies.SetAchieved(User.Data, 172860);

            AnsiConsole.Markup("[green]" + File.ReadAllText(@"Assets/Menu/Developer.txt") + "[/]");
            Thread.Sleep(2000);
            Console.Clear();
            AnsiConsole.Markup("[green]" + File.ReadAllText(@"Assets/Menu/Publisher.txt") + "[/]");
            Thread.Sleep(2000);
            Console.Clear();

            DrawAbles.Top_Menu();
            AnsiConsole.Markup("[green]" + File.ReadAllText(@"Assets/Menu/Menu_0.txt") + "[/]");
            DrawAbles.Bottom_Menu();
            while (true)
            {
                await Task.Run(Draw);
            }
        }

    }



    static bool Key_A; // Look - Left Door
    static bool Key_D; // Look - Right Door
    static bool Key_Q; // Close - Left Door
    static bool Key_E; // Close - Right Door

    static int Slected = 1;
    
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

    public class DrawAbles
    {
        public static async void Top_Menu()
        {
            AnsiConsole.Markup(("[cyan]User: [blue]" + User.Data.Name + "[/][/] "));
        }
        public static async void Bottom_Menu()
        {
            AnsiConsole.MarkupLine($"[blue]{ (Slected == 1 ? ">" : "") }Press 1 - Start[/]");
            AnsiConsole.MarkupLine($"[blue]{ (Slected == 2 ? ">" : "") }Press 2 - Controls[/]");
        }

        public static async void Menu_0()
        {
            DrawAbles.Top_Menu();
            AnsiConsole.Markup("[green]" + File.ReadAllText(@"Assets/Menu/Menu_0.txt") + "[/]");
            DrawAbles.Bottom_Menu();
        }
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
                AnsiConsole.Markup("[green]" + File.ReadAllText(@"Assets/Menu/Menu_1.txt") + "[/]");
                DrawAbles.Bottom_Menu();

                Thread.Sleep(80);
                Console.Clear();

                DrawAbles.Top_Menu();
                AnsiConsole.Markup("[green]" + File.ReadAllText(@"Assets/Menu/Menu_2.txt") + "[/]");
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
                if (Console.KeyAvailable) // Non-blocking peek
                {
                    var key = Console.ReadKey(true).Key;
                    // process key
                    KeyPressed(key);
                    DrawAbles.Menu_0();
                }


            }
        }


    }

}
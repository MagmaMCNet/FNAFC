using GameJolt;
using GameJolt.Objects;
using GameJolt.Services;
using Spectre.Console;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;
using System.IO.Compression;
using static Program.DrawAbles.Night;
using static Program.DrawAbles;
using FNAFC;
using Xceed.Wpf.Toolkit;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Text;
#pragma warning disable CS1998
[SupportedOSPlatform("Windows")]

internal class Program
{

    protected static int GameID = 545682;
    protected static string PrivateGameKey = "c281531c10202764e8ab8cf4060eaebf";

    // 





    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

    // https://docs.microsoft.com/en-us/windows/console/handlerroutine?WT.mc_id=DT-MVP-5003978
    private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

    private enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }




    

    public static string AssetLocation = "Data/";


    static GameJoltApi API = new GameJoltApi(GameID, PrivateGameKey);
    static Response<Credentials> User;

    static string GameState = "Dev"; // Menu, Game, Dead
    static GameData? Config = new GameData();

    static bool LoggedIn = false;
    static string UserName = "Offline";

    static AudioManager OfficeBackground = new();
    static AudioManager MenuMusic = new();
    static AudioManager Night_1 = new();



    class GameData
    {
        public int Night { get; set; }

        public string? UserName { get; set; }
        public string? Token { get; set; }

        public int UpdateDelay { get; set; }
        public string __Comment__UpdateDelay = "Changing This May Make It Break or Become Unstable - Default: 500";


        public int DrawTime { get; set; }
        public string __Comment__DrawTime = "Time Between Frames To Alow Time To Draw Text";

        public bool Debug { get; set; }
        public string __Comment__Debug = "Prints Debug Info Used In Development Process";

        public Byte Volume { get; set; }

    }

    static void Debug(string text)
    {
        if (Config.Debug)
            AnsiConsole.MarkupLine ("[purple]Debug: [/][darkblue]" + text + "[/]");
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
            Thread.Sleep(100);
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
            Thread.Sleep(100);
            Console.Clear();

        }
    }

    static class Login
    {
        public static string UserName()
        {
            AnsiConsole.Write(new FigletText("Gamejolt API Login").Color(Color.DarkBlue).Centered());
            return "" + AnsiConsole.Ask<string>("[lime]Enter Gamejolt Username:[/] ") + "";
        }
        public static string UserToken()
        {
            return "" + AnsiConsole.Ask<string>("[lime]Enter Gamejolt Token:[/] ") + "";
        }
    }

    public static void OnLoad()
    {
        OfficeBackground.FileName = "Background_Office";
        MenuMusic.FileName = "Menu";
        //OfficeBackground.Play();
    }
    public static string arg;
    public static bool cheatmode = false;
    public static void Main(string[] args)
    {
        arg = string.Join("", args);
        Console.WriteLine(arg);
        if (arg == "Mode=498c772a8ec47d74b113e2ec16377572") 
        {
            cheatmode = true;
            Console.Title = Console.Title + " - Cheat Mode";
        }
        SetConsoleCtrlHandler(Handler, true);
        Task.Run(() => OnLoad());
        Task.Run(() =>
        {
            AudioUpdate();
        });
            MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        if (!Directory.Exists(AssetLocation))
        {
            DrawAbles.Frame();
            AnsiConsole.Write(new FigletText("Data Directory Error").Centered().Color(Color.DarkRed_1));
            AnsiConsole.MarkupLine("[red]Directory Error Data/ Does not Exist Downloading In Progress...[/]");
            Thread.Sleep(500);
            using (System.Net.WebClient client = new())
            {
                client.DownloadFile("https://cdn.magma-mc.net/Files/FNAFC_Data.zip", "AssetsDownload.temp.zip");
            }
            DrawAbles.Frame();
            AnsiConsole.MarkupLine("[red]Download Complete.[/]");
            AnsiConsole.MarkupLine("[red]Extracting...[/]");
            ZipFile.ExtractToDirectory("AssetsDownload.temp.zip", "Data");
            if (File.Exists("AssetsDownload.temp.zip"))
                File.Delete("AssetsDownload.temp.zip");
            DrawAbles.Frame();
            AnsiConsole.MarkupLine("Assets Found Continuing As normal");
            DrawAbles.Frame(1000);
        }
        // Gamejolt Login
        GameState = "Login";

        WindowScale.Input();
        if (!File.Exists("FNAFC.json"))
        {
            string name = "Offline";
            string token = "null";
            bool DoLogin = (AnsiConsole.Ask<string>("Login To Gamejolt Api {Y/N}").ToUpper() == "Y" ? true : false);
            if (DoLogin)
            {

                name = Login.UserName();
                token = Login.UserToken();
                Debug("GameJolt API Logging in;");
                User = await API.Users.AuthAsync(name, token);

                Config.UserName = name;
                Config.Token = token;
                Config.UpdateDelay = 500;
                Config.DrawTime = 100;
                Config.Debug = false;
                Config.Volume = 80;

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    WriteIndented = true
                };
                File.WriteAllText("FNAFC.json", JsonSerializer.Serialize(Config, options));
            } else
            {
                User = await API.Users.AuthAsync(name, token);
                Config.UpdateDelay = 500;
                Config.DrawTime = 100;
                Config.Debug = false;
                Config.Volume = 80;
            }

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
            LoggedIn = true;
            UserName = User.Data.Name;
            API.Trophies.SetAchieved(User.Data, 173212);
            //API.Trophies.SetAchieved(User.Data, 172860);
        } else
        {
            AnsiConsole.Write(new FigletText("Login Error").Centered().Color(Color.Red1));
            string Continue = AnsiConsole.Ask<string>("[blue]Continue With Out Being Logged In? {Y[gray]/[/][cyan]N[/]}: [/]");
            if (File.Exists("FNAFC.json"))
                File.Delete("FNAFC.json");
            Console.WriteLine(Continue);
            if (Continue.ToUpper() != "Y")
            {
                Environment.Exit(2);
            }
            DrawAbles.Frame();
        }
        AnsiConsole.Markup("[green]" + File.ReadAllText(AssetLocation + @"Menu/Developer.txt") + "[/]");
        DrawAbles.Frame(2200);

        AnsiConsole.Markup("[green]" + File.ReadAllText(AssetLocation + @"Menu/Publisher.txt") + "[/]");
        DrawAbles.Frame(2200);
        GameState = "Menu";

        DrawAbles.MainMenu.Frames.Static();
        while (true)
        {
            await Task.Run(Draw);
        }

    }



    static int Slected = 1;
    static string LastFrame = null;
    static Stopwatch DeltaTime = new Stopwatch();
    static double dt = 0;
    static string Clock = "12AM";



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
            if (Key == ConsoleKey.O)
            {
                Console.Clear();
                GameState = "Dead";
            }
            if (Key == ConsoleKey.Enter || Key == ConsoleKey.RightArrow)
            {
                if (Slected == 1)
                {
                    Console.Clear();
                    Start_Night1();
                } else if (Slected == 2)
                {
                    Console.Clear();
                    GameState = "Controls";
                }

            }

            DrawAbles.MainMenu.Frames.Static(true);
        }
        else if (GameState == "Office")
        {
            if (Key == ConsoleKey.Q)
            {
                // Close/Open Left Door
                DrawAbles.Night.Stats.Office.State = "L_Door";
            }
            else if (Key == ConsoleKey.A)
            {
                // Light Left Door
                DrawAbles.Night.Stats.Office.State = "L_Look";
            }
            else if (Key == ConsoleKey.E)
            {
                // Close/Open Right Door
                DrawAbles.Night.Stats.Office.State = "R_Door";
            }
            else if (Key == ConsoleKey.D)
            {
                // Light Right Door
                DrawAbles.Night.Stats.Office.State = "R_Look";
            }
        }

    }
    public static void PlayLooped(AudioManager NetCoreAudioPlayer, string AssetAudioLocation, bool IfTrue)
    {
        if (!IfTrue)
        {
            NetCoreAudioPlayer.Stop();
            return;
        }
        else if (!NetCoreAudioPlayer.Playing)
        {
            NetCoreAudioPlayer.Play(AssetLocation + @"Audio/" + AssetAudioLocation + @".mp3");
        }
     }
    public static AudioManager PlayAudio(string AssetAudioLocation, bool loop = false)
    {
        AudioManager AudioPlayer = new();
        AudioPlayer.Play(AssetAudioLocation, loop);
        return AudioPlayer;

    }
    public class DrawAbles
    {

        public static async void Frame(int add = 0)
        {
            Thread.Sleep(Config.DrawTime + add);
            Console.Clear();
        }

        public static void DrawImage(string Type, string AsciiImage, string Color = "green")
        {
            AnsiConsole.Markup($"[{Color}]" + File.ReadAllText($"{AssetLocation}{Type}/{AsciiImage}.txt") + "[/]");
        }

        public static void Controls(bool Force = false)
        {
            if (LastFrame == "Controls" && !Force)
                return;
            Frame();
            AnsiConsole.Write(new FigletText("Controls").Centered().Color(Color.Blue3_1));
            AnsiConsole.MarkupLine("[cyan]Left:[/]");
            AnsiConsole.MarkupLine("[blue]    Q - Close Door[/]");
            AnsiConsole.MarkupLine("[blue]    A - Light[/]");
            AnsiConsole.MarkupLine("[cyan]Right:[/]");
            AnsiConsole.MarkupLine("[blue]    E - Close Door[/]");
            AnsiConsole.MarkupLine("[blue]    D - Light[/]");
            Console.WriteLine();
            AnsiConsole.Markup("[red]> Press Any Key To Continue[/]");
            Console.ReadKey();
            GameState = "Menu";
            LastFrame = "Controls";
        }

        public static class MainMenu
        {
            public static async void Top()
            {
                Console.Clear();
                AnsiConsole.Markup(("[cyan]User: [blue]" + UserName + "[/][/] "));
            }
            public static async void Bottom()
            {
                AnsiConsole.MarkupLine($"[blue][cyan]{(Slected == 1 ? ">" : " ")}[/] - Start[/]");
                AnsiConsole.MarkupLine($"[blue][cyan]{(Slected == 2 ? ">" : " ")}[/] - Controls[/]");
            }
            public static class Frames
            {
                public static void Static(bool Force = false)
                {
                    if (LastFrame == "Menu_Static" && !Force)
                        return;
                    LastFrame = "Menu_Static";

                    Top();
                    DrawImage("Menu", "Menu_0");
                    Bottom();
                }
                public static void CheatMod()
                {
                    if (LastFrame == "Menu_StaticS")
                        return;
                    LastFrame = "Menu_StaticS";
                    Console.Clear();;
                    AnsiConsole.Markup($"[lime]" + File.ReadAllText($"Data/Extra/DevMode.txt")
                        .Replace("%GAMEJOLT%", LoggedIn.ToString() + ("%GAMEJOLT%".Length > LoggedIn.ToString().Length ? string.Concat(Enumerable.Repeat(" ",(("%GAMEJOLT%".Length - LoggedIn.ToString().Length)))) : ""))
                        .Replace("%USERNAME%", Config.UserName + ("%USERNAME%".Length > Config.UserName.Length ? string.Concat(Enumerable.Repeat(" ", (("%USERNAME%".Length - Config.UserName.Length)))) : ""))
                        .Replace("0.5F", $"{(float)Config.UpdateDelay/1000}F") + "[/]");
                    Console.WriteLine();
                    Bottom();
                }

                public static void Glitch(bool Force = false)
                {
                    if (LastFrame == "Menu_Glitch" && !Force)
                        return;
                    LastFrame = "Menu_Glitch";

                    Top();
                    DrawImage("Menu", "Menu_1");
                    Bottom();

                    Frame(20);

                    Top();
                    DrawImage("Menu", "Menu_2");
                    Bottom();

                    Frame(20);

                }
            }
        }

        // Night
        public static class Night
        {
            public static class Controls
            {
                public static void Update()
                {
                    Stats.Controls_1_MoveTimer += (dt * Stats.Difficulty_1);
                    Stats.Controls_2_MoveTimer += (dt * Stats.Difficulty_2);
                    Stats.Time += dt*2;
                    // 1
                    if ((Stats.Controls_1_MoveTimer) > 10f)
                    {
                        if (Posistions.Stage.Contains(" 1 "))
                        {
                            Posistions.Stage = Posistions.Stage.Replace(" 1 ", "");
                            Posistions.PartyRoom += " 1 ";
                        }

                        else if (Posistions.PartyRoom.Contains(" 1 "))
                        {
                            Posistions.PartyRoom = Posistions.PartyRoom.Replace(" 1 ", "");
                            Posistions.L_Path += " 1 ";
                        }

                        else if (Posistions.L_Path.Contains(" 1 "))
                        {
                            Posistions.L_Path = Posistions.L_Path.Replace(" 1 ", "");
                            Posistions.L_Door += " 1 ";
                        }
                        else if (Posistions.L_Door.Contains(" 1 "))
                        {
                            if (Stats.L_Door)
                            {
                                Posistions.L_Door = Posistions.L_Door.Replace(" 1 ", "");
                                Posistions.Stage += " 1 ";
                            } else
                            {
                                Posistions.L_Door = Posistions.L_Door.Replace(" 1 ", "");
                                Posistions.Stage += " 1 ";
                                GameState = "Dead";
                            }
                        }
                        Stats.Controls_1_MoveTimer = 0;
                    }



                    // 2
                    if ((Stats.Controls_2_MoveTimer) > 10f)
                    {
                        if (Posistions.Stage.Contains(" 2 "))
                        {
                            Posistions.Stage = Posistions.Stage.Replace(" 2 ", "");
                            Posistions.PartyRoom += " 2 ";
                        }

                        else if (Posistions.PartyRoom.Contains(" 2 "))
                        {
                            Posistions.PartyRoom = Posistions.PartyRoom.Replace(" 2 ", "");
                            Posistions.L_Path += " 2 ";
                        }

                        else if (Posistions.L_Path.Contains(" 2 "))
                        {
                            Posistions.L_Path = Posistions.L_Path.Replace(" 2 ", "");
                            Posistions.L_Door += " 2 ";
                        }
                        else if (Posistions.L_Door.Contains(" 2 "))
                        {
                            if (Stats.L_Door)
                            {
                                Posistions.L_Door = Posistions.L_Door.Replace(" 2 ", "");
                                Posistions.Stage += " 2 ";
                            }
                            else
                            {
                                Posistions.L_Door = Posistions.L_Door.Replace(" 2 ", "");
                                Posistions.Stage += " 2 ";
                                GameState = "Dead";
                            }
                        }
                        Stats.Controls_2_MoveTimer = 0;
                    }
                }
            }

            public static class Stats
            {
                public static double Time;

                public static float Difficulty_1 = 0.1f;
                public static float Difficulty_2 = 0f;
                public static double Controls_1_MoveTimer = 0;
                public static double Controls_2_MoveTimer = 0;

                public static float LD_DeathTime = 0;
                public static float RD_DeathTime = 0;

                public static bool LD_Here;
                public static bool RD_Here;

                public static bool L_Door;
                public static bool D_Door;

                public static class Office
                {
                    public static string State = "Static";

                }
                /*
                 * Static - Main Frame No Visible Entites
                 * L_Here - Left Door Visible Entites
                 * R_Here - Right Door Visible Entites
                 * L_Door - Left Door Closed
                 * R_Door - Right Door Closed
                 */
            }
            public static class Posistions
            {
                // 1, 2
                public static string Stage = " 1 2 ";

                public static string PartyRoom = "";

                public static string L_Path = "";
                public static string R_Path = "";

                public static string L_Door = "";
                public static string R_Door = "";
            }


            public static async void Bottom(bool Force = false)
            {
                if (!Force)
                {
                    if (Stats.Time / 60 >= 6)
                    {
                        if (Clock != "6AM")
                        {
                            Config.Night += 1;
                            LastFrame = "Update";
                        }
                        Clock = "6AM";
                        Thread.Sleep(800);
                        GameState = "6AM";
                    }
                    else if (Stats.Time / 60 >= 5)
                    {
                        if (Clock != "5AM")
                        {
                            LastFrame = "Update";
                        }
                        Clock = "5AM";
                    }
                    else if (Stats.Time / 60 >= 4)
                    {
                        if (Clock != "4AM")
                        {
                            LastFrame = "Update";
                        }
                        Clock = "4AM";
                    }
                    else if (Stats.Time / 60 >= 3)
                    {
                        if (Clock != "3AM")
                        {
                            LastFrame = "Update";
                        }
                        Clock = "3AM";
                    }
                    else if (Stats.Time / 60 >= 2)
                    {
                        if (Clock != "2AM")
                        {
                            LastFrame = "Update";
                        }
                        Clock = "2AM";
                    }
                    else if (Stats.Time / 60 >= 1)
                    {
                        if (Clock != "1AM")
                        {
                            LastFrame = "Update";
                        }
                        Clock = "1AM";
                    }
                }
                else if (Force)
                    AnsiConsole.Markup($"[lime]Time: {Clock} > [/]");
            }
            public static async void Office(bool Force = false)
            {
                if (Stats.Office.State == "Static")
                {

                    
                    if (LastFrame == "Office_Static" && !Force)
                        return;
                    LastFrame = "Office_Static";
                    Frame(10);
                    DrawImage("Office", "Static", "blue");
                    Bottom(true);
                }
                else if (Stats.Office.State == "L_Look")
                {
                    if (LastFrame == "L_Look" && !Force)
                        return;
                    LastFrame = "L_Look";
                    if (Posistions.L_Door.Contains(" 1 "))
                    {
                        Frame();
                        DrawImage("Office", "L_Here", "blue");
                    } else
                    {
                        Frame();
                        DrawImage("Office", "Static", "blue"); 
                        LastFrame = "Look";
                    }

                    AudioManager LightAudio = PlayAudio("Light");
                    Thread.Sleep(1000);
                    LightAudio.Stop();
                    
                    Stats.Office.State = "Static"; 
                }
                else if (Stats.Office.State == "L_Door")
                {
                    if (LastFrame == "L_Door" && !Force)
                        return;
                    LastFrame = "L_Door";
                    if (Posistions.L_Door.Contains(" 1 "))
                    {
                        Posistions.L_Door = Posistions.L_Door.Replace(" 1 ", "");
                        Posistions.Stage += " 1 ";
                    }
                    Frame();
                    DrawImage("Office", "L_Door", "blue");

                    AudioManager Door = PlayAudio("Door");
                    Thread.Sleep(2000);
                    Door.Stop();
                    Door = PlayAudio("Door");
                    Thread.Sleep(200);
                    Door.Stop();

                    Stats.Office.State = "Static";

                }

                // Right

                else if (Stats.Office.State == "R_Look")
                {
                    if (LastFrame == "R_Look" && !Force)
                        return;
                    LastFrame = "R_Look";

                    if (Posistions.L_Door.Contains(" 2 "))
                    {
                        Frame();
                        DrawImage("Office", "R_Here", "blue");
                    }
                    else
                    {
                        Frame();
                        DrawImage("Office", "Static", "blue");
                        LastFrame = "Look";
                    }

                    AudioManager LightAudio = PlayAudio("Light");
                    Thread.Sleep(1000);
                    LightAudio.Stop();

                    Stats.Office.State = "Static";
                }
                else if (Stats.Office.State == "R_Door")
                {
                    if (LastFrame == "R_Door" && !Force)
                        return;
                    LastFrame = "R_Door";
                    if (Posistions.L_Door.Contains(" 2 "))
                    {
                        Posistions.L_Door = Posistions.L_Door.Replace(" 2 ", "");
                        Posistions.Stage += " 2 ";
                    }
                    Frame();
                    DrawImage("Office", "R_Door", "blue");

                    AudioManager Door = PlayAudio("Door");
                    Thread.Sleep(2000);
                    Door.Stop();
                    Door = PlayAudio("Door");
                    Thread.Sleep(200);
                    Door.Stop();

                    Stats.Office.State = "Static";

                }

            }

            public static async void Start_Night1()
            {
                GameState = "Office";
                Stats.Time = 0;

                Stats.Difficulty_1 = 1f;
                Stats.Difficulty_2 = 0;

                Stats.LD_DeathTime = 0;
                Stats.RD_DeathTime = 0;

                Stats.LD_Here = false;
                Stats.RD_Here = false;
                Night_1 = PlayAudio("Night_1");
            }

            public static async void Start_Night2()
            {
                GameState = "Office";
                Stats.Time = 0;

                Stats.Difficulty_1 = 1.4f;
                Stats.Difficulty_2 = 0.2f;

                Stats.LD_DeathTime = 0;
                Stats.RD_DeathTime = 0;

                Stats.LD_Here = false;
                Stats.RD_Here = false;
                PlayAudio("Night_2");
            }
        }
    }
    static async void Update()
    {
        dt = (float)DeltaTime.Elapsed.TotalSeconds;
        DeltaTime.Restart();

    }
    static void AudioUpdate()
    {
        while (true)
        {
            if (GameState == "Office")
                OfficeBackground.Play("", true);
            else
                OfficeBackground.Stop();

            if (GameState == "Menu" || GameState == "Controls")
                MenuMusic.Play("", true);
            else
                MenuMusic.Stop();
            Thread.Sleep(150);
        }
    }
    static async void Draw()
    {
        Update();
        Debug(dt.ToString());
        // Draw

        if (GameState == "Menu")
        {
            Random random = new Random();
            if (random.Next(0, 24) == 1 && !cheatmode)
            {
                MainMenu.Frames.Glitch();

            }
            if (!cheatmode)
                MainMenu.Frames.Static();
            else
                MainMenu.Frames.CheatMod();
            // Sleep
            Thread.Sleep(Config.UpdateDelay);
        }
        else if (GameState == "Office")
        {
            Bottom();
            Office();
            Night.Controls.Update();
            Thread.Sleep(Config.UpdateDelay);
        }
        else if (GameState == "Dead")
        {
            try
            {
                Night_1.Stop();
            }
            catch(Exception E)
            {
                Debug(E.ToString());
            }
            Clock = "12AM";
            Frame();
            AudioManager DS = PlayAudio("DeathScreen");
            if (LoggedIn)
                API.Trophies.SetAchieved(User.Data, 173210);

            for (int i = 0; i < 2.5 * Config.DrawTime; i++)
            {
                LastFrame = "DeathScreen";
                AnsiConsole.Markup("[red] [/]");
                Thread.Sleep(Config.DrawTime / 10);
                AnsiConsole.Markup("[red]---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------[/]");
                Thread.Sleep(Config.DrawTime / 10);
                AnsiConsole.Markup("[red]----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------[/]");
            }
            DS.Stop();
            Frame(100);
            Frame(200);
            GameState = "Menu";
        } else if (GameState == "6AM")
        {

            Night_1.Stop();
            Frame(100);

            if (LoggedIn)
                API.Trophies.SetAchieved(User.Data, 173208);
            PlayAudio("6AM");
            DrawAbles.Frame(500);
            DrawAbles.DrawImage("Extra", "5AM");
            DrawAbles.Frame(8800);
            API.Trophies.SetAchieved(User.Data, 172860);
            DrawAbles.DrawImage("Extra", "6AM");
            DrawAbles.Frame(3500);
            GameState = "Menu";
        }
        else if (GameState == "Controls")
        {
            DrawAbles.Controls();
        }
        while (Console.KeyAvailable)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                // process key
                KeyPressed(key);

            }
        }
    }
    private static bool Handler(CtrlType signal)
    {
        switch (signal)
        {
            case CtrlType.CTRL_BREAK_EVENT:
            case CtrlType.CTRL_C_EVENT:
            case CtrlType.CTRL_LOGOFF_EVENT:
            case CtrlType.CTRL_SHUTDOWN_EVENT:
            case CtrlType.CTRL_CLOSE_EVENT:
                // TODO Cleanup resources
                foreach(var pro in Process.GetProcessesByName("AudioManager"))
                {
                    pro.Kill();
                }
                Console.Clear();
                AnsiConsole.Write(new FigletText("Closing Audio Controllers").Centered().Color(Color.Blue3_1));
                Thread.Sleep(500);
                Environment.Exit(0);
                return false;

            default:
                return false;
        }
    }

}
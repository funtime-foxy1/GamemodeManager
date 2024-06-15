using GamemodeManagerAPI.Mods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.GameFile
{
    internal class GameModeFile
    {
        static string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool DoesExist()
        {
            return File.Exists(Path.Combine(assemblyLocation, "gamemodes"));
        }

        public static List<string> GetAllGamemodes()
        {
            StreamReader reader = File.OpenText(Path.Combine(assemblyLocation, "gamemodes"));
            var currentLine = "";
            while (!currentLine.Contains("*start"))
            {
                currentLine = reader.ReadLine();
            }
            List<string> res = new List<string>();
            for (int i = 0; i < InstalledGamemodeLength(); i++)
            {
                currentLine = reader.ReadLine();
                string[] tokens = currentLine.Split(' ');
                res.Add(tokens[0]);
            }
            reader.Close();
            return res;
        }

        public static void RemoveGamemode(string GUID)
        {
            var Lines = File.ReadAllLines(Path.Combine(assemblyLocation, "gamemodes"));
            var newLines = Lines.Where(line => !line.Contains(GUID));
            File.WriteAllLines(Path.Combine(assemblyLocation, "gamemodes"), newLines);
        }

        public static GamemodeResult GetGamemode(string GUID)
        {
            StreamReader reader = File.OpenText(Path.Combine(assemblyLocation, "gamemodes"));
            var gamemode_ = new GamemodeResult(GUID);
            var currentLine = "";
            while (!currentLine.Contains("*start"))
            {
                currentLine = reader.ReadLine();
            }
            for (int i = 0; i < InstalledGamemodeLength(); i++)
            {
                currentLine = reader.ReadLine();
                string[] tokens = currentLine.Split(' ');
                if (tokens[0] == GUID)
                {
                    gamemode_.success = true;
                    gamemode_.results = JsonConvert.DeserializeObject<Dictionary<string, object>>(tokens[1]);
                    gamemode_.gamemode.data = gamemode_.results;
                    break;
                }
            }
            reader.Close();
            if (!gamemode_.success) Plugin.Log.LogWarning("Couldn't find gamemode: " + GUID);
            return gamemode_;
        }
        public static bool CreateGamemode(string author, string name, Dictionary<string, object> data)
        {
            StreamWriter write = File.AppendText(Path.Combine(assemblyLocation, "gamemodes"));
            write.WriteLine($"{author}.{name} " + JsonConvert.SerializeObject(data));
            write.Close();
            return true;
        }
        public static bool CreateGamemodeRaw(string GUID, Dictionary<string, object> data)
        {
            StreamWriter write = File.AppendText(Path.Combine(assemblyLocation, "gamemodes"));
            write.WriteLine($"{GUID} " + JsonConvert.SerializeObject(data));
            write.Close();
            return true;
        }

        public static int InstalledGamemodeLength()
        {
            StreamReader reader = File.OpenText(Path.Combine(assemblyLocation, "gamemodes"));
            string currentLine = "";
            int gamemodeLength = 0;
            while (!currentLine.Contains("*start"))
            {
                currentLine = reader.ReadLine();
            }
            while (!reader.EndOfStream)
            {
                currentLine = reader.ReadLine();
                gamemodeLength++;
            }
            reader.Close();
            return gamemodeLength;
        }
    }
}

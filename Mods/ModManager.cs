using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.Mods
{
    public class ModManager
    {
        public static List<GamemodeMod> mods = new List<GamemodeMod>();

        public static bool AddMod(GamemodeMod plugin)
        {
            mods.Add(plugin);
            return true;
        }
    }
}

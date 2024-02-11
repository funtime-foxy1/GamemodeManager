using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.Mods
{
    public abstract class GamemodeMod : BaseUnityPlugin
    {
        protected bool ModActive { get; set; }
        protected bool RegisterMod()
        {
            if (ModManager.mods.Contains(this))
            {
                return false;
            }
            bool res = ModManager.AddMod(this);
            Plugin.Log.LogWarning("Mod: " + this.Info.Metadata.GUID + " has been loaded: " + res);
            return res;
        }

        //constructor
        protected GamemodeMod()
        {
            RegisterMod();
        }
    }
}

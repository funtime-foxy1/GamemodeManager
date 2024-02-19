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
        public bool ModActive { get; set; }
        public Dictionary<string, object> settings = new Dictionary<string, object>();
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
        public object GetSetting(string key)
        {
            return settings[key];
        }
        protected void SetSetting(string key, string value)
        {
            settings[key] = value;
        }

        //constructor
        protected GamemodeMod()
        {
            RegisterMod();
        }
    }
}

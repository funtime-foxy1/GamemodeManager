using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApi.Classes;

namespace GamemodeManager.Util
{
    internal class TerminalUtil
    {
        public static void AddCommand(string cmdName, Func<string> onCall)
        {
            TerminalApi.TerminalApi.AddCommand(cmdName, new CommandInfo()
            {
                Category = "Gamemode",
                DisplayTextSupplier = onCall
            });
        }
    }
}

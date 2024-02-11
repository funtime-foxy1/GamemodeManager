using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.GameFile
{
    internal class FileExplorer
    {
        public static void ShowExplorer(string itemPath)
        {
            
            itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
            System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
        }
    }
}

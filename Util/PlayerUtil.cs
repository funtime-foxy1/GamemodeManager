using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.Util
{
    internal class PlayerUtil
    {
        public static bool IsHost()
        {
            return RoundManager.Instance.NetworkManager.IsHost;
        }
    }
}

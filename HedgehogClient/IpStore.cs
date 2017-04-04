using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogClient {
    internal static class IpStore {
        public static void Save(string ip) {
            File.WriteAllText("ip.txt", ip);
        }

        public static string Load() {
            if(File.Exists("ip.txt"))
                return File.ReadAllText("ip.txt");
            return "";
        }
    }
}

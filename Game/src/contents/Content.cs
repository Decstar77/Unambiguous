using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Content
    {
        public static string BasePath = "C:/Projects/CS/Game/Content/";
        
        public static string ResolvePath(string path) {
            return BasePath + path;
        }
        
    }
}

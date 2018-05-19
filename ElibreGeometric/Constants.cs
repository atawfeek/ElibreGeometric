using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibreGeometric
{
    public static class Constants
    {
        private static decimal _nudOffset;
        private static int _nudCount;

        public static decimal NudOffset
        {
            get { return 0; }
            set { _nudOffset = value; }
        }

        public static int NudCount
        {
            get { if (_nudCount == 0) return 5; else return _nudCount; }
            set { _nudCount = value; }
        }
    }
}

using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibreGeometric
{
    public static class Bootstrapper
    {
        public static void Init()
        {
            DependencyInjector.Register<ILogger, Logger>();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log
{
    public interface ILogger
    {
        void Log(Exception ex);
    }
}

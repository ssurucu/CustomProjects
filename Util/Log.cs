using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public static class Log
    {
        //write to log file
        public static void SaveLog(string logText)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"..\..\..\..\P2-BPC\Util\bca-log.txt", true);
            file.WriteLine(logText);
            file.Close();


        }

    }
}

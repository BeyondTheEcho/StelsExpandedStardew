using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StelsExpandedStardew
{
    internal static class SMAPI_Utils
    {
        public static IMonitor? m_Monitor { get; set; }

        public static bool TryGetTileProperty(ButtonPressedEventArgs e, out string property)
        {
            property = Game1.currentLocation.doesTileHaveProperty((int)e.Cursor.GrabTile.X, (int)e.Cursor.GrabTile.Y, "Action", "Buildings");

            return property != null;
        }

        public static bool TryExtractIDFromQualified(string input, out int number)
        {
            const string pattern = @"\(O\)(\d+)";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string numberString = match.Groups[1].Value;
                if (int.TryParse(numberString, out number))
                {
                    return true;
                }
            }

            number = 0;
            return false;
        }


        public static void Log(string logMessage)
        {
            m_Monitor?.Log(logMessage, LogLevel.Debug);
        }

        public static void LogError(string logMessage)
        {
            m_Monitor?.Log(logMessage, LogLevel.Warn);
        }
    }
}

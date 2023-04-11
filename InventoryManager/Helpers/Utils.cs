using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace InventoryManagerAPI.Helpers
{
    public enum LogLevel
    {
        VERBOSE = 6,
        DEBUG = 5, 
        INFO = 4, 
        WARNING = 3, 
        ERROR = 2, 
        FATAL = 1
    }

    public class Utils
    {
        #region - Logging

        //Controls the current log level
        public static int _logLevel = 6;
        public static string Log(LogLevel level, string message)
        {
            string msg = LogMessage(level, message);

            //Output only if the current log level is higher or equal than the log level of the message
            if (_logLevel >= (int)level)
            {
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
            }

            return msg;
        }

        public static string Log(Exception exception)
        {
            //Custom message for errors
            string msg = MultiLineTrim(LogMessage(LogLevel.ERROR, $@"
                Message:
                {exception.Message}
                --------------------
                Inner Exception:
                {exception.InnerException}
                --------------------
                Stack Trace:
                {exception.StackTrace}
            "));

            //Output only if the current log level is higher or equal than the log level of the message
            if (_logLevel >= (int)LogLevel.ERROR)
            {
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
            }

            return msg;
        }

        private static string LogMessage(LogLevel level, string message)
        {
            string logLevel = Enum.GetName(typeof(LogLevel), level);

            return $"[{DateTime.Now.ToString("u")}][{logLevel}] {message}";
        }

        #endregion - Logging

        #region - String

        public static string MultiLineTrim(string input)
        {
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var trimmedLines = lines.Select(line => line.Trim());
            return string.Join(Environment.NewLine, trimmedLines);
        }



        #endregion - String

        #region - Authorization

        /// <summary>
        /// Determines whether the given action matches the specified pattern.
        /// Supports multiple wildcards, such as:
        /// '*' matches all actions,
        /// '/inventory/*' matches actions starting with '/inventory/',
        /// '/user/*/roles/*' matches actions with the format '/user/{userId}/roles/{action}',
        /// '/product/read' matches the exact action '/product/read'.
        /// </summary>
        /// <param name="action">The action to be checked.</param>
        /// <param name="pattern">The pattern to match the action against.</param>
        /// <returns>True if the action matches the pattern, otherwise false.</returns>
        public static bool MatchesPattern(string action, string pattern)
        {
            if (pattern == "*")
            {
                return true;
            }
            else
            {
                // Replace each wildcard with a regex pattern that matches one or more characters
                string regexPattern = pattern.Replace("*", ".*");

                // Create a regex object with the modified pattern and set RegexOptions.IgnoreCase for case-insensitive matching
                Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

                // Check if the action matches the regex pattern
                return regex.IsMatch(action);
            }
        }

        #endregion - Authorization
    }
}

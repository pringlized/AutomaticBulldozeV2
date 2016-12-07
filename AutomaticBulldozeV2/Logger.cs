using System;
using UnityEngine;

namespace AutomaticBulldozeV2
{
    internal static class Logger
    {
        private const string Prefix = "Automatic Bulldoze V2: ";

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(Func<string> acquire)
        {
            var message = acquire();
            Debug.Log(Prefix + "(DEBUG) " + message);
        }

        public static void Log(string message, params object[] args)
        {
            var msg = Prefix + string.Format(message, args);
            Debug.Log(msg);
        }

        public static void LogWarning(string message, params object[] args)
        {
            var msg = Prefix + string.Format(message, args);
            Debug.LogWarning(msg);
        }

        public static void LogError(string message, params object[] args)
        {
            var msg = Prefix + string.Format(message, args);
            Debug.LogError(msg);
        }
    }
}

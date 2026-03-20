using System.Diagnostics;

namespace KylesUnityLib
{
    internal static class Logger
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        [Conditional("DEBUG")]
        public static void LogWarning(string message) 
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("DEBUG")]
        public static void LogWarning(string message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

    }
}

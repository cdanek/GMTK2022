using System;
using System.Diagnostics;

namespace KaimiraGames.GameJam
{
    public static class Logging
    {
        private const bool DEBUG_LOGGING_ENABLED = true;
        private const bool ERROR_LOGGING_ENABLED = true;
        private const bool INFO_LOGGING_ENABLED = true;
        private const bool VERBOSE_LOGGING_ENABLED = true;
        private const bool WARNING_LOGGING_ENABLED = true;
        private const string DEBUG_COLOR = "#6666FFFF";
        private const string ERROR_COLOR = "red";
        private const string INFO_COLOR = "ccccccff";
        private const string VERBOSE_COLOR = "#ccccccff";
        private const string WARNING_COLOR = "brown";

        public static event Action<string> OnLog;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void d()
        {
            if (DEBUG_LOGGING_ENABLED) d(GetClassAndMethod());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void d(string s) // Debug Logging
        {
            OnLog?.Invoke(s);
            if (DEBUG_LOGGING_ENABLED)
            {
                UnityEngine.Debug.Log($"<color={DEBUG_COLOR}>Debug: {s}</color>");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void e()
        {
            if (ERROR_LOGGING_ENABLED) e(GetClassAndMethod());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void e(string s) // Error Logging - pauses editor when Error Pause is enabled.
        {
            OnLog?.Invoke(s);
            if (ERROR_LOGGING_ENABLED)
            {
                UnityEngine.Debug.LogError($"<color={ERROR_COLOR}>ERROR: {s}</color>"); // Pauses the Unity editor if "Error Pause" is enabled in the console editor.
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void i()
        {
            if (INFO_LOGGING_ENABLED) i(GetClassAndMethod());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void i(string s)  // Info Logging
        {
            OnLog?.Invoke(s);
            if (INFO_LOGGING_ENABLED)
            {
                UnityEngine.Debug.Log($"<color={INFO_COLOR}>Info: {s}</color>");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void v()
        {
            if (VERBOSE_LOGGING_ENABLED) v(GetClassAndMethod());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void v(string s)  // Verbose Logging
        {
            OnLog?.Invoke(s);
            if (VERBOSE_LOGGING_ENABLED)
            {
                UnityEngine.Debug.Log($"<color={VERBOSE_COLOR}>Verbose: {s}</color>");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void w()
        {
            if (WARNING_LOGGING_ENABLED) w(GetClassAndMethod());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
        public static void w(string s)  // Warning Logging
        {
            OnLog?.Invoke(s);
            if (WARNING_LOGGING_ENABLED)
            {
                UnityEngine.Debug.LogWarning($"<color={WARNING_COLOR}>WARN: {s}</color>");
            }
        }

        private static string GetClassAndMethod()
        {
            StackTrace stackTrace = new();
            StackFrame frame = stackTrace.GetFrame(2); // get the frame of the most recent caller
            string s = frame.GetMethod().DeclaringType.Name + "::" + frame.GetMethod().Name + "(";
            foreach (System.Reflection.ParameterInfo pi in frame.GetMethod().GetParameters())
            {
                s += pi.ParameterType.ToString() + " " + pi.Name + ",";
            }
            s += ")";
            return s;
        }


    }
}

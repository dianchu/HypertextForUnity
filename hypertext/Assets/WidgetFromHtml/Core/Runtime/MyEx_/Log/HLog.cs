using System;

namespace WidgetFromHtml.Core
{
    public static class HLog
    {
        public static Action<object> LogDelegate =
#if IN_RIDER
            Console.WriteLine;
#else
            UnityEngine.Debug.Log;
#endif

        public static Action<object> LogErrorDelegate =
#if IN_RIDER
            Console.WriteLine;
#else
            UnityEngine.Debug.LogError;
#endif


        public static void LogInfo(object obj)
        {
            LogDelegate?.Invoke($"[Info][WidgetFromHtml]:{obj}");
        }

        public static void LogError(object obj)
        {
            LogErrorDelegate?.Invoke($"[Error][WidgetFromHtml]:{obj}");
        }

        public static void Ex(string msg)
        {
            throw new Exception(msg
            );
        }
    }
}
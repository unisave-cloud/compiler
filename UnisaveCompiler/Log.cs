using System;

namespace UnisaveCompiler
{
    /// <summary>
    /// Formats console logging
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Should log messages be colored?
        /// </summary>
        public static bool UseColors { get; set; } = true;

        private static void Print(string type, string message)
        {
            string now = DateTime.Now.ToString("yyyy-dd-MM H:mm:ss");
            
            Console.WriteLine($"[{now}] {type} {message}");
        }
        
        public static void Debug(string message)
        {
            if (UseColors)
                Console.ForegroundColor = ConsoleColor.Gray;
            
            Print("DEBUG", message);
            
            if (UseColors)
                Console.ResetColor();
        }
        
        public static void Info(string message)
        {
            if (UseColors)
                Console.ResetColor();
            
            Print("INFO", message);
        }
        
        public static void Warning(string message)
        {
            if (UseColors)
                Console.ForegroundColor = ConsoleColor.Yellow;
            
            Print("WARNING", message);
            
            if (UseColors)
                Console.ResetColor();
        }

        public static void Error(string message)
        {
            if (UseColors)
                Console.ForegroundColor = ConsoleColor.Red;
            
            Print("ERROR", message);
            
            if (UseColors)
                Console.ResetColor();
        }
    }
}
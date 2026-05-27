using System;
using Microsoft.Owin.Hosting;
using TrackMyGradeAPI.Logging;

namespace TrackMyGradeAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Resolve |DataDirectory| in connection string to the folder containing the .exe
            // SQLite will create TrackMyGrade.db here automatically
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            const string baseUrl = "http://localhost:5000";

            try
            {
                using (WebApp.Start<Startup>(baseUrl))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("=========================================");
                    Console.WriteLine("  TrackMyGrade API started successfully");
                    Console.WriteLine($"  Listening on:  {baseUrl}");
                    Console.WriteLine($"  Database:      {AppDomain.CurrentDomain.BaseDirectory}TrackMyGrade.db");
                    Console.WriteLine("  Press Enter to stop...");
                    Console.WriteLine("=========================================");
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                // Log startup exception to ELMAH for diagnostics
                // Note: ELMAH configuration is read from App.config automatically
                try
                {
                    ErrorLoggingConfig.LogErrorWithMessage(
                        $"Startup Exception - Failed to start TrackMyGrade API on {baseUrl}",
                        ex);
                }
                catch
                {
                    // ELMAH logging failed - will print to console as fallback
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to start API. Full error:");
                Console.WriteLine(new string('-', 50));
                Exception current = ex;
                int depth = 0;
                while (current != null)
                {
                    Console.WriteLine($"{new string(' ', depth * 2)}[{current.GetType().Name}]");
                    Console.WriteLine($"{new string(' ', depth * 2)}{current.Message}");
                    current = current.InnerException;
                    depth++;
                }
                Console.WriteLine(new string('-', 50));
                Console.ResetColor();
                Console.ReadLine();
            }
        }
    }
}

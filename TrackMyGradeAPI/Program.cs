using Microsoft.Owin.Hosting;
using System;
using TrackMyGradeAPI.Data;

namespace TrackMyGradeAPI
{
    /// <summary>
    /// Entry point for the self-hosted OWIN Web API application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method that starts the OWIN host and initializes the database.
        /// </summary>
        public static void Main(string[] args)
        {
            // Initialize the database (apply migrations and seed data)
            ApplicationDbContext.Initialize();

            string baseAddress = "http://localhost:5000/";

            // Start OWIN host
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("=========================================\n  TrackMyGrade API started successfully\n  Listening on:  " + baseAddress + "\n=========================================\nPress Enter to stop...");
                Console.ReadLine();
            }
        }
    }
}
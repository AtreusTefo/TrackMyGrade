using System;
using System.Web;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.Logging;

namespace TrackMyGradeAPI
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Initialize the database
            ApplicationDbContext.Initialize();

            // Initialize ELMAH error logging
            ErrorLoggingConfig.InitializeErrorLogging();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Log unhandled exceptions with ELMAH
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                ErrorLoggingConfig.LogError(ex);
            }
        }
    }
}

using System;
using System.Web;

namespace TrackMyGradeAPI
{
    // NOTE: This application is self-hosted via Microsoft.Owin.SelfHost (Program.cs → WebApp.Start<Startup>).
    // Global.asax / HttpApplication lifecycle events (Application_Start, Application_Error, etc.)
    // are NOT invoked in self-hosted mode. All startup logic lives in Startup.cs.
    // This file is retained only so the project type remains recognized as a web project.
    /// <summary>
    /// Global HTTP application class for TrackMyGrade API.
    /// Note: This is self-hosted and lifecycle events are handled in Startup.cs.
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>
        /// Application start event handler (not invoked in self-hosted mode).
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
        }
    }
}

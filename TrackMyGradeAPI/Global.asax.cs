using System;
using System.Web;

namespace TrackMyGradeAPI
{
    // NOTE: This application is self-hosted via Microsoft.Owin.SelfHost (Program.cs → WebApp.Start<Startup>).
    // Global.asax / HttpApplication lifecycle events (Application_Start, Application_Error, etc.)
    // are NOT invoked in self-hosted mode. All startup logic lives in Startup.cs.
    // This file is retained only so the project type remains recognized as a web project.
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }
    }
}

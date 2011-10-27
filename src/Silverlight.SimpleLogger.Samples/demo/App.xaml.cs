using System;
using System.Windows;

using Silverlight.SimpleLogger;

namespace demo
{
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();

            // configure logger
            SilverlightLogger.Configure(SilverlightLogLevel.All, "SilverlightApp.log");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.RootVisual = new MainPage();
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // log the error
            SilverlightLogger.Error("Error happened", e.ExceptionObject);
        }        
    }
}

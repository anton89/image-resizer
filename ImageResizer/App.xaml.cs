using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageResizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            Startup += new StartupEventHandler(App_Startup); // Can be called from XAML 

            DispatcherUnhandledException += App_DispatcherUnhandledException; //Example 2 

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; //Example 4 
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            //Here if called from XAML, otherwise, this code can be in App() 
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException; // Example 1 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // Example 3 
        }

        // Example 1 
        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            //MessageBox.Show("1. CurrentDomain_FirstChanceException");
            //ProcessError(e.Exception);   - This could be used here to log ALL errors, even those caught by a Try/Catch block 
        }

        // Example 2 
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(@"2. App_DispatcherUnhandledException" + e.Exception.Message + @" 
" + e.Exception.InnerException
);
            e.Handled = true;
        }

        // Example 3 
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("3. CurrentDomain_UnhandledException" + e.ExceptionObject.ToString());
            var exception = e.ExceptionObject as Exception;
            if (e.IsTerminating)
            {
                //Now is a good time to write that critical error file! 
                MessageBox.Show("Goodbye world!");
            }
        }

        // Example 4 
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show("4. TaskScheduler_UnobservedTaskException" + e.Exception.Message);
            e.SetObserved();
        }
    }
}

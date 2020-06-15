using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Komodo.IndexClient;
using Komodo.IndexManager;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Console manager.
    /// </summary>
    public class ConsoleManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private bool _Enabled;
        private Settings _Settings; 
        private Func<bool> _ExitDelegate;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="settings">Server configuration.</param>
        /// <param name="exitApplication">Function to call when terminating the server.</param>
        public ConsoleManager(
            Settings settings, 
            Func<bool> exitApplication)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _Enabled = true;
            _Settings = settings;
            _ExitDelegate = exitApplication;

            Task.Run(() => ConsoleWorker());
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Terminate the console.
        /// </summary>
        public void Stop()
        {
            _Enabled = false;
            return;
        }

        #endregion

        #region Private-Methods

        private void ConsoleWorker()
        {
            string userInput = "";
            while (_Enabled)
            {
                Console.Write("Command (? for help) > ");
                userInput = Console.ReadLine();

                if (userInput == null) continue;
                switch (userInput.ToLower().Trim())
                {
                    case "?":
                        Menu();
                        break;

                    case "c":
                    case "cls":
                    case "clear":
                        Console.Clear();
                        break;

                    case "q":
                    case "quit":
                        _Enabled = false;
                        _ExitDelegate();
                        break;
                         
                    default:
                        Console.WriteLine("Unknown command.  '?' for help.");
                        break;
                }
            }
        }

        private void Menu()
        {
            Console.WriteLine("---");
            Console.WriteLine("  ?                         help / this menu");
            Console.WriteLine("  cls / c                   clear the console");
            Console.WriteLine("  quit / q                  exit the application"); 
            Console.WriteLine("");
            return;
        }
         
        #endregion 
    }
}

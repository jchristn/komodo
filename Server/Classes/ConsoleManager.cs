using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogLogging;
using RestWrapper;

namespace KomodoServer
{
    public class ConsoleManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private bool Enabled { get; set; }
        private Config Config { get; set; }
        private Func<bool> ExitApplicationDelegate;

        #endregion

        #region Constructors-and-Factories

        public ConsoleManager(
            Config config,
            Func<bool> exitApplication)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            Enabled = true;
            Config = config;
            ExitApplicationDelegate = exitApplication;

            Task.Run(() => ConsoleWorker());
        }

        #endregion

        #region Public-Methods

        public void Stop()
        {
            Enabled = false;
            return;
        }

        #endregion

        #region Private-Methods

        private void ConsoleWorker()
        {
            string userInput = "";
            while (Enabled)
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
                        Enabled = false;
                        ExitApplicationDelegate();
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

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}

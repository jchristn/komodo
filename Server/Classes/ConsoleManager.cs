using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogLogging;
using RestWrapper;
using KomodoCore;

namespace KomodoServer
{
    public class ConsoleManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private bool _Enabled;
        private Config _Config;
        private IndexManager _Indices;
        private Func<bool> _ExitDelegate;

        #endregion

        #region Constructors-and-Factories

        public ConsoleManager(
            Config config,
            IndexManager indices,
            Func<bool> exitApplication)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (indices == null) throw new ArgumentNullException(nameof(indices));

            _Enabled = true;
            _Config = config;
            _Indices = indices;
            _ExitDelegate = exitApplication;

            Task.Run(() => ConsoleWorker());
        }

        #endregion

        #region Public-Methods

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

                    case "list":
                        ListIndices();
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
            Console.WriteLine("  list                      list indices");
            Console.WriteLine("");
            return;
        }
        
        private void ListIndices()
        {
            List<Index> indices = _Indices.GetIndices();
            if (indices != null && indices.Count > 0)
            {
                Console.WriteLine("Indices: " + indices.Count);
                foreach (Index currIndex in indices)
                {
                    Console.WriteLine("  " + currIndex.IndexName + " [" + currIndex.RootDirectory + "]");
                }
            }
            else
            {
                Console.WriteLine("No indices");
            }
        }

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}

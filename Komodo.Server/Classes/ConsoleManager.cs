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
        private KomodoIndices _Indices;
        private Func<bool> _ExitDelegate;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="config">Server configuration.</param>
        /// <param name="indices">List of indices.</param>
        /// <param name="exitApplication">Function to call when terminating the server.</param>
        public ConsoleManager(
            Settings config,
            KomodoIndices indices,
            Func<bool> exitApplication)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (indices == null) throw new ArgumentNullException(nameof(indices));

            _Enabled = true;
            _Settings = config;
            _Indices = indices;
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
            List<KomodoIndex> indices = _Indices.Get();
            if (indices != null && indices.Count > 0)
            {
                Console.WriteLine("Indices: " + indices.Count);
                foreach (KomodoIndex index in indices)
                {
                    Console.WriteLine("  " + index.Name + " [" + index.GUID+ "]");
                }
            }
            else
            {
                Console.WriteLine("No indices");
            }
        }

        #endregion 
    }
}

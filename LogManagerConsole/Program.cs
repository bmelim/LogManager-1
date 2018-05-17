using LogManager;
using System;
using System.Text;

namespace LogManagerConsole {
    class Program {
        static void Main(string[] args) {
            StringBuilder sbHelp = new StringBuilder();
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append("HELP:");
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append("A: Add new directory");
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append("R: Remove a directory");
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append("L: List all the directories");
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append("C: Compress logs from directories");
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append("D: Delete old logs from directories");
            sbHelp.Append(System.Environment.NewLine);
            sbHelp.Append(System.Environment.NewLine);

            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            string line;

            Console.WriteLine(sbHelp.ToString());

            do {
                cki = Console.ReadKey();
                switch (cki.Key) {
                    case ConsoleKey.A:
                        line = Console.ReadLine();
                        AppController.GetInstance().AddNewDirectory(line);
                        break;

                    case ConsoleKey.R:
                        line = Console.ReadLine();
                        AppController.GetInstance().RemoveDirectory(line);
                        break;

                    case ConsoleKey.L:
                        AppController.GetInstance().GetDirectories().ForEach((e) => {
                            Console.WriteLine(e["path"]);
                        });
                        break;

                    case ConsoleKey.C:
                        AppController.GetInstance().ArchiveLogDirectories();
                        break;

                    case ConsoleKey.D:
                        AppController.GetInstance().RemoveOldLog();
                        break;

                    case ConsoleKey.H:
                        Console.WriteLine(sbHelp.ToString());
                        break;
                }
            } while (cki.Key != ConsoleKey.Q);
        }
    }
}

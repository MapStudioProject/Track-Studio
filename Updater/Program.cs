using System;
using System.Threading;
using System.IO;
using System.Linq;

namespace Updater
{
    internal class Program
    {
        static string execDirectory = "";
        static string folderDir = "";

        static void Main(string[] args)
        {
            execDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            folderDir = execDirectory;
            UpdaterHelper.Setup("MapStudioProject", "Track-Studio", "MapStudio.exe");

            bool force = args.Contains("-f");
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-d":
                    case "--download":
                        UpdaterHelper.DownloadLatest(execDirectory, 1, force);
                        break;
                    case "-i":
                    case "--install":
                        UpdaterHelper.Install(execDirectory);
                        break;
                    case "-b":
                    case "--boot":
                        Boot();
                        Environment.Exit(0);
                        break;
                    case "-e":
                    case "--exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static void Boot()
        {
            Console.WriteLine("Booting...");

            Thread.Sleep(3000);
            System.Diagnostics.Process.Start(Path.Combine(folderDir, "MapStudio.exe"));
        }
    }
}

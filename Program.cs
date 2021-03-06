﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace RAPIS_FIMC
{
    static class Program
    {
        #region Constants
        public static System.Globalization.CultureInfo ICul = System.Globalization.CultureInfo.CurrentCulture; // Invariant current system culture
        public static string WebsiteLink = "https://sunburst275.jimdofree.com/";
        public static string ProgramHeader = "RAPIS FIMC";
        public static string ProgramHeaderWithDash = "RAPIS FIMC - ";
        public static int CmdLineArgumentOffset = 1;
        #endregion
        #region Variables
        public static Version Version;
        #endregion

        /// <summary>The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CreateVersion();

            // Get commandline args
            string[] cmdLineArgs = Environment.GetCommandLineArgs();

            // DBG: Test cmdlineargs:
            // C:\Users\Timo\Desktop\Testus\Text\dolandark.txt 2 5 C:\Users\Timo\Desktop\Testus\Text\dolandark_cmd.csv

            // Check command line arguments for validity and show messages if necessary
            if (cmdLineArgs.Length != 0 + CmdLineArgumentOffset)
            {
                // Help
                if (cmdLineArgs[1].ToLower() == "help")
                {
                    // Build message
                    StringBuilder sb = new StringBuilder();
                    {
                        sb.AppendLine("This program removes a column of characters in each line of a file.");
                        sb.AppendLine("To use this program, please enter the parameter as follows:");
                        sb.AppendLine("<Source> <From> <To> <Destination>");
                        sb.AppendLine("");
                        sb.AppendLine("The source file has to exist and the file types of source and destination have to be supported.");
                        sb.AppendLine("Also, the program needs access to the paths of the files.");
                        sb.AppendLine("\nCurrently supported input/output files are:");
                        foreach (KeyValuePair<FileInfo.FileType, string> entry in FileInfo.supportedFileTypes)
                        {
                            sb.Append(string.Format("*.{0}\t", entry.Value));
                        }
                    }
                    DialogResult answ = MessageBox.Show(sb.ToString(), ProgramHeaderWithDash + "Commandline help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (answ == DialogResult.OK)
                        Environment.Exit(0);
                }
                // Wrong argument count
                else if (cmdLineArgs.Length != 4 + CmdLineArgumentOffset)
                {
                    DialogResult answ = MessageBox.Show("Number of arguments should be 4.\n\nPlease enter the parameter as follows:\n<Source> <From> <To> <Destination>\nFor help, type \"help\".", ProgramHeaderWithDash + "Invalid argument count!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (answ == DialogResult.OK)
                        Environment.Exit(0);
                }
                // Invalid arguments
                else if (!ValidateCmdLineArguments(cmdLineArgs))
                {
                    DialogResult answ = MessageBox.Show("Arguments are faulty.\nPlease enter the parameter as follows:\n<Source> <From> <To> <Destination>\n\nAlso make sure that the source and destination file formats are supported.\nFor help, type \"help\".", ProgramHeaderWithDash + "Invalid argument(s)!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (answ == DialogResult.OK)
                        Environment.Exit(0);
                }
            }

            Application.Run(new MainForm(cmdLineArgs));
        }

        /// <summary>Validates command line arguments according to the rules of this program.</summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>True when they are valid, otherwise false.</returns>
        public static bool ValidateCmdLineArguments(string[] args)
        {
            int index = CmdLineArgumentOffset;
            var source = args[index++];
            var from = args[index++];
            var to = args[index++];
            var destination = args[index++];

            Console.WriteLine(string.Format("Command line arguments:\n{0}\t{1}\t{2}\t{3}", source, from, to, destination));

            // Check whether values are convertible
            if (!int.TryParse(from, out _))
            {
                return false;
            }
            if (!int.TryParse(to, out _))
            {
                return false;
            }

            // Check whether the files are supported files
            string sourceFileExtension = FileInfo.ExtractFileExtension(FileInfo.ExtractFileName(source));
            string destinationFileExtension = FileInfo.ExtractFileExtension(FileInfo.ExtractFileName(destination));

            if (!FileInfo.IsValidFileType(sourceFileExtension))
                return false;
            if (!FileInfo.IsValidFileType(destinationFileExtension))
                return false;

            return true;
        }
        /// <summary>Creates all program version info</summary>
        private static void CreateVersion()
        {
            DateTime today = DateTime.Now;
            int Major = 1;
            int Minor = 0;
            int Build = int.Parse(today.ToString("yy"));
            int Revision = (int)1e4 + int.Parse(today.ToString("MMdd"));
            string VersionString = string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
            Version = Version.Parse(VersionString);
        }
    }
}

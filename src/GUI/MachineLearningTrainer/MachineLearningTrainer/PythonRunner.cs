using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer
{
    /// <summary>
    /// This class provides the functions, to call python scripts from c# code and revieve it's result.
    /// </summary>
    public static class PythonRunner
    {
        /// <summary>
        /// Finds the installation Path of 3.6 python.
        /// </summary>
        /// <returns></returns>
        private static string FindPythonInstallationPath()
        {
            try
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key =hklm.OpenSubKey("SOFTWARE\\Python\\PythonCore\\3.6\\InstallPath"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("ExecutablePath");
                        if (o != null)
                        {
                            return o.ToString();
                        }
                    }
                }
                throw new Exception("Couldn't find path");
            }
            catch (Exception ex)  
            {
                return "";
            }
        }

        private static StringBuilder stdOut = new StringBuilder();
        /// <summary>
        /// Runs a python script.
        /// </summary>
        /// <param name="path">Path to the .py file.</param>
        /// <param name="showOutputWindow">When true, console window is shown.</param>
        /// <param name="args">Cmd args for the script.</param>
        /// <returns></returns>
        public static string RunScriptAsynchronous(string path, bool showOutputWindow, string [] args, bool redirectStdOut = true)
        {
            try
            {
                stdOut = new StringBuilder();
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "C:/Users/Philipp/Anaconda3/python.exe";
                start.Arguments = string.Format("{0} {1}", path, string.Join(" ", args));
                start.CreateNoWindow = showOutputWindow ? false : true;
                start.UseShellExecute = false;
                start.RedirectStandardOutput = redirectStdOut;
                Process p = new Process();
                p.StartInfo = start;
                p.OutputDataReceived += OutputHandler;
                p.Start();
                if(redirectStdOut)
                    p.BeginOutputReadLine();
                p.WaitForExit();
                return stdOut.ToString();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Runs a python script.
        /// </summary>
        /// <param name="path">Path to the .py file.</param>
        /// <param name="showOutputWindow">When true, console window is shown.</param>
        /// <param name="args">Cmd args for the script.</param>
        /// <returns></returns>
        public static string RunScriptSynchronous(string path, bool showOutputWindow, string[] args, bool redirectStdOut = true)
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "C:/Users/Philipp/Anaconda3/python.exe";
                start.Arguments = string.Format("{0} {1}", path, string.Join(" ", args));
                start.CreateNoWindow = showOutputWindow ? false : true;
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {

                    using (StreamReader reader = process.StandardOutput)
                    {
                        var std_out = reader.ReadToEnd();
                        return std_out == "" ? "There went something wrong" : std_out;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);
            stdOut.Append(outLine.Data);
            //stdOut.Append('\n');
        }



    }
}

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
    /// This class provides the functions, to call python scripts from c# code and reveive it's result.
    /// </summary>
    public static class PythonRunner
    {
        /// <summary>
        /// Finds the installation Path of 3.6 python.
        /// </summary>
        /// <returns></returns>
        public static string FindPythonInstallationPath()
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
            catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            {
                return "";
            }
        }



        public static string RunScript(string path, bool showOutputWindow)
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = FindPythonInstallationPath();
                start.Arguments = string.Format("{0} {1}", path, "test");
                start.CreateNoWindow = showOutputWindow ? false : true;
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}

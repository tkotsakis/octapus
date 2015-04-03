using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Relational.Octapus.Common
{
    public class Commander
    {

        public static Boolean Execute(string directory, string cmdFile, string args, ref string errorMessage)
        {
            Boolean retValue = true;
            errorMessage = null;
            Process cmdExec = new Process();
            try
            {
                cmdExec.StartInfo.FileName = cmdFile;
                cmdExec.StartInfo.WorkingDirectory = directory;
                cmdExec.StartInfo.Arguments = args;
                cmdExec.StartInfo.UseShellExecute = false;
                cmdExec.StartInfo.CreateNoWindow = true;
                cmdExec.StartInfo.RedirectStandardOutput = true;
                cmdExec.StartInfo.RedirectStandardError = true;
                cmdExec.Start();
                cmdExec.WaitForExit();
                cmdExec.Close();
            }
            catch (Exception ex)
            {
                cmdExec.Close();
                errorMessage = ex.Message;
                retValue = false;
            }
            return retValue;
        }

        public static Boolean Exec(string directory, string cmdFile, string args, ref string errorMessage)
        {
            string output = string.Empty;
            string error = string.Empty;
            Boolean retValue = true;

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(cmdFile);
                processStartInfo.FileName = cmdFile;
                processStartInfo.WorkingDirectory = directory;
                processStartInfo.Arguments = args;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                processStartInfo.UseShellExecute = false;

                Process process = Process.Start(processStartInfo);
                using (StreamReader streamReader = process.StandardOutput)
                {
                    output = streamReader.ReadToEnd();
                }

                using (StreamReader streamReader = process.StandardError)
                {
                    error = streamReader.ReadToEnd();
                }

                retValue = Regex.Split(output,("\r\n")).ToList().Contains("0") ? true : false;
                process.Close();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                retValue = false;
            }


            return retValue;

        }

        static void  CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data, ConsoleColor.White);
        }

        static void CaptureError(object sender, DataReceivedEventArgs e)
        {
            ShowOutput(e.Data, ConsoleColor.Red);
        }

        static string ShowOutput(string data, ConsoleColor color)
        {
            if (data != null)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine("Received: {0}", data);
                Console.ForegroundColor = oldColor;
            }
            return data;
        }

        public static bool DiskHasFreeSpace(string driveName, string spaceNeeded)
        {
            DriveInfo driveInfo = new DriveInfo(driveName);
            var availableDiskSpace = (driveInfo.AvailableFreeSpace / 1024) / 1024 ;
            return availableDiskSpace <= Convert.ToDouble(spaceNeeded) ? false : true;
        }

    }
}

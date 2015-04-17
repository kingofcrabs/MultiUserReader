using ReadResult.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ReadResult
{
    class SharedFolder
    {
        string userName;
        string password;
        public SharedFolder()
        {
            userName = ConfigurationManager.AppSettings["userName"];
            password = ConfigurationManager.AppSettings["password"];
        }
        public void CopyFolderFromRemote(string remoteFolder)
        {
            string tempFolder = GlobalVars.Instance.TempFolder;
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string command = "NET USE " + remoteFolder + " /user:" + userName + " " + password;
            ExecuteCommand(command, 5000);

            command = " copy \"" + remoteFolder + "\"  \"" + tempFolder+ "\"";
            ExecuteCommand(command, 60000);
        }

        public void SaveACopyfileToServer(string srcFilePath)
        {
            string savePath = GlobalVars.Instance.RemoteFolder; 
            var directory = Path.GetDirectoryName(savePath).Trim();
            var filenameToSave = Path.GetFileName(srcFilePath);

            if (!directory.EndsWith("\\"))
                filenameToSave = "\\" + filenameToSave;

            string  command = "NET USE " + directory + " /user:" + userName + " " + password;
            ExecuteCommand(command, 5000);

            command = " copy \"" + srcFilePath + "\"  \"" + directory + filenameToSave + "\"";
            ExecuteCommand(command, 5000);

        }

        public int ExecuteCommand(string command, int timeout)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = "C:\\",
            };

            var process = Process.Start(processInfo);
            process.WaitForExit(timeout);
            var exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        } 
    }
}

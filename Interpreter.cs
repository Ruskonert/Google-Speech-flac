using System;
using System.Diagnostics;
using System.IO;

namespace Workspace
{
    class Interpreter
    {
        private string fileName;
        private string argument;

        private ProcessStartInfo startInfo = new ProcessStartInfo();
        private Process runtimeProcess = null;
        private Int32 exitCode = -1;

        public int ExitCode { get => exitCode; private set => exitCode = value; }

        public Int32 GetExitCode()
        {
            return this.ExitCode;
        }

        public Interpreter(String fileName, String args = null)
        {
            this.fileName = fileName;
            this.argument = args;
        }
        
        public void Start(string args = null)
        {
            this.startInfo.FileName = fileName;

            string currentArgument = args;

            if (args == null) args = this.argument;

            this.startInfo.Arguments = currentArgument;
            this.startInfo.UseShellExecute = false;
            this.startInfo.RedirectStandardOutput = true;
            using (Process process = Process.Start(this.startInfo))
            {
                this.runtimeProcess = process;
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
            
        }

        public void Stop()
        {
            if (this.runtimeProcess != null)
                this.runtimeProcess.Close();

        }
    }
}

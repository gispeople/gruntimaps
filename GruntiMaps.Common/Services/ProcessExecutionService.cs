using System.Diagnostics;
using System.Text;

namespace GruntiMaps.Common.Services
{
    public static class ProcessExecutionService
    {
        private const int TimeOutInMilliseconds = 180000;

        public static (bool success, string error) ExecuteProcess(string fileName, string arguments)
        {
            using (var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            })
            {
                process.Start();
                var errorMessage = new StringBuilder();
                while (!process.StandardError.EndOfStream)
                {
                    errorMessage.AppendLine(process.StandardError.ReadLine());
                }
                process.WaitForExit(TimeOutInMilliseconds);

                return process.ExitCode == 0
                    ? (true, null)
                    : (false, errorMessage.ToString());
            }
        }
    }
}

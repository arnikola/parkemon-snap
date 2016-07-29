using System;
using System.Diagnostics;

namespace ServiceCommon.Utilities.Extensions
{
    public static class ProcessExtensions
    {
        public struct ProcessResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
        }

        public static ProcessResult Execute(this ProcessStartInfo cmd, bool throwOnError = true)
        {
            var result = new ProcessResult();
            try
            {
                var process = new Process { StartInfo = cmd };
                cmd.UseShellExecute = false;
                cmd.RedirectStandardError = true;
                cmd.RedirectStandardOutput = true;
                cmd.CreateNoWindow = true;
                process.Start();
                result.Output = process.StandardOutput.ReadToEnd();
                result.Error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                result.ExitCode = process.ExitCode;
                var message = string.Format(
                    "{0} {1} exited with error code {2}:\n[{0} Output]\n{3}\n[{0} Error]\n{4}",
                    cmd.FileName,
                    cmd.Arguments,
                    result.ExitCode,
                    result.Output,
                    result.Error);
                Trace.TraceInformation(message);
                if (process.ExitCode != 0 && throwOnError)
                {
                    throw new ApplicationException($"Exception executing command: {message}");
                }
            }
            catch (Exception exception)
            {
                if (throwOnError)
                {
                    throw;
                }

                result.Error +=
                    $"Exception executing process {cmd.FileName} {cmd.Arguments}: {exception.ToDetailedString()}\n";
                if (result.ExitCode == 0)
                {
                    result.ExitCode = -Math.Abs(exception.GetHashCode());
                }
            }

            return result;
        }
    }
}

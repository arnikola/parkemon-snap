using System;
using System.Diagnostics;
using System.Threading;

namespace ServiceCommon.Utilities
{
    /// <summary>
    /// The debug util.
    /// </summary>
    public static class DebugUtil
    {
        /// <summary>
        /// Waits for the debugger to attach.
        /// </summary>
        [Obsolete("Production code should avoid this method.")]
        [Conditional("DEBUG")]
        public static void WaitForDebuggerAttack()
        {
            var currentProcess = Process.GetCurrentProcess();
            while (!Debugger.IsAttached)
            {
                Debug.WriteLine("{0} ({1}): Waiting for debugger to attach...", currentProcess.ProcessName, currentProcess.Id);
                Trace.TraceWarning("Waiting for debugger to attach...");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Debug.WriteLine("{0} ({1}): Debugger attached.", currentProcess.ProcessName, currentProcess.Id);
        }
    }
}

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Process helper with asynchronous interface
    /// - Based on https://gist.github.com/georg-jung/3a8703946075d56423e418ea76212745
    /// - And on https://stackoverflow.com/questions/470256/process-waitforexit-asynchronously
    /// </summary>
    public static class ProcessAsyncHelper
    {
    /// <summary>
    /// Run a process asynchronously
    /// <para>To capture STDOUT, set StartInfo.RedirectStandardOutput to TRUE</para>
    /// <para>To capture STDERR, set StartInfo.RedirectStandardError to TRUE</para>
    /// </summary>
    /// <param name="fileName">Process filename to be run</param>
    /// <param name="arguments">Process aguments</param>
    /// <param name="stdOut">Starndar output writet</param>
    /// <param name="stdErr">Standard Error output writer</param>
    /// <param name="stdIn">If provided, will be written to stdIn</param>
    /// <param name="timeoutMs">The timeout in milliseconds (null for no timeout)</param>
    /// <param name="workingDir">The working directory for process run</param>
    /// <returns>Result object</returns>
    public static async Task<int> RunAsync(
            string fileName,
            string arguments = null,
            TextWriter stdOut = null,
            TextWriter stdErr = null,
            string stdIn = null,
            int? timeoutMs = null,
            string workingDir = null)
        {
            int exitCode = -1;

            var startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                Arguments = arguments,
                FileName = fileName,
                RedirectStandardOutput = stdOut != null,
                RedirectStandardError = stdErr != null,
                UseShellExecute = false,
                WorkingDirectory = workingDir
            };

            if (!string.IsNullOrWhiteSpace(stdIn))
            {
                startInfo.RedirectStandardInput = true;
            }

        using (var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                // List of tasks to wait for a whole process exit
                List<Task> processTasks = new List<Task>();

                // === EXITED Event handling ===
                var processExitEvent = new TaskCompletionSource<object>();
                process.Exited += (sender, args) =>
                {
                    processExitEvent.TrySetResult(true);
                };
                processTasks.Add(processExitEvent.Task);

                // === STDOUT handling === 
                if (process.StartInfo.RedirectStandardOutput)
                {
                    var stdOutCloseEvent = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            stdOutCloseEvent.TrySetResult(true);
                        }
                        else
                        {
                            stdOut.WriteLine(e.Data);
                        }
                    };

                    processTasks.Add(stdOutCloseEvent.Task);
                }
                else
                {
                    // STDOUT is not redirected, so we won't look for it
                }

                // === STDERR handling === 
                if (process.StartInfo.RedirectStandardError)
                {
                    var stdErrCloseEvent = new TaskCompletionSource<bool>();

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            stdErrCloseEvent.TrySetResult(true);
                        }
                        else
                        {
                            stdErr.WriteLine(e.Data);
                        }
                    };

                    processTasks.Add(stdErrCloseEvent.Task);
                }
                else
                {
                    // STDERR is not redirected, so we won't look for it
                }

                // === START OF PROCESS ===
                if (!process.Start())
                {
                    return process.ExitCode;
                }

                if (process.StartInfo.RedirectStandardInput)
                {
                    using (var writer = process.StandardInput)
                    {
                        writer.Write(stdIn);
                    }
                }

            // Reads the output stream first as needed and then waits because deadlocks are possible
            if (process.StartInfo.RedirectStandardOutput)
                {
                    process.BeginOutputReadLine();
                }
                else
                {
                    // No STDOUT
                }

                if (process.StartInfo.RedirectStandardError)
                {
                    process.BeginErrorReadLine();
                }
                else
                {
                    // No STDERR
                }

                // === ASYNC WAIT OF PROCESS ===

                // Process completion = exit AND stdout (if defined) AND stderr (if defined)
                Task processCompletionTask = Task.WhenAll(processTasks);

                // Task to wait for exit OR timeout (if defined)
                Task<Task> awaitingTask = timeoutMs.HasValue
                    ? Task.WhenAny(Task.Delay(timeoutMs.Value), processCompletionTask)
                    : Task.WhenAny(processCompletionTask);

                // Let's now wait for something to end...
                if ((await awaitingTask.ConfigureAwait(false)) == processCompletionTask)
                {
                    // -> Process exited cleanly
                    exitCode = process.ExitCode;
                }
                else
                {
                    // -> Timeout, let's kill the process
                    try
                    {
                        stdOut.WriteLine("Kill process [{0}] {1}", process.Id, process.ProcessName);
                        process.Kill();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return exitCode;
        }
    } 
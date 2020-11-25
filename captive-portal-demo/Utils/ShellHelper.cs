using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using Microsoft.VisualBasic.CompilerServices;

namespace captive_portal_demo.Utils
{
    public static class ShellHelper
    {
        public static int Shell(this string cmd, bool debug = false)
        {
            if (cmd.Contains('|'))
            {
                //PIPE DETECTED, NEED TO SPLIT
                if (debug)
                {
                    Console.WriteLine("Pipe detected in: " + cmd);
                    Console.WriteLine("Will split and pipe");
                }

                var commandLeft = cmd.Substring(0, cmd.IndexOf('|') - 1);
                if (debug)
                    Console.WriteLine("CommandLeft is: " + commandLeft);
                var commandRight = cmd.Substring(cmd.IndexOf('|') + 2);
                if (debug)
                    Console.WriteLine("CommandRight is: " + commandRight);
                StringWriter outputTextWriter = new StringWriter();
                ExecuteSync(commandLeft, outputTextWriter);
                var leftResult = outputTextWriter.ToString();
                if(debug) 
                    Console.WriteLine("Left results is: " + leftResult);
                return ExecuteSync(commandRight, Console.Out, leftResult);
            }
            else
            {
                return ExecuteSync(cmd, Console.Out);
            }
        }

        private static int ExecuteSync(string cmd, TextWriter output, string input = null)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            return ProcessAsyncHelper.RunAsync(fileName: "/bin/bash", arguments: $"-c \"{escapedArgs}\"", output, Console.Out, input).GetAwaiter()
                .GetResult();
        }

    }
}

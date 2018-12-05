using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PagarMe.Generic
{
    public static class Terminal
    {
        static Terminal()
        {
            var assemblyInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
            AssemblyPath = assemblyInfo.Directory.FullName;
        }

        public static readonly String AssemblyPath;

        public static Result Run(String command, params String[] args)
        {
            return run(command, args, false);
        }

        public static Result RunAsAdm(String command, params String[] args)
        {
            return run(command, args, true);
        }

        private static Result run(String command, String[] args, Boolean requestAdm)
        {
            Log.Me.Info($"Running {command} with args:");

            for (var a = 0; a < args.Length; a++)
            {
                Log.Me.Info($"[{a}] {args[a]}");
            }

            var fullPath = Path.Combine(AssemblyPath, command);

            var fixedArgs = args.Select(fixArg).ToArray();

            if (File.Exists(fullPath))
            {
                var allCommands = File.ReadAllText(fullPath);

                for (var a = 0; a < fixedArgs.Length; a++)
                {
                    var param = $"%{a + 1}";
                    var value = fixedArgs[a];
                    allCommands = allCommands.Replace(param, value);
                }

                var newLineSeparator = new[] {Environment.NewLine};

                Log.Me.Info("Commands that will be executed:");

                allCommands
                    .Split(newLineSeparator, StringSplitOptions.None)
                    .ToList()
                    .ForEach(Log.Me.Info);
            }

            var joinedArgs = String.Join(" ", fixedArgs);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo(command, joinedArgs)
                {
                    WorkingDirectory = AssemblyPath,
                    RedirectStandardOutput = !requestAdm,
                    RedirectStandardError = !requestAdm,
                    UseShellExecute = requestAdm,
                },
                EnableRaisingEvents = true,
            };

            if (requestAdm)
            {
                proc.StartInfo.Verb = "runas";
            }

            proc.Start();
            proc.WaitForExit();

            return new Result(proc);
        }

        private static String fixArg(String arg)
        {
            return arg.Contains(" ") ? $"\"{arg}\"" : arg;
        }

        public class Result
        {
            internal Result(Process proc)
            {
                Code = proc.ExitCode;

                if (proc.StartInfo.RedirectStandardOutput)
                {
                    Output = proc.StandardOutput.ReadClean();
                }

                if (proc.StartInfo.RedirectStandardError)
                {
                    Error = proc.StandardError.ReadToEnd();
                }
            }

            public Boolean Succedded => Code == 0;

            public Int32 Code { get; private set; }
            public String Output { get; private set; }
            public String Error { get; private set; }
        }

        public static String ReadClean(this StreamReader reader)
        {
            var result = reader.ReadToEnd();
            result = Regex.Replace(result, @"\0", "");
            return result?.Trim();
        }
    }

}
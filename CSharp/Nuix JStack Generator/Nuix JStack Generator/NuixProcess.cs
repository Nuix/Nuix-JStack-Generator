using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nuix_JStack_Generator
{
    public class NuixProcess
    {
        private static Regex nuixProcessMatcher = new Regex(@"(nuix_((app)|(console)|(single_worker)|(worker_agent))|(javaw?)|(nuix-ims)|(nuix-[^\.]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex workerPathTrim = new Regex(@"\\bin$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string appRootDirectory = "";
        private static string localJstackExe = "";

        private Process actualProcess;
        private bool methodDetermined = false;
        private JStackMethod generationMethod = JStackMethod.ToolsJarMethod;

        public enum JStackMethod
        {
            ToolsJarMethod,
            AppJStackMethod,
            UserJStackMethod,
        }

        public bool IsToBeMonitored { get; set; }

        public bool HasExited { get { return actualProcess.HasExited; } }
        public int ProcessId { get { return actualProcess.Id; } }
        public bool IsConsoleVersion { get { return actualProcess.ProcessName.Contains("_console"); } }
        public bool IsAppVersion { get { return !IsConsoleVersion; } }
        public bool IsRest { get { return Path.GetFileNameWithoutExtension(FileName).Contains("REST"); } }
        public string FileName { get { return actualProcess.Modules[0].FileName; } }
        public string InstallRoot
        {
            get
            {
                //Worker install root needs to be calculated slightly differently since worker exe
                //in a sub folder
                string imageName = Path.GetFileNameWithoutExtension(FileName).ToLower();
                if (FileName.EndsWith("nuix_single_worker.exe"))
                {
                    string result = workerPathTrim.Replace(Path.GetDirectoryName(FileName), "");
                    return result;
                }
                else if (imageName == "java" || imageName == "javaw")
                {
                    var segments = Path.GetDirectoryName(FileName).Split('\\').Select(segment => segment.ToLower())
                        .Where(segment => segment != "jre" && segment != "bin" && segment != "..");
                    return String.Join("\\", segments);
                }
                else
                {
                    string result = Path.GetDirectoryName(FileName);
                    return result;
                }
            }
        }
        public string FriendlyName
        {
            get
            {
                string imageName = Path.GetFileNameWithoutExtension(FileName).ToLower();
                switch (imageName)
                {
                    case "nuix_app":
                        return "Nuix App";
                    case "nuix_console":
                        return "Nuix Console";
                    case "nuix_single_worker":
                        return "Nuix Worker";
                    case "nuix_worker_agent":
                        return "Nuix Agent";
                    case "nuix-ims":
                        return "Nuix IMS";
                    case "java":
                    case "javaw":
                        string title = actualProcess.MainWindowTitle;
                        if (!string.IsNullOrWhiteSpace(title)) { return title; }
                        else { return "Java App"; }
                    default:
                        return "Nuix Process: " + imageName;
                }
            }
        }
        public string Details
        {
            get
            {
                return FriendlyName + " : " + actualProcess.Id.ToString();
            }
        }

        private NuixProcess()
        {
            IsToBeMonitored = true;
        }

        static NuixProcess()
        {
            appRootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            localJstackExe = Path.Combine(appRootDirectory, "jre", "bin", "jstack.exe");
        }

        public void CreateJStack(string outputDirectory)
        {
            if (!methodDetermined)
            {
                if (File.Exists(localJstackExe))
                    generationMethod = JStackMethod.UserJStackMethod;
                else if (File.Exists(InstallRoot + "\\lib\\tools.jar"))
                    generationMethod = JStackMethod.ToolsJarMethod;
                else
                    generationMethod = JStackMethod.AppJStackMethod;

                methodDetermined = true;
            }

            var filenamePieces = new string[] {
                actualProcess.ProcessName.ToUpper(),
                "PID"+actualProcess.Id.ToString(),
                DateTime.Now.ToString("yyyyMMdd"),
                DateTime.Now.ToString("HHmmss"),
                DateTime.Now.ToString("ffff"),
                "JSTACK"
            };
            string outputFile = Path.Combine(outputDirectory, string.Join("_", filenamePieces) + ".txt");
            switch (generationMethod)
            {
                case JStackMethod.ToolsJarMethod:
                    createJStackOld(outputFile);
                    break;
                case JStackMethod.AppJStackMethod:
                    createJStackNew(outputFile);
                    break;
                case JStackMethod.UserJStackMethod:
                    createJStackLocal(outputFile);
                    break;
                default:
                    break;
            }
        }

        private void createJStackOld(string outputFile)
        {
            string commandArguments = string.Format("-classpath \"{0}\\lib\\tools.jar\" sun.tools.jstack.JStack {1}", InstallRoot, actualProcess.Id);
            string nuixJavaExePath = "\"" + Path.Combine(InstallRoot, "jre", "bin", "java.exe") + "\"";

            Process jstackProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = nuixJavaExePath,
                Arguments = commandArguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                while (!jstackProcess.StandardOutput.EndOfStream)
                {
                    string line = jstackProcess.StandardOutput.ReadLine();
                    sw.WriteLine(line);
                }

                while (!jstackProcess.StandardError.EndOfStream)
                {
                    string line = jstackProcess.StandardError.ReadLine();
                    sw.WriteLine(line);
                }
            }
        }

        private void createJStackNew(string outputFile)
        {
            string exePath = string.Format("\"{0}\\jre\\bin\\jstack.exe\"", InstallRoot);
            Process jstackProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = actualProcess.Id.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                while (!jstackProcess.StandardOutput.EndOfStream)
                {
                    string line = jstackProcess.StandardOutput.ReadLine();
                    sw.WriteLine(line);
                }

                while (!jstackProcess.StandardError.EndOfStream)
                {
                    string line = jstackProcess.StandardError.ReadLine();
                    sw.WriteLine(line);
                }
            }
        }

        private void createJStackLocal(string outputFile)
        {
            Process jstackProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = localJstackExe,
                Arguments = actualProcess.Id.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                while (!jstackProcess.StandardOutput.EndOfStream)
                {
                    string line = jstackProcess.StandardOutput.ReadLine();
                    sw.WriteLine(line);
                }

                while (!jstackProcess.StandardError.EndOfStream)
                {
                    string line = jstackProcess.StandardError.ReadLine();
                    sw.WriteLine(line);
                }
            }
        }

        public static List<NuixProcess> GetRunningNuixProcesses()
        {
            List<NuixProcess> result = new List<NuixProcess>();
            Process[] runningProcesses = Process.GetProcesses();
            var nuixProcesses = runningProcesses.Where(p => nuixProcessMatcher.IsMatch(p.ProcessName));
            foreach (var process in nuixProcesses)
            {
                NuixProcess np = new NuixProcess() { actualProcess = process };
                if (np.IsRest) { continue; }
                try
                {
                    // Test if we can get HasExited for this process, if not, ignore it
                    bool test = np.HasExited;
                    result.Add(np);
                }
                catch (Exception) { }
            }
            return result;
        }
    }
}

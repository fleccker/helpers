using System.Diagnostics;

namespace helpers.Unix
{
    [LogSource("Linux Shell")]
    public class LinuxCommand 
    {
        private string _command = "";
        private string _output = "";

        public string Command { get => _command; }
        public string LastOutput { get => _output; }

        public LinuxCommand WithArg(string command)
        {
            _command += $" {command}";
            _command = _command.Replace("\"", "\"\"");

            return this;
        }

        public LinuxCommand WithExecute(string fileName)
        {
            return WithArg($"bash {fileName}");
        }

        public string Execute()
        {
            var process = CreateCommandProcess();

            ExecuteProcess(process);

            _output = ReadOutput(process);
            _command = "";

            DisposeProcess(process);

            return _output;
        }

        public string FormatDirectoryName(string directoryName)
        {
            return directoryName.Replace(" ", "\\ ");
        }

        private void ExecuteProcess(Process process)
        {
            process.Start();
            process.WaitForExit();
        }

        private string ReadOutput(Process process)
        {
            return process.StandardOutput.ReadToEnd();
        }

        private Process CreateCommandProcess()
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + _command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
        }

        private void DisposeProcess(Process process)
        {
            process.Dispose();
        }

        public static bool IsAptPackageInstalled(string packageName)
        {
            var output = SearchInstalledAptPackages(packageName)
                .Execute();

            return output.Contains(packageName);
        }

        public static LinuxCommand ListAptPackages()
        {
            return new LinuxCommand()
                .WithArg("sudo apt list");
        }

        public static LinuxCommand ListInstalledAptPackages()
        {
            return new LinuxCommand()
                .WithArg("sudo apt list --installed");
        }

        public static LinuxCommand SearchAptPackages(string search)
        {
            return new LinuxCommand()
                .WithArg($"sudo apt list {search}");
        }

        public static LinuxCommand SearchInstalledAptPackages(string search)
        {
            return new LinuxCommand()
                .WithArg($"sudo apt list {search} --installed");
        }

        public static LinuxCommand InstallAptPackage(string packageName)
        {
            return new LinuxCommand()
                .WithArg($"sudo apt install -y {packageName}");
        }

        public static LinuxCommand UninstallAptPackage(string packageName)
        {
            return new LinuxCommand()
                .WithArg($"sudo apt purge -y {packageName}");
        }

        public static LinuxCommand AptAutoremove()
        {
            return new LinuxCommand()
                .WithArg($"sudo apt autoremove -y");
        }

        public static LinuxCommand AddRepository(string repoName)
        {
            return new LinuxCommand()
                .WithArg($"sudo apt-add-repository {repoName}");
        }

        public static LinuxCommand KillProcess(string pNameOrId)
        {
            return new LinuxCommand()
                .WithArg($"pkill {pNameOrId}");
        }

        public static LinuxCommand StartInScreen(string screenName, string command)
        {
            return new LinuxCommand()
                .WithArg($"screen -dmS {screenName} {command}");
        }

        public static LinuxCommand KillScreen(string screenName)
        {
            return new LinuxCommand()
                .WithArg($"screen -S {screenName} -X quit");
        }

        public static LinuxCommand KillAllScreens()
        {
            return new LinuxCommand()
                .WithArg($"pkill screen");
        }

        public static LinuxCommand RemoveFileOrDirectory(string path, bool useSudo = false)
        {
            return new LinuxCommand()
                .WithArg(useSudo ? $"sudo rm -rf {path}" : $"rm -rf {path}");
        }

        public static LinuxCommand RebootSystem()
        {
            return new LinuxCommand()
                .WithArg($"sudo reboot");
        }

        public static string ReadOutput(LinuxCommand command) => command.Execute();
    }
}
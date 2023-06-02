using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

using SystemInfoLibrary.Hardware;
using SystemInfoLibrary.Hardware.CPU;
using SystemInfoLibrary.Hardware.GPU;
using SystemInfoLibrary.Hardware.RAM;
using SystemInfoLibrary.OperatingSystem;

namespace helpers
{
    [LogSource("Current System")]
    public static class CurrentSystem
    {
        private static OperatingSystem _sys;
        private static OperatingSystemInfo _sysInfo;
        private static HardwareInfo _hwInfo;
        private static Process _curProcess;

        private static CPUInfo _cpuInfo;
        private static GPUInfo _gpuInfo;
        private static RAMInfo _ramInfo;

        static CurrentSystem()
        {
            _sys = Environment.OSVersion;
            _sysInfo = OperatingSystemInfo.GetOperatingSystemInfo();
            _curProcess = Process.GetCurrentProcess();

            UpdateData();
        }

        public static long MappedProcessMemory { get => Environment.WorkingSet; }

        public static bool Is64BitProcess { get => Environment.Is64BitProcess; }

        public static string ProcessName { get => _curProcess.ProcessName; }

        public static int ProcessId { get => _curProcess.Id; }
        public static int SessionId { get => _curProcess.SessionId; }
        public static int ProcessThreadCount { get => _curProcess.Threads.Count; }

        public static DateTime ProcessStartTime { get => _curProcess.StartTime; }
        public static TimeSpan TotalCpuTime { get => _curProcess.TotalProcessorTime; }
        public static TimeSpan UserCpuTime { get => _curProcess.UserProcessorTime; }
        public static TimeSpan PriviligedCpuTime { get => _curProcess.PrivilegedProcessorTime; }

        public static int ThreadId { get => Environment.CurrentManagedThreadId; }
        public static int ThreadContextId { get => Thread.CurrentContext.ContextID; }

        public static string ThreadName { get => Thread.CurrentThread.Name; set => Thread.CurrentThread.Name = value; }
        public static string ExecutionContext { get => Thread.CurrentThread.ExecutionContext.ToString(); }

        public static CultureInfo ThreadCulture { get => Thread.CurrentThread.CurrentCulture; set => Thread.CurrentThread.CurrentCulture = value; }

        public static string SystemDirectory { get => Environment.SystemDirectory; }
        public static string CurrentDirectory { get => Environment.CurrentDirectory; set => Environment.CurrentDirectory = value; }
        public static string NewLine { get => Environment.NewLine; }
        public static string CommandLine { get => Environment.CommandLine; }
        public static string PlaformId { get => _sys.Platform.ToString(); }

        public static string[] CommandLineArgs { get => Environment.GetCommandLineArgs(); }

        public static string MachineName { get => Environment.MachineName; }
        public static string UserName { get => Environment.UserName; }
        public static string UserDomain { get => Environment.UserDomainName; }

        public static string OsServicePack { get => _sys.ServicePack; }
        public static string OsName { get => _sysInfo.Name; }
        public static string OsType { get => _sysInfo.OperatingSystemType.ToString(); }
        public static string OsArchitecture { get => _sysInfo.Architecture; }

        public static string AppRuntime { get => _sysInfo.Runtime; }
        public static global::System.Version JavaVersion { get => _sysInfo.JavaVersion; }

        public static ulong FreeMemoryKB
        {
            get
            {
                UpdateData();

                return _ramInfo.Free;
            }
        }

        public static ulong TotalMemoryKB { get => _ramInfo.Total; }

        public static string GpuName { get => _gpuInfo.Name; }
        public static string GpuVendor { get => _gpuInfo.Brand; }
        public static ulong GpuMemoryTotal { get => _gpuInfo.MemoryTotal; }

        public static string CpuName { get => _cpuInfo.Name; }
        public static string CpuVendor { get => _cpuInfo.Brand; }
        public static string CpuArchitecture { get => _cpuInfo.Architecture; }

        public static double CpuFrequencyMHz
        {
            get
            {
                UpdateData();

                return _cpuInfo.Frequency;
            }
        }

        public static int CpuLogicalCores { get => _cpuInfo.LogicalCores; }
        public static int CpuPhysicalCores { get => _cpuInfo.PhysicalCores; }

        public static bool IsMono { get => _sysInfo.IsMono; }
        public static bool IsLinux { get => _sysInfo.OperatingSystemType == OperatingSystemType.Linux; }
        public static bool IsUnity { get => _sysInfo.OperatingSystemType == OperatingSystemType.Unity5; }
        public static bool IsBSD { get => _sysInfo.OperatingSystemType == OperatingSystemType.BSD; }
        public static bool IsHaiku { get => _sysInfo.OperatingSystemType == OperatingSystemType.Haiku; }
        public static bool IsWindows { get => _sysInfo.OperatingSystemType == OperatingSystemType.Windows; }
        public static bool IsSolaris { get => _sysInfo.OperatingSystemType == OperatingSystemType.Solaris; }
        public static bool IsMacOSX { get => _sysInfo.OperatingSystemType == OperatingSystemType.MacOSX; }
        public static bool IsWebAssembly { get => _sysInfo.OperatingSystemType == OperatingSystemType.WebAssembly; }
        public static bool Is32Bit { get => Bits == 32; }
        public static bool Is64Bit { get => Bits == 64; }

        public static int Bits { get => IntPtr.Size * 8; }

        private static void UpdateData()
        {
            _sysInfo.Update();

            _hwInfo = _sysInfo.Hardware;
            _gpuInfo = _hwInfo.GPUs.First();
            _cpuInfo = _hwInfo.CPUs.First();
            _ramInfo = _hwInfo.RAM;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knacka.Se.ProtobufGenerator
{
    public class HaveProtoc : IHaveProtoc
    {

        const string ProtoBufToolsPackage = "google.protobuf.tools";
        const string GrpcToolsPackage = "grpc.tools";
        const string ProtoCBinary = "protoc.exe";
        const string GrpcPluginBinary = "grpc_csharp_plugin.exe";
        const string BinPath64 = @"tools\windows_x64\" + ProtoCBinary;
        const string BinPath32 = @"tools\windows_x86\" + ProtoCBinary;

        private string _protocPath = null;
        private string _grpcPath = null;
        private bool _grpc;
        private string _localFilepath;
        private string _toolsPackage;

        public HaveProtoc(string path, bool grpc = false)
        {
            _localFilepath = path;
            _grpc = grpc;
            _toolsPackage = grpc ? GrpcToolsPackage : ProtoBufToolsPackage;
        }

        public bool HaveFoundProtoc 
            => ProtocPath != null;

        public string ProtocPath 
            => _protocPath ?? (_protocPath = TryResolvePath());

        public bool HaveFoundGprc 
            => GrpcPath != null;

        public string GrpcPath 
            => _grpcPath ?? (_grpcPath = TryResolveGrpcPath());

        private string TryResolvePath()
        {
            var path = FindExePath(ProtoCBinary);

            if (string.IsNullOrEmpty(path))
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    path = FindExeInProfileNugets(_toolsPackage, BinPath64)
                        ?? FindExeInLocalPackages(_toolsPackage, BinPath64);
                }
                else
                {
                    path = FindExeInProfileNugets(_toolsPackage, BinPath32)
                        ?? FindExeInLocalPackages(_toolsPackage, BinPath32);
                }
            }

            return path;
        }

        private string TryResolveGrpcPath()
        {
            var protocPath = ProtocPath;
            if (!string.IsNullOrEmpty(protocPath))
            {
                var grpcPath = Path.Combine(Path.GetDirectoryName(protocPath), GrpcPluginBinary);
                if (File.Exists(grpcPath))
                {
                    return grpcPath;
                }
                // else: protoc exists, but no plugin found
            }
            // else: no protoc found

            return null;
        }

        private string FindExeInLocalPackages(string googleProtobufToolsName, string pathInPackage)
        {
            if (!string.IsNullOrEmpty(_localFilepath))
            {
                var tmpDir = _localFilepath;
                while (Directory.Exists(tmpDir))
                {
                    var tmpPackagesDir = Path.Combine(tmpDir, "packages");
                    if (Directory.Exists(tmpPackagesDir))
                    {
                        var dirName = Directory.GetDirectories(tmpPackagesDir)
                            .Where(x => x.StartsWith(googleProtobufToolsName))
                            .FirstOrDefault();
                        if (!string.IsNullOrEmpty(dirName))
                        {
                            var tmpPath = Path.Combine(dirName, pathInPackage);
                            if (File.Exists(tmpPath))
                            {
                                return tmpPath;
                            }
                        }

                    }
                    try
                    {
                        tmpDir = Directory.GetParent(tmpDir).FullName;
                    }
                    catch (Exception)
                    {
                        tmpDir = null;
                    }
                }
            }
            return null;
        }

        private static string FindExeInProfileNugets(string package, string pathInPackage)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(path))
            {
                path = Path.Combine(path, ".nuget\\packages");
                if (Directory.Exists(path))
                {
                    path = Path.Combine(path, package);
                    if (Directory.Exists(path))
                    {
                        var dirs = Directory.GetDirectories(path).OrderByDescending(x => x).ToArray();
                        if (dirs != null && dirs.Length > 0)
                        {
                            path = dirs[0];
                            path = Path.Combine(path, pathInPackage);
                            if (File.Exists(path))
                            {
                                return path;
                            }
                        }
                    }
                }
            }
            return path;
        }

        static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == String.Empty)
                {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                        {
                            if (CanExecute(path))
                            {
                                return Path.GetFullPath(path);
                            }
                        }
                    }
                }
                return null;
            }
            return Path.GetFullPath(exe);
        }

        private static bool CanExecute(string path)
        {
            try
            {
                using (var proc = new Process())
                {
                    var psi = proc.StartInfo;
                    psi.FileName = path;
                    psi.Arguments = "--help";
                    psi.RedirectStandardError = psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;
                    proc.Start();
                    var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                    var stderrTask = proc.StandardError.ReadToEndAsync();
                    if (!proc.WaitForExit(1000))
                    {
                        try { proc.Kill(); } catch { }
                    }
                    var exitCode = proc.ExitCode;
                    var stdout = "";
                    var stderr = "";
                    if (stdoutTask.Wait(1000)) stdout = stdoutTask.Result;
                    if (stderrTask.Wait(1000)) stderr = stderrTask.Result;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

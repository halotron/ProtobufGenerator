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

        private string _path = null;
        private string _localFilepath;

        public HaveProtoc(string path)
        {
            _localFilepath = path;
        }

        public bool HaveFoundProtoc
        {
            get
            {
                if (_path == null)
                    _path = TryResolvePath();
                return _path != null;
            }
        }

        public string ProtocPath
        {
            get
            {
                if (_path == null)
                    _path = TryResolvePath();
                return _path;
            }
        }

        private string TryResolvePath()
        {
            var path = FindExePath("protoc.exe");

            if (string.IsNullOrEmpty(path))
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    path = FindExeInProfileNugets("google.protobuf.tools", "tools\\windows_x64\\protoc.exe");
                    if (path == null)
                    {
                        path = FindExeInLocalPackages("google.protobuf.tools", "tools\\windows_x64\\protoc.exe");
                    }
                }
                else
                {
                    path = FindExeInProfileNugets("google.protobuf.tools", "tools\\windows_x86\\protoc.exe");
                    if (path == null)
                    {
                        path = FindExeInLocalPackages("google.protobuf.tools", "tools\\windows_x86\\protoc.exe");
                    }
                }
            }
            return path;
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

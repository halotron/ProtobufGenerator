using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knacka.Se.ProtobufGenerator
{
    public class GenerateFromProto : ICanGenerateFromProto
    {
        private readonly string _protocPath;

        public GenerateFromProto(string protocPath)
        {
            _protocPath = protocPath;
        }

        public byte[] GenerateCsharpFromProto(string protoContent, string protoDirPath)
        {
            if (string.IsNullOrEmpty(_protocPath))
                return null;

            string stdout, stderr;

            var infile = Path.GetTempFileName();
            var infileDir = Path.GetDirectoryName(infile);
            File.WriteAllText(infile, protoContent);
            string outdir = GetTempDir();

            var exitCode = RunProtoc(_protocPath, $"--csharp_out={outdir} --proto_path={infileDir} {infile}", infileDir, out stdout, out stderr);

            var files = Directory.GetFiles(outdir);
            if (files != null && files.Any())
            {
                var first = Path.Combine(outdir, files[0]);
                if (File.Exists(first))
                {
                    var content = File.ReadAllBytes(first);
                    CleanTempDir(outdir);
                    return content;
                }
            }
            CleanTempDir(outdir);
            return null;
        }

        private static void CleanTempDir(string outdir)
        {
            try
            {
                Directory.Delete(outdir, true);
            }
            catch (Exception)
            {
            }
        }

        private static string GetTempDir()
        {
            var tmpPath = Path.GetTempPath();
            string tempDir = null;
            var tries = 100;
            while (tempDir == null || !Directory.Exists(tempDir))
            {
                tries--;
                if (tries <= 0)
                    return null;
                var path = Path.Combine(tmpPath, new Random().Next().ToString());
                try
                {
                    Directory.CreateDirectory(path);
                    tempDir = path;
                }
                catch (Exception)
                {
                    tempDir = null;
                }
            }
            return tempDir;
        }

        static int RunProtoc(string path, string arguments, string workingDir, out string stdout, out string stderr)
        {
            using (var proc = new Process())
            {
                var psi = proc.StartInfo;
                psi.FileName = path;
                psi.Arguments = arguments;
                if (!string.IsNullOrEmpty(workingDir)) psi.WorkingDirectory = workingDir;
                psi.RedirectStandardError = psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                proc.Start();
                var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                var stderrTask = proc.StandardError.ReadToEndAsync();
                if (!proc.WaitForExit(5000))
                {
                    try { proc.Kill(); } catch { }
                }
                var exitCode = proc.ExitCode;
                stderr = stdout = "";
                if (stdoutTask.Wait(1000)) stdout = stdoutTask.Result;
                if (stderrTask.Wait(1000)) stderr = stderrTask.Result;

                return exitCode;
            }
        }
    }
}


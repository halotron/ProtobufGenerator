using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public byte[] GenerateCsharpFromProto(string protoContent, string protoDirPath, string infile)
        {
            if (string.IsNullOrEmpty(_protocPath))
                return null;

            string stdout, stderr;

            string infileDir = GetTempDir();

            // Copy specified proto file to the working temp directory.
            var originalInfile = Path.Combine(protoDirPath, infile);
            File.Copy(originalInfile, Path.Combine(infileDir, infile));

            // Copy imported proto files to the same directory so that protoc can find them.
            foreach (var importedPath in GetImportedProtoPaths(protoContent))
            {
                var dest = Path.Combine(infileDir, importedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));

                File.Copy(Path.Combine(protoDirPath, importedPath), dest);
            }

            string outdir = GetTempDir();

            var exitCode = RunProtoc(_protocPath, $"--csharp_out={outdir} --proto_path={infileDir} {infile}", infileDir, out stdout, out stderr);

            var files = Directory.GetFiles(outdir);
            if (files != null && files.Any())
            {
                var first = Path.Combine(outdir, files[0]);
                if (File.Exists(first))
                {
                    var content = File.ReadAllBytes(first);
                    CleanTempDir(infileDir);
                    CleanTempDir(outdir);
                    return content;
                }
            }
            CleanTempDir(infileDir);
            CleanTempDir(outdir);
            return null;
        }

        private static string[] GetImportedProtoPaths(string protoContent)
        {
            return Regex.Split(protoContent, @"\r?\n")
                .Where((x) => x.StartsWith("import"))
                .Select((x) => Regex.Match(x, @"""(.*)""").Groups[1].Value)
                .Select((x) => x.Replace("/", "\\"))
                .ToArray();
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


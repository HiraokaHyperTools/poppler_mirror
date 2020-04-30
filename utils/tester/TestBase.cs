using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace tester
{
    public class TestBase
    {
        private string pdftoppmExe => Resolve("%pdftoppmexe%");
        private string pdfattachExe => Resolve("%pdfattachexe%");
        private string pdftocairoExe => Resolve("%pdftocairoexe%");
        private string workDir = null;

        protected string Resolve(string path, bool mkdirAndCwd = false)
        {
            var fullPath = Path.Combine(
                TestContext.CurrentContext.WorkDirectory, // fallback
                Environment.ExpandEnvironmentVariables(path)
            );
            if (mkdirAndCwd)
            {
                Directory.CreateDirectory(fullPath);
                workDir = fullPath;
            }
            return fullPath;
        }

        protected string Mkdir(string path)
        {
            return Resolve(path, true);
        }

        protected void pdftoppm(params string[] args)
        {
            RunCmd(
                pdftoppmExe,
                string.Join(
                    " ",
                    args.Select(arg => "\"" + arg + "\"")
                )
            );
        }

        protected int RunCmd(
            string exe,
            string args,
            int exitCode = 0
        )
        {
            args = Environment.ExpandEnvironmentVariables(args);
            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                WorkingDirectory = workDir ?? TestContext.CurrentContext.WorkDirectory,
            };
            var p = Process.Start(psi);
            p.WaitForExit();
            if (p.ExitCode != exitCode)
            {
                throw new Exception($"Unexpected ExitCode: {p.ExitCode} ≠ {exitCode} (expected)\n exe: {exe}\nargs: {args}");
            }
            return p.ExitCode;
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var dir = Mkdir("%outd%/01");
            pdftoppm("-r", "10", "%ind%/pdfs/pages.pdf", "ppm");
            pdftoppm("-r", "10", "-png", "%ind%/pdfs/pages.pdf", "png");
            pdftoppm("-r", "10", "-jpeg", "%ind%/pdfs/pages.pdf", "jpeg");
            pdftoppm("-r", "10", "-tiff", "%ind%/pdfs/pages.pdf", "tiff");
            Assert.Pass();
        }

        [Test]
        public void TestCommands()
        {
            RunCmd(pdfattachExe, "", exitCode: 99);
            RunCmd(pdftocairoExe, "", exitCode: 99);
        }
    }
}

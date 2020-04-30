using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace tester
{
    public class TestBase
    {
        protected string BinariesDirectory =>
            Environment.GetEnvironmentVariable("BinariesDirectory");
        protected string bin32 =>
            Path.Combine(BinariesDirectory, "poppler-release", "x86", "bin");
        protected string pdftoppm32 =>
            Path.Combine(bin32, "pdftoppm.exe");

        protected string SourcesDirectory =>
            Environment.GetEnvironmentVariable("SourcesDirectory");
        protected string tester =>
            Path.Combine(SourcesDirectory, "utils", "tester");

        protected void RunPdftoppm(
            string args
        )
        {
            RunCmd(pdftoppm32, args);
        }

        protected void RunCmd(
            string exe,
            string args
        )
        {
            args = args.Replace("@tester", tester);
            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                WorkingDirectory = TestContext.CurrentContext.WorkDirectory,
            };
            var p = Process.Start(psi);
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new Exception($"ExitCode: {p.ExitCode}\nexe: {exe}\nargs: {args}");
            }
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            RunPdftoppm("\"@tester/pdfs/pages.pdf\" out01-ppm");
            RunPdftoppm("-png \"@tester/pdfs/pages.pdf\" out01-png");
            RunPdftoppm("-jpeg \"@tester/pdfs/pages.pdf\" out01-jpeg");
            RunPdftoppm("-tiff \"@tester/pdfs/pages.pdf\" out01-tiff");
            Assert.Pass();
        }
    }
}

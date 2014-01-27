using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StrongNaming.Tests
{
    [TestClass]
    public class Tests
    {
        private const string ShellKey = "shell";

        public TestContext TestContext { get; set; }

        internal PowerShell Shell
        {
            get
            {
                var shell = TestContext.Properties[ShellKey] as PowerShell;
                if (shell != null)
                {
                    shell.Commands.Clear();
                    return shell;
                }
                throw new InvalidOperationException();
            }
        }

        [TestInitialize]
        public void Init()
        {
            var ps = PowerShell.Create();
            ps.AddScript("ipmo .\nivot.powershell.strongnaming.dll").Invoke();
            TestContext.Properties.Add(ShellKey, ps);
        }

        [TestMethod]
        public void Test_Current_Working_Directory()
        {
            string pwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var result = Shell.AddScript("$pwd").Invoke<string>().Single();
            Assert.AreEqual(result, pwd, "Unexpected $pwd");
        }

        [TestMethod]
        public void Test_Sign_Primary_Assembly()
        {
            var result =
                Shell.AddScript(
                    "$k = import-strongnamekeypair .\\key.snk;" +
                    "set-strongname .\\dummylibrary.dll -force -key $k;" +
                    "test-strongname .\\dummylibrary.dll")
                    .Invoke<bool>().Single();

            Assert.IsTrue(result, "Assembly was not signed.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Shell.Dispose();
            TestContext.Properties[ShellKey] = null;
        }
    }
}

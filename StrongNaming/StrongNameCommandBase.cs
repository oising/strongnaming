using System;
using System.IO;
using System.Management.Automation;

namespace StrongNaming
{
    public abstract class StrongNameCommandBase : PSCmdlet
    {       
        protected const string NounStrongName = "StrongName";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true
            )]
        [Alias("PSPath")]
        [ValidateNotNull]
        public string[] AssemblyFile { get; set; }

        protected override void ProcessRecord()
        {
            foreach (string unresolvedPath in AssemblyFile)
            {
                // Convert to provider-internal path (for win32)
                string path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(unresolvedPath);

                if (!File.Exists(path))
                {
                    // TODO: localize
                    // not found, so try next and write non-terminating error
                    WriteError(
                        new ErrorRecord(
                            new FileNotFoundException(String.Format("Assembly file '{0}' does not exist.", unresolvedPath)),
                            "AssemblyFileNotFound", ErrorCategory.ObjectNotFound, unresolvedPath));
                }
                else
                {
                    WriteVerbose("Processing " + path);
                    ProcessAssemblyFile(path);
                }
            }
        }

        protected abstract void ProcessAssemblyFile(string filePath);
    }
}
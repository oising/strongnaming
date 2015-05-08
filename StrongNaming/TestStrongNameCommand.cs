using System.Management.Automation;
using Mono.Cecil;

namespace StrongNaming
{
    using System;

    [OutputType(typeof(bool))]
    [Cmdlet(VerbsDiagnostic.Test, NounStrongName)]
    public class TestStrongNameCommand : StrongNameCommandBase
    {
        protected override void ProcessAssemblyFile(string filePath)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(filePath);
                WriteObject(assembly.Name.HasPublicKey);
            }
            catch (BadImageFormatException)
            {
                // TODO: localize
                WriteVerbose("File " + filePath + " is not a valid .NET assembly.");
                WriteObject(false);
            }
        }
    }
}
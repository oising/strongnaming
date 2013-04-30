using System.Management.Automation;
using Mono.Cecil;

namespace StrongNaming
{
    [OutputType(typeof(bool))]
    [Cmdlet(VerbsDiagnostic.Test, NounStrongName)]
    public class TestStrongNameCommand : StrongNameCommandBase
    {
        protected override void ProcessAssemblyFile(string filePath)
        {
            var assembly = AssemblyDefinition.ReadAssembly(filePath);
            WriteObject(assembly.Name.HasPublicKey);
        }
    }
}
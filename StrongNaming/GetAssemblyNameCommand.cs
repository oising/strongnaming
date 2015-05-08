using System;
using System.Management.Automation;
using System.Reflection;
using Mono.Cecil;

namespace StrongNaming
{
    [OutputType(typeof(AssemblyName))]
    [Cmdlet(VerbsCommon.Get, "AssemblyName")]
    public class GetAssemblyNameCommand : StrongNameCommandBase
    {
        protected override void ProcessAssemblyFile(string filePath)
        {
            AssemblyName name;

            try
            {
                name = AssemblyName.GetAssemblyName(filePath);
            }
            catch (BadImageFormatException e)
            {
                // TODO: localize
                WriteError(
                    new ErrorRecord(
                        e,
                        filePath + " is not a valid .NET assembly.",
                        ErrorCategory.InvalidOperation,
                        filePath));
                return;
            }

            var psobj = PSObject.AsPSObject(name);

            var definition = AssemblyDefinition.ReadAssembly(filePath);
            psobj.Properties.Add(
                new PSNoteProperty("AssemblyReferences",
                    definition.MainModule.AssemblyReferences));

            WriteObject(psobj);
        }
    }
}
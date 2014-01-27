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
            var name = AssemblyName.GetAssemblyName(filePath);
            var psobj = PSObject.AsPSObject(name);
            
            var definition = AssemblyDefinition.ReadAssembly(filePath);
            psobj.Properties.Add(
                new PSNoteProperty("AssemblyReferences",
                    definition.MainModule.AssemblyReferences));

            WriteObject(psobj);
        }
    }
}
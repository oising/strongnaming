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
            WriteObject(AssemblyName.GetAssemblyName(filePath));
        }
    }
}
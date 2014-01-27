using System;
//using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;

namespace StrongNaming
{
    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsCommon.Set, NounStrongName, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetStrongNameCommand : StrongNameCommandBase
    {
        private string _actionText;
        
        [Parameter(Mandatory = true,
            HelpMessage = "Use Import-StrongNameKeyPair to create this" +
                " parameter from an SNK or PFX file.")]
        [ValidateNotNull]
        public StrongNameKeyPair KeyPair { get; set; }

        [Parameter]
        public SwitchParameter NoBackup { get; set; }

        [Parameter]
        public SwitchParameter Passthru { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter DelaySign { get; set; }

        protected override void BeginProcessing()
        {
            WriteVerbose("Loaded SNK.");

            // TODO: localize
            _actionText = NoBackup.IsPresent ? "Sign without backup" : "Sign with backup";
        }

        protected override void ProcessAssemblyFile(string filePath)
        {
            var assembly = AssemblyDefinition.ReadAssembly(filePath);

            // Support -WhatIf
            if (ShouldProcess(assembly.FullName, _actionText))
            {
                if (assembly.Name.HasPublicKey && (!Force.IsPresent))
                {
                    // TODO: localize
                    WriteWarning("Assembly '" + assembly.Name.Name + "' already has a strong name. Use -Force to sign.");
                    return;
                }

                if (NoBackup.IsPresent == false)
                {
                    string backupPath = Path.ChangeExtension(filePath, "bak");

                    // should backup file?
                    if (File.Exists(backupPath))
                    {
                        // TODO: localize
                        WriteWarning("Skipping assembly " + filePath + " as a backup at " + backupPath + " already exists.");
                        return;
                    }

                    File.Copy(filePath, backupPath);
                    WriteVerbose("Assembly was backed up to: " + backupPath);
                }

                assembly.Name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
                assembly.Name.PublicKey = KeyPair.PublicKey;
                assembly.Name.HasPublicKey = true;
                assembly.Name.Attributes &= AssemblyAttributes.PublicKey;

                byte[] token = GetKeyTokenFromKey(KeyPair.PublicKey);
                bool aborting = false;

                // Does the primary assembly have any unsigned references?
                if (assembly.MainModule.AssemblyReferences.Any(reference => reference.PublicKeyToken.Length == 0))
                {
                    if (Force ||
                        ShouldContinue(
                            "One or more assembly references are not referencing strong named assemblies. " +
                            "If you continue, these references will be updated to use the same public key token " +
                            "as the primary assembly. Use -Force to avoid this prompt in future." +
                            "\n\nNOTE: The referenced assemblies will still need to be given a strong name " +
                            "or this assembly will fail to load at runtime.",
                            "Assembly References"))
                    {
                        foreach (var reference in assembly.MainModule.AssemblyReferences)
                        {
                            WriteVerbose("Examining " + reference.Name);

                            if (reference.PublicKeyToken.Length > 0)
                            {
                                WriteVerbose("Skipping signed reference " + reference.Name);
                                continue;
                            }

                            WriteVerbose("Setting public key token for reference: " + reference.Name);
                            reference.PublicKeyToken = token;
                        }
                    }
                    else
                    {
                        aborting = true;
                    }
                }

                if (!aborting)
                {
                    if (DelaySign.IsPresent)
                    {
                        assembly.Write(filePath);
                        WriteVerbose("Using delay-signing.");
                    }
                    else
                    {
                        assembly.Write(filePath,
                            new WriterParameters
                            {
                                StrongNameKeyPair = KeyPair
                            });
                    }

                    WriteVerbose("Assembly file " + filePath + " was (re)signed successfully.");

                    if (Passthru)
                    {
                        WriteObject(new FileInfo(filePath));
                    }
                }
            }
        }

        private static byte[] GetKeyTokenFromKey(byte[] fullKey)
        {
            byte[] hash;
            using (var sha1 = SHA1.Create())
            {
                hash = sha1.ComputeHash(fullKey);
            }

            return hash.Reverse().Take(8).ToArray();
        }
    }
}


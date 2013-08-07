using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;

namespace StrongNaming
{
    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsCommon.Set, NounStrongName, SupportsShouldProcess = true)]
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
                foreach (var reference in assembly.MainModule.AssemblyReferences)
                {
                    if (reference.PublicKeyToken.Length != 0)
                        continue;

                    reference.PublicKeyToken = token;
                }

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

        private static byte[] GetKeyTokenFromKey(byte[] fullKey)
        {
            byte[] hash;
            using (SHA1CryptoServiceProvider sha1 = SHA1CryptoServiceProvider.Create())
                hash = sha1.ComputeHash (fullKey);

            return hash.Reverse().Take(8).ToArray();
        }
    }
}


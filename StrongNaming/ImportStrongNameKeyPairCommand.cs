using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace StrongNaming
{
    [Cmdlet(VerbsData.Import, "StrongNameKeyPair",
        DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
    [OutputType(typeof(StrongNameKeyPair))]
    public class ImportStrongNameKeyPairCommand : PSCmdlet
    {
        private const string ParamSetPfx = "PFX";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty]
        public string KeyFile { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ParamSetPfx)]
        [ValidateNotNull]
        public SecureString Password { get; set; }

        protected override void EndProcessing()
        {
            var filePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(KeyFile);
            byte[] keyPairBytes;

            if (!File.Exists(filePath))
            {
                // TODO: localize
                ThrowTerminatingError(
                    new ErrorRecord(
                        new FileNotFoundException(String.Format("Key file '{0}' does not exist.",
                            KeyFile)),
                        "KeyFileNotFound", ErrorCategory.ObjectNotFound, KeyFile));
            }

            if (ParameterSetName == ParamSetPfx)
            {
                // File is PFX, password-protected
                var cert = new X509Certificate2(filePath, Password, X509KeyStorageFlags.Exportable);

                var provider = cert.PrivateKey as RSACryptoServiceProvider;
                if (provider == null)
                {
                    // TODO: localize
                    ThrowTerminatingError(
                        new ErrorRecord(
                            new InvalidOperationException(
                                String.Format("Key file '{0}' is not a valid password-protected RSA-CSP PFX file.",
                                    KeyFile)),
                            "KeyFileInvalid", ErrorCategory.InvalidOperation, KeyFile));
                }

                //// ReSharper disable PossibleNullReferenceException
                keyPairBytes = provider.ExportCspBlob(includePrivateParameters: true);
                //// ReSharper restore PossibleNullReferenceException
            }
            else
            {
                // regular SNK
                keyPairBytes = File.ReadAllBytes(filePath);
            }

            WriteObject(new StrongNameKeyPair(keyPairBytes));
        }
    }
}

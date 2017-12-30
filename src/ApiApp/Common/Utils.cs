using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ApiApp.Common
{
    public class Utils
    {
        public static string GenerateWriteSasUrl(CloudBlockBlob blockblob, int sasExpireHours = 1)
        {
            string sas = blockblob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(sasExpireHours)
                });

            return blockblob.Uri.AbsoluteUri + sas;
        }

        public static string GenerateReadSasUrl(CloudBlockBlob blockblob, int sasExpireHours = 24)
        {
            string sas = blockblob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(sasExpireHours)
                });

            return blockblob.Uri.AbsoluteUri + sas;
        }
    }
}
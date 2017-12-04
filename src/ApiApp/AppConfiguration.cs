using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace ApiApp
{
    public class AppConfiguration
    {
        // TODO: Go to config
        private const string StorageSecretKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/storagecs";

        private static readonly AzureServiceTokenProvider AzureServiceTokenProvider = new AzureServiceTokenProvider();

        private static readonly KeyVaultClient KeyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(AzureServiceTokenProvider.KeyVaultTokenCallback));

        public string GetStorageConnectionString()
        {
            return KeyVaultClient.GetSecretAsync(StorageSecretKeyVaultLocation).GetAwaiter().GetResult().Value;
        }
    }
}
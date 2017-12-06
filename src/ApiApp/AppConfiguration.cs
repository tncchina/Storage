using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.Documents.Client;

namespace ApiApp
{
    public class AppConfiguration
    {
        // TODO: Go to config
        private const string StorageCSKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/storagecs";
        private const string DBUrlKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/dbendpointurl";
        private const string DBKeyKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/dbkey";
        private const string DatabaseName = "goldenmonkey";
        private const string AnimalImageCollectionName = "animalimages";
        private const string AnimalImageContainerName = "animalimages";

        private static readonly AzureServiceTokenProvider AzureServiceTokenProvider = new AzureServiceTokenProvider();

        private static readonly KeyVaultClient KeyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(AzureServiceTokenProvider.KeyVaultTokenCallback));

        private static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
            KeyVaultClient.GetSecretAsync(StorageCSKeyVaultLocation).GetAwaiter().GetResult().Value);

        private static readonly DocumentClient DBClient = new DocumentClient(
            new Uri(KeyVaultClient.GetSecretAsync(DBUrlKeyVaultLocation).GetAwaiter().GetResult().Value),
            KeyVaultClient.GetSecretAsync(DBKeyKeyVaultLocation).GetAwaiter().GetResult().Value);

        private static readonly Uri AICollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, AnimalImageCollectionName);

        public CloudStorageAccount BlobStorageAccount => StorageAccount;

        public DocumentClient CosmosDBClient => DBClient;

        public Uri AnimalImageCollectionUri => AICollectionUri;

        public string AnimalImageContainer => AnimalImageContainerName;

        public string AnimalImageCollectionId => AnimalImageCollectionName;

        public string DatabaseId => DatabaseName;
    }
}
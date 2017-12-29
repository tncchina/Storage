using System;

using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.Documents.Client.TransientFaultHandling.Strategies;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;

namespace ApiApp
{
    public class AppConfiguration
    {
        // TODO: Go to config
        private const string StorageCSKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/storagecs";
        private const string DBUrlKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/dbendpointurl";
        private const string DBKeyKeyVaultLocation = "https://tnckv4test.vault.azure.net/secrets/dbkey";
        private const string DatabaseName = "goldenmonkey";
        private const string OperationResultCollectionName = "operationresults";
        private const string AnimalImageCollectionName = "animalimages";
        private const string AnimalImageContainerName = "animalimages";

        private static readonly AzureServiceTokenProvider AzureServiceTokenProvider = new AzureServiceTokenProvider();

        private static readonly KeyVaultClient KeyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(AzureServiceTokenProvider.KeyVaultTokenCallback));

        private static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
            KeyVaultClient.GetSecretAsync(StorageCSKeyVaultLocation).GetAwaiter().GetResult().Value);

        private static readonly IReliableReadWriteDocumentClient DBClient = new DocumentClient(
            new Uri(KeyVaultClient.GetSecretAsync(DBUrlKeyVaultLocation).GetAwaiter().GetResult().Value),
            KeyVaultClient.GetSecretAsync(DBKeyKeyVaultLocation).GetAwaiter().GetResult().Value)
            .AsReliable(new DocumentDbRetryStrategy(RetryStrategy.DefaultExponential));

        private static readonly Uri AICollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, AnimalImageCollectionName);
        private static readonly Uri AIOperationCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, OperationResultCollectionName);

        public CloudStorageAccount BlobStorageAccount => StorageAccount;

        public IReliableReadWriteDocumentClient CosmosDBClient => DBClient;

        public Uri AnimalImageCollectionUri => AICollectionUri;

        public string AnimalImageContainer => AnimalImageContainerName;

        public string AnimalImageCollectionId => AnimalImageCollectionName;

        public Uri OperationResultCollectionUri => AIOperationCollectionUri;

        public string OperationResultCollectionId => OperationResultCollectionName;

        public string DatabaseId => DatabaseName;
    }
}
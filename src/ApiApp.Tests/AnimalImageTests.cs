using ApiApp.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiApp.Tests
{
    [TestClass]
    public partial class AnimalImageTests
    {
        static HttpClient client = new HttpClient();

        static Action<string> LogInfo = Console.WriteLine;

        [ClassInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            string tenant = "fb280588-57d8-4416-9821-e337832bfa02";
            string resource = "d90011f8-819f-4a37-b1c7-484b92ccee1d";
            string clientId = "3195d3f6-a444-45cf-8981-6c4258faf75d";

            AuthenticationContext authContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenant);
            AuthenticationResult result = null;
            try
            {
                result = authContext.AcquireTokenAsync(resource, clientId, new Uri("http://ApiApp.Tests"), new PlatformParameters(PromptBehavior.Auto)).Result;
            }
            catch (Exception ex)
            {
                LogInfo(ex.ToString());
            }

            client.BaseAddress = new Uri("https://apiapptest20171126015849.azurewebsites.net/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }


        [TestMethod]
        public async Task TestGetAnimalImage()
        {
            string content = "";
            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync("api/animalimages/");
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                }
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            catch (Exception e)
            {
                LogInfo(e.Message);
                throw;
            }
            finally
            {
                LogInfo($"received response status {response.StatusCode}, response content: {content}");
            }
        }

        [TestMethod]
        public async Task UploadOneAnimalImage()
        {
            // upload request
            string content = "";
            HttpResponseMessage response = null;
            var animal = new AnimalImage
            {
                Tag = "my first image",
                LocationId = "Golden Mokey Area",
                ImageName = "image0001",
                FileFormat = "JPG"
            };

            AnimalImage animalReturned = null;

            try
            {
                var stringContent = JsonConvert.SerializeObject(animal);
                var requestContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                response = await client.PostAsync("api/animalimages", requestContent);
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                }
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                animalReturned = JsonConvert.DeserializeObject<AnimalImage>(content);
                Assert.IsNotNull(animalReturned.ImageBlob);
                Assert.IsNotNull(animalReturned.UploadBlobSASUrl);
                Assert.IsNotNull(animalReturned.Id);
            }
            catch (Exception e)
            {
                LogInfo(e.Message);
                throw;
            }
            finally
            {
                LogInfo($"received response status {response.StatusCode}, response content: {content}");
            }

            // now can use the provided UploadBlobSASUrl to upload image
            string blobContent = "This is how I upload an image";
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(animalReturned.UploadBlobSASUrl));

            // Create operation: Upload a blob with the specified name to the container.
            // If the blob does not exist, it will be created. If it does exist, it will be overwritten.
            try
            {
                MemoryStream msWrite = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
                msWrite.Position = 0;
                using (msWrite)
                {
                    await blob.UploadFromStreamAsync(msWrite);
                }

                LogInfo($"Create operation succeeded for SAS {animalReturned.UploadBlobSASUrl}\n");
            }
            catch (StorageException e)
            {
                LogInfo($"Create operation failed for SAS {animalReturned.UploadBlobSASUrl} : {e.ToString()}\n");
                throw;
            }

            // Get
            response = await client.GetAsync($"api/animalimages/{animalReturned.Id}");
            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
                LogInfo($"received response status {response.StatusCode}, response content: {content}");
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            animalReturned = JsonConvert.DeserializeObject<AnimalImage>(content);
            Assert.IsNotNull(animalReturned.ImageBlob);
            Assert.IsNotNull(animalReturned.DownloadBlobSASUrl);
            Assert.IsNotNull(animalReturned.Id);
            Assert.AreEqual(animal.Tag, animalReturned.Tag);
            Assert.AreEqual(animal.LocationId, animalReturned.LocationId);
            Assert.AreEqual(animal.ImageName, animalReturned.ImageName);
            Assert.AreNotEqual(animal.FileFormat, animalReturned.FileFormat);
            Assert.AreEqual(animal.FileFormat.ToLowerInvariant(), animalReturned.FileFormat);
        }
    }
}

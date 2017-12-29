using ApiApp.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        const string ValidCsvMetadata =
@"原始文件编号,文件编号,文件格式,文件夹编号,相机编号,布设点位编号,相机安装日期,拍摄时间,工作天数,对象类别,物种名称,动物数量,性别,独立探测首张,备注
IMAG0001,L-TJH15-V08A-0001,JPG,L-TJH15-V08A,T0330,L-TJH15-V08A,2017-12-01,19:00,1,工作人员,,,,1,设置相机
IMAG0002,L-TJH15-V08A-0002,JPG,L-TJH15-V08A,T0330,L-TJH15-V08A,,19:00,1,工作人员,,,,,
IMAG0003,L-TJH15-V08A-0003,JPG,L-TJH15-V08A,T0330,L-TJH15-V08A,,19:00,1,工作人员
IMAG0004,L-TJH15-V08A-0004,AVI,L-TJH15-V08A,T0330,L-TJH15-V08A,,19:00,1,工作人员,,,,,,,,,
IMAG0025,L-TJH15-V08A-0005,JPG,L-TJH15-V08A,T0330,L-TJH15-V08A,,2:12,2,兽类,猪獾,1,,1";

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

        [TestMethod]
        public void TestCsvMetadataParsing()
        {
            List<AnimalImage> expectedValues = new List<AnimalImage>(new[]
            {
                new AnimalImage
                {
                    OriginalFileId = "L-TJH15-V08A-0001",
                    OriginalImageId = "IMAG0001",
                    FileFormat = "JPG",
                    OriginalFolderId = "L-TJH15-V08A",
                    CameraId = "T0330",
                    LocationId = "L-TJH15-V08A",
                    CameraInstallationDate = new DateTime(2017, 12, 01),
                    ShootingTime = DateTime.Parse("19:00"),
                    WorkingDays = 1,
                    Category = "工作人员",
                    IndependentProbeFirst = "1",
                    Notes = "设置相机"
                },
                new AnimalImage
                {
                    OriginalFileId = "L-TJH15-V08A-0002",
                    OriginalImageId = "IMAG0002",
                    FileFormat = "JPG",
                    OriginalFolderId = "L-TJH15-V08A",
                    CameraId = "T0330",
                    LocationId = "L-TJH15-V08A",
                    ShootingTime = DateTime.Parse("19:00"),
                    WorkingDays = 1,
                    Category = "工作人员"
                },
                new AnimalImage
                {
                    OriginalFileId = "L-TJH15-V08A-0003",
                    OriginalImageId = "IMAG0003",
                    FileFormat = "JPG",
                    OriginalFolderId = "L-TJH15-V08A",
                    CameraId = "T0330",
                    LocationId = "L-TJH15-V08A",
                    ShootingTime = DateTime.Parse("19:00"),
                    WorkingDays = 1,
                    Category = "工作人员"
                },
                new AnimalImage
                {
                    OriginalFileId = "L-TJH15-V08A-0004",
                    OriginalImageId = "IMAG0004",
                    FileFormat = "AVI",
                    OriginalFolderId = "L-TJH15-V08A",
                    CameraId = "T0330",
                    LocationId = "L-TJH15-V08A",
                    ShootingTime = DateTime.Parse("19:00"),
                    WorkingDays = 1,
                    Category = "工作人员"
                },
                new AnimalImage
                {
                    OriginalFileId = "L-TJH15-V08A-0005",
                    OriginalImageId = "IMAG0025",
                    FileFormat = "JPG",
                    OriginalFolderId = "L-TJH15-V08A",
                    CameraId = "T0330",
                    LocationId = "L-TJH15-V08A",
                    ShootingTime = DateTime.Parse("2:12"),
                    WorkingDays = 2,
                    Category = "兽类",
                    SpecicesName = "猪獾",
                    AnimalQuantity = 1,
                    IndependentProbeFirst = "1"
                }
            });

            List<AnimalImage> parsedData = AnimalImage.ReadFromCsv(ValidCsvMetadata);

            Assert.AreEqual(expectedValues.Count, parsedData.Count);

            for (int i = 0; i < expectedValues.Count; i++)
            {
                Assert.AreEqual(expectedValues[i].OriginalFileId, parsedData[i].OriginalFileId, $"Line {i}, field 'OriginalFileId'");
                Assert.AreEqual(expectedValues[i].OriginalImageId, parsedData[i].OriginalImageId, $"Line {i}, field 'OriginalImageId'");
                Assert.AreEqual(expectedValues[i].FileFormat, parsedData[i].FileFormat, $"Line {i}, field 'FileFormat'");
                Assert.AreEqual(expectedValues[i].OriginalFolderId, parsedData[i].OriginalFolderId, $"Line {i}, field 'OriginalFolderId'");
                Assert.AreEqual(expectedValues[i].CameraId, parsedData[i].CameraId, $"Line {i}, field 'CameraId'");
                Assert.AreEqual(expectedValues[i].LocationId, parsedData[i].LocationId, $"Line {i}, field 'LocationId'");
                Assert.AreEqual(expectedValues[i].CameraInstallationDate, parsedData[i].CameraInstallationDate, $"Line {i}, field 'CameraInstallationDate'");
                Assert.AreEqual(expectedValues[i].ShootingTime, parsedData[i].ShootingTime, $"Line {i}, field 'ShootingTime'");
                Assert.AreEqual(expectedValues[i].WorkingDays, parsedData[i].WorkingDays, $"Line {i}, field 'WorkingDays'");
                Assert.AreEqual(expectedValues[i].Category, parsedData[i].Category, $"Line {i}, field 'Category'");
                Assert.AreEqual(expectedValues[i].SpecicesName, parsedData[i].SpecicesName, $"Line {i}, field 'SpecicesName'");
                Assert.AreEqual(expectedValues[i].AnimalQuantity, parsedData[i].AnimalQuantity, $"Line {i}, field 'AnimalQuantity'");
                Assert.AreEqual(expectedValues[i].Sex, parsedData[i].Sex, $"Line {i}, field 'Sex'");
                Assert.AreEqual(expectedValues[i].IndependentProbeFirst, parsedData[i].IndependentProbeFirst, $"Line {i}, field 'IndependentProbeFirst'");
                Assert.AreEqual(expectedValues[i].Notes, parsedData[i].Notes, $"Line {i}, field 'Notes'");
            }
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using ApiApp.Models;
using System.Text;
using Newtonsoft.Json;

namespace ApiApp.Tests
{
    public partial class AnimalImageTests
    {
        //[TestMethod]
        public async Task GetUploadBatchInfo()
        {

            string content = "";
            HttpResponseMessage response = null;
            try
            {
                var uploadBatchRequest = new UploadBatchRequest { BatchTag = "Batch 1", BatchDescription = "First Batch in GM" };
                var stringContent = JsonConvert.SerializeObject(uploadBatchRequest);
                var requestContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                response = await client.PostAsync("api/actions/uploadbatch", requestContent);
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                }
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var info = JsonConvert.DeserializeObject<UploadBatchResponse>(content);
                //Assert.AreEqual(uploadBatchRequest.BatchTag, info.BatchTag);
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
        public async Task GetUploadBatchMissingMetadata()
        {
            await UploadBatchTestCore(
                new UploadBatchRequest
                {
                    BatchTag = "Batch 1",
                    BatchDescription = "First Batch in GM"
                },
                (response, content) =>
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                    var info = JsonConvert.DeserializeObject<OperationStatus>(content);
                    Assert.AreEqual("ProperyCannotBeEmpty", info.Code);
                    Assert.IsTrue(info.Message.Contains("CSVMetadata"));
                    return true;
                });
        }

        [TestMethod]
        public async Task GetUploadBatchBadCsvMetadata()
        {
            await UploadBatchTestCore(
                new UploadBatchRequest
                {
                    BatchTag = "Batch 1",
                    BatchDescription = "First Batch in GM",
                    CSVMetadata = "123"
                },
                (response, content) =>
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                    var info = JsonConvert.DeserializeObject<OperationStatus>(content);
                    Assert.AreEqual("BadCSVMetadata", info.Code);
                    return true;
                });
        }

        [TestMethod]
        public async Task GetUploadBatchValidCsvMetadata()
        {
            await UploadBatchTestCore(
                new UploadBatchRequest
                {
                    BatchTag = "Batch 1",
                    BatchDescription = "First Batch in GM",
                    CSVMetadata = AnimalImageTests.ValidCsvMetadata
                },
                (response, content) =>
                {
                    Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
                    Assert.IsTrue(string.IsNullOrEmpty(content));
                    Assert.IsTrue(response.Headers.Contains("Location"));
                    Assert.IsTrue(response.Headers.Location.AbsolutePath.StartsWith("/api/operationresults/"));
                    Assert.IsTrue(response.Headers.Contains("Retry-After"));
                    Assert.IsTrue(response.Headers.RetryAfter.Delta > TimeSpan.Zero);
                    return true;
                });
        }

        private static async Task UploadBatchTestCore(UploadBatchRequest request, Func<HttpResponseMessage, string, bool> validator)
        {
            string content = "";
            HttpResponseMessage response = null;
            try
            {
                var stringContent = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                response = await client.PostAsync("api/actions/uploadbatch", requestContent);
                try
                {
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                }

                Assert.IsTrue(validator(response, content));
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
    }
}

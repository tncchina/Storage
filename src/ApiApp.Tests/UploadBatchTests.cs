using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using ApiApp.Models;
using System.Text;
using Newtonsoft.Json;

namespace ApiApp.Tests
{
    public partial class AnimalImageTests
    {        
        [TestMethod]
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
                Assert.AreEqual(uploadBatchRequest.BatchTag, info.BatchTag);
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

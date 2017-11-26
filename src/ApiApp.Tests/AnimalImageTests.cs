using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;

namespace ApiApp.Tests
{
    [TestClass]
    public partial class AnimalImageTests
    {
        static HttpClient client = new HttpClient();

        Action<string> LogInfo = Console.WriteLine;

        [ClassInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            //client.BaseAddress = new Uri("http://apiapptest20171126015849.azurewebsites.net/");
            client.BaseAddress = new Uri("http://localhost:63513/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        [TestMethod]
        public async Task TestGetAnimalImage()
        {
            string content = "";
            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync("api/animalimages/1");
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
    }
}

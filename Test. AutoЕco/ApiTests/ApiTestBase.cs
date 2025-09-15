using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RestSharp;
using System;
using System.IO;

namespace Autotests_ai_ecosystem.ApiTests
{
    [TestFixture]
    public abstract class ApiTestBase
    {
        protected RestClient Client { get; private set; }
        protected IConfiguration Configuration { get; private set; }

        [SetUp]
        public void ApiSetup()
        {
            // Загрузка конфигурации
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var baseUrl = Configuration["ApiSettings:BaseUrl"];
            var timeout = int.Parse(Configuration["ApiSettings:Timeout"] ?? "30");

            // Проверка на null для baseUrl
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("BaseUrl is not configured in appsettings.json");
            }

            var options = new RestClientOptions(baseUrl)
            {
                Timeout = TimeSpan.FromSeconds(timeout),
                ThrowOnAnyError = false
            };

            Client = new RestClient(options);
        }

        [TearDown]
        public void ApiTearDown()
        {
            Client?.Dispose();
        }

        protected void LogResponse(RestResponse response)
        {
            Console.WriteLine("=== API RESPONSE ===");
            Console.WriteLine($"Status Code: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response URI: {response.ResponseUri}");
            Console.WriteLine($"Content Type: {response.ContentType}");
            Console.WriteLine($"Content Length: {response.Content?.Length ?? 0} bytes");
            Console.WriteLine("=== RESPONSE BODY ===");
            Console.WriteLine(response.Content);
            Console.WriteLine("=== END RESPONSE ===");
        }

        protected void VerifyResponseIsSuccessful(RestResponse response)
        {
            // Явное указание пространства имен NUnit.Framework для устранения неоднозначности
            NUnit.Framework.Assert.That(response.IsSuccessful, Is.True,
                $"Request failed. Status: {response.StatusCode}, Error: {response.ErrorMessage}");

            NUnit.Framework.Assert.That(response.Content, Is.Not.Null.Or.Empty,
                "Response content is null or empty");
        }
    }
}
using Autotests_ai_ecosystem.ApiTests;
using NUnit.Framework;
using RestSharp;
using System.Text.Json;

namespace Autotests_ai_ecosystem.Tests.Api
{
    [TestFixture]
    public class UsersApiTests : ApiTestBase
    {
        [Test]
        public void GetUsers_ShouldReturnListOfUsers()
        {
            // Arrange
            var request = new RestRequest("/users", Method.Get);

            // Act
            Console.WriteLine("Sending GET request to /users...");
            var response = Client.Execute(request);

            // Assert
            VerifyResponseIsSuccessful(response);
            LogResponse(response);

            // Дополнительные проверки
            Assert.That(response.ContentType, Does.Contain("application/json"),
                "Response should be in JSON format");

            // Парсим JSON для дополнительных проверок
            var users = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            Assert.That(users.ValueKind, Is.EqualTo(JsonValueKind.Array),
                "Response should be a JSON array");

            Console.WriteLine($"Number of users returned: {users.GetArrayLength()}");
        }

        [Test]
        public void GetUserById_ShouldReturnSpecificUser()
        {
            // Arrange
            int userId = 1;
            var request = new RestRequest($"/users/{userId}", Method.Get);

            // Act
            Console.WriteLine($"Sending GET request to /users/{userId}...");
            var response = Client.Execute(request);

            // Assert
            VerifyResponseIsSuccessful(response);
            LogResponse(response);

            // Проверяем, что в ответе есть ожидаемые поля
            Assert.That(response.Content, Does.Contain("id"),
                "Response should contain user id");
            Assert.That(response.Content, Does.Contain("name"),
                "Response should contain user name");
            Assert.That(response.Content, Does.Contain("email"),
                "Response should contain user email");
        }
    }
}
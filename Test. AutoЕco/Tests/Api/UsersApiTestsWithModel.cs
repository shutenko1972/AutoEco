using Autotests_ai_ecosystem.ApiTests;
using Autotests_ai_ecosystem.Models;
using NUnit.Framework;
using RestSharp;

namespace Autotests_ai_ecosystem.Tests.Api
{
    [TestFixture]
    public class UsersApiTestsWithModel : ApiTestBase
    {
        [Test]
        public void GetUsers_WithModel_ShouldReturnValidUsers()
        {
            // Arrange
            var request = new RestRequest("/users", Method.Get);

            // Act
            Console.WriteLine("Sending GET request to /users...");
            var response = Client.Execute<List<User>>(request);

            // Assert
            VerifyResponseIsSuccessful(response);
            LogResponse(response);

            // Проверки с использованием модели
            Assert.That(response.Data, Is.Not.Null, "Deserialized data should not be null");
            Assert.That(response.Data, Is.Not.Empty, "Should return at least one user");

            var firstUser = response.Data[0];
            Console.WriteLine($"First user: {firstUser.Name} ({firstUser.Email})");

            Assert.That(firstUser.Id, Is.GreaterThan(0), "User ID should be positive");
            Assert.That(firstUser.Name, Is.Not.Null.And.Not.Empty, "User name should not be empty");
            Assert.That(firstUser.Email, Is.Not.Null.And.Not.Empty, "User email should not be empty");
            Assert.That(firstUser.Email, Does.Contain("@"), "User email should contain @ symbol");
        }
    }
}
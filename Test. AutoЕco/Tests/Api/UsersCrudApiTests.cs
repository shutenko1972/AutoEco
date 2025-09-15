using Autotests_ai_ecosystem.ApiTests;
using Autotests_ai_ecosystem.Models;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Autotests_ai_ecosystem.Tests.Api
{
    [TestFixture]
    public class UsersCrudApiTests : ApiTestBase
    {
        private User? _createdUser; // Добавлен nullable модификатор
        private int _testUserId = 1; // Используем существующий ID для тестов

        [Test, Order(1)]
        public void CreateUser_ShouldReturnCreatedUser()
        {
            // Arrange
            var newUser = new User
            {
                Name = "Test User",
                Username = "testuser",
                Email = "test.user@example.com",
                Address = new Address
                {
                    Street = "Test Street",
                    Suite = "Apt. 123",
                    City = "Test City",
                    Zipcode = "12345",
                    Geo = new Geo { Lat = "40.7128", Lng = "-74.0060" }
                },
                Phone = "1-234-567-8901",
                Website = "testuser.org",
                Company = new Company
                {
                    Name = "Test Company",
                    CatchPhrase = "Test catch phrase",
                    Bs = "test business"
                }
            };

            var request = new RestRequest("/users", Method.Post);
            request.AddJsonBody(newUser);

            // Act
            Console.WriteLine("Sending POST request to create user...");
            var response = Client.Execute<User>(request);

            // Assert - Jsonplaceholder всегда возвращает 201 Created с ID=11
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created),
                "User creation should return 201 Created status");

            Assert.That(response.Data, Is.Not.Null, "Created user data should not be null");
            Assert.That(response.Data.Id, Is.EqualTo(11), "Jsonplaceholder always returns ID=11 for new users");
            Assert.That(response.Data.Name, Is.EqualTo(newUser.Name), "User name should match");

            _createdUser = response.Data;
            Console.WriteLine($"Mock response: Created user with ID: {_createdUser.Id}");
        }

        [Test, Order(2)]
        public void GetExistingUser_ShouldReturnUserData()
        {
            // Arrange - используем существующий ID (1-10)
            var request = new RestRequest($"/users/{_testUserId}", Method.Get);

            // Act
            Console.WriteLine($"Sending GET request to retrieve existing user {_testUserId}...");
            var response = Client.Execute<User>(request);

            // Assert
            VerifyResponseIsSuccessful(response);
            LogResponse(response);

            Assert.That(response.Data, Is.Not.Null, "User data should not be null");
            Assert.That(response.Data.Id, Is.EqualTo(_testUserId), "User ID should match");
            Assert.That(response.Data.Name, Is.Not.Null.And.Not.Empty, "User name should not be empty");
            Assert.That(response.Data.Email, Does.Contain("@"), "User email should contain @ symbol");
        }

        [Test, Order(3)]
        public void UpdateUser_ShouldReturnUpdatedData()
        {
            // Arrange - используем существующий ID
            var updatedUser = new User
            {
                Id = _testUserId,
                Name = "Updated Test User",
                Username = "updateduser",
                Email = "updated.user@example.com",
                Address = new Address
                {
                    Street = "Updated Street",
                    Suite = "Apt. 456",
                    City = "Updated City",
                    Zipcode = "67890",
                    Geo = new Geo { Lat = "34.0522", Lng = "-118.2437" }
                },
                Phone = "1-987-654-3210",
                Website = "updateduser.org",
                Company = new Company
                {
                    Name = "Updated Company",
                    CatchPhrase = "Updated catch phrase",
                    Bs = "updated business"
                }
            };

            var request = new RestRequest($"/users/{_testUserId}", Method.Put);
            request.AddJsonBody(updatedUser);

            // Act
            Console.WriteLine($"Sending PUT request to update user {_testUserId}...");
            var response = Client.Execute<User>(request);

            // Assert - Jsonplaceholder возвращает обновленные данные, но не сохраняет их
            VerifyResponseIsSuccessful(response);
            LogResponse(response);

            Assert.That(response.Data, Is.Not.Null, "Updated user data should not be null");
            Assert.That(response.Data.Id, Is.EqualTo(_testUserId), "User ID should remain the same");
            Assert.That(response.Data.Name, Is.EqualTo(updatedUser.Name), "User name should be updated in response");
        }

        [Test, Order(4)]
        public void DeleteUser_ShouldReturnOk()
        {
            // Arrange
            var request = new RestRequest($"/users/{_testUserId}", Method.Delete);

            // Act
            Console.WriteLine($"Sending DELETE request to remove user {_testUserId}...");
            var response = Client.Execute(request);

            // Assert - Jsonplaceholder всегда возвращает 200 OK, даже если пользователь не существует
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "Delete should return 200 OK status in mock API");

            LogResponse(response);
            Console.WriteLine($"Mock response: User {_testUserId} 'deleted'");
        }

        [Test, Order(5)]
        public void GetUserAfterDelete_ShouldStillReturnUser()
        {
            // Arrange - Jsonplaceholder не удаляет пользователей по-настоящему
            var request = new RestRequest($"/users/{_testUserId}", Method.Get);

            // Act
            Console.WriteLine($"Sending GET request after 'delete' to user {_testUserId}...");
            var response = Client.Execute<User>(request);

            // Assert - Jsonplaceholder всегда возвращает пользователя, даже после "удаления"
            VerifyResponseIsSuccessful(response);
            Assert.That(response.Data, Is.Not.Null, "User should still exist in mock API");
            Assert.That(response.Data.Id, Is.EqualTo(_testUserId), "User ID should match");

            Console.WriteLine("Mock API behavior: Users are never actually deleted");
        }

        [Test, Order(6)]
        public void GetAllUsers_ShouldReturnList()
        {
            // Arrange
            var request = new RestRequest("/users", Method.Get);

            // Act
            Console.WriteLine("Sending GET request to /users...");
            var response = Client.Execute<List<User>>(request);

            // Assert
            VerifyResponseIsSuccessful(response);
            Assert.That(response.Data, Is.Not.Null.And.Not.Empty, "Should return list of users");
            Assert.That(response.Data.Count, Is.EqualTo(10), "Jsonplaceholder always returns 10 users");

            Console.WriteLine($"Number of users: {response.Data.Count}");
        }

        [Test, Order(7)]
        public void CreateUser_WithMinimumData_ShouldSucceed()
        {
            // Arrange
            var minimalUser = new User
            {
                Name = "Minimal User",
                Email = "minimal@example.com"
            };

            var request = new RestRequest("/users", Method.Post);
            request.AddJsonBody(minimalUser);

            // Act
            Console.WriteLine("Sending POST request with minimal data...");
            var response = Client.Execute<User>(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created),
                "Should return 201 Created even with minimal data");

            Assert.That(response.Data, Is.Not.Null, "Response should not be null");
            Assert.That(response.Data.Id, Is.EqualTo(11), "Should return ID=11");
            Assert.That(response.Data.Name, Is.EqualTo(minimalUser.Name), "Name should match");

            Console.WriteLine("Minimal user creation test passed");
        }

        [Test, Order(8)]
        public void VerifyCreatedUserData_IfExists()
        {
            // Проверяем, был ли создан пользователь в первом тесте
            if (_createdUser == null)
            {
                Assert.Inconclusive("User was not created in previous test");
                return;
            }

            // Arrange
            var request = new RestRequest($"/users/{_createdUser.Id}", Method.Get);

            // Act
            Console.WriteLine($"Sending GET request to verify created user {_createdUser.Id}...");
            var response = Client.Execute<User>(request);

            // Assert - Jsonplaceholder может не иметь созданного пользователя
            Console.WriteLine($"Response status for created user: {response.StatusCode}");

            if (response.IsSuccessful)
            {
                Assert.That(response.Data, Is.Not.Null, "User data should not be null");
                Console.WriteLine("Created user exists in mock API (unexpected behavior)");
            }
            else
            {
                Console.WriteLine("Created user does not exist in mock API (expected behavior)");
            }

            LogResponse(response);
        }
    }
}
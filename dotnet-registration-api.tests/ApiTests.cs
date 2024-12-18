using dotnet_registration_api.Data.Entities;
using dotnet_registration_api.Data.Models;
using dotnet_registration_api.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace dotnet_registration_api.tests
{
    public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly DataHelper _helper;
        public ApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            var scope = _factory.Services.CreateScope();
            _helper = scope.ServiceProvider.GetRequiredService<DataHelper>();
            _helper.SeedData();
        }
        [Fact]
        public async Task GetUsers_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/users");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task GetUser_ReturnsOk()
        {
            var u = await _helper.GetUser();
            if (u == null)
            {
                throw new Exception("No user found");
            }
            var response = await _client.GetAsync($"/api/users/{u.Id}");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(result);
            Assert.Equal(u.Id, result.Id);
        }
        [Fact]
        public async Task GetUser_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/users/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task UpdateUser_ReturnsOk()
        {
            var u = await _helper.GetUser();
            if (u == null)
            {
                throw new Exception("No user found");
            }
            var update = new UpdateRequest()
            {
                FirstName = u.FirstName,
                LastName = "updated Lastname",
                Username = u.Username,
                OldPassword = "test",
                NewPassword = "test2"
            };
            var response = await _client.PutAsync($"/api/users/{u.Id}", new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await _client.GetAsync($"/api/users/{u.Id}");
            var updatedUser = await result.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(updatedUser);
            Assert.Equal("updated Lastname", updatedUser.LastName);
        }
        [Fact]
        public async Task UpdateUser_ReturnsNotFound()
        {
            var u = new UpdateRequest
            {
                FirstName = "Test1",
                LastName = "Lastname2",
                Username = "test123213",
                OldPassword = "test",
                NewPassword = "test123"
            };
            var response = await _client.PutAsync($"/api/users/9999", new StringContent(JsonConvert.SerializeObject(u), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task UpdateUser_ReturnsBadRequest()
        {
            var u = await _helper.GetUser();
            var u2 = await _helper.GetUser();
            if (u == null)
            {
                throw new Exception("No user found");
            }
            var update = new UpdateRequest()
            {
                FirstName = u.FirstName,
                LastName = "updated Lastname",
                Username = u.Username,
                OldPassword = "wrongpassword",
                NewPassword = "test2"
            };
            var response = await _client.PutAsync($"/api/users/{u.Id}", new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            update.Username = u2.Username;
            update.OldPassword = "test";
            var response2 = await _client.PutAsync($"/api/users/{u.Id}", new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }
        [Fact]
        public async Task DeleteProduct_ReturnsOk()
        {
            var u = _helper.GetUser();
            if (u == null)
            {
                throw new Exception("No user found");
            }
            var response = await _client.DeleteAsync($"/api/users/{u.Id}");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task DeleteProduct_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync($"/api/users/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RegisterUser_ReturnsOk()
        {
            var u = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser122",
                Password = "password"
            };
            var response = await _client.PostAsync("/api/users/register", new StringContent(JsonConvert.SerializeObject(u), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest()
        {
            var u = await _helper.GetUser();
            var u2 = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser122",
                Password = ""
            };
            var response = await _client.PostAsync("/api/users/register", new StringContent(JsonConvert.SerializeObject(u2), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            u2.Username = u.Username;
            u2.Password = "asdasdasd";
            var response2 = await _client.PostAsync("/api/users/register", new StringContent(JsonConvert.SerializeObject(u2), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }
        [Fact]
        public async Task Check_User_PasswordHash()
        {
            var u = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser123232",
                Password = "password"
            };
            var response = await _client.PostAsync("/api/users/register", new StringContent(JsonConvert.SerializeObject(u), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(result);
            Assert.Equal(HashHelper.HashPassword(u.Password), result.PasswordHash);
        }

    }
}
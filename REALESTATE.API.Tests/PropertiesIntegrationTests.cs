using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using REALESTATE_.API.Data;
using REALESTATE_.API.models;
using BCrypt.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace REALESTATE_.API.Tests
{
    public class PropertiesIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        CustomWebApplicationFactory _factory;

        public PropertiesIntegrationTests(
           CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProperties_ShouldReturnSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/Properties");

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }
        [Fact]
        public async Task GetMyProperties_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Act
            var response = await _client.GetAsync(
                "/api/Properties/my-properties");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        [Fact]
        public async Task GetMyProperties_ShouldReturnForbidden_WhenUserIsBuyer()
        {
            // Arrange
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "10"),
        new Claim(ClaimTypes.Email, "buyer@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    jwt);

            // Act
            var response = await _client.GetAsync(
                "/api/Properties/my-properties");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }
        [Fact]
        public async Task GetAdminStatistics_ShouldReturnSuccess_WhenUserIsAdmin()
        {
            // Arrange
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    jwt);

            // Act
            var response = await _client.GetAsync(
                "/api/Properties/admin-statistics");

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginData = new
            {
                email = "wrong@test.com",
                password = "WrongPassword123!"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                "/api/Owners/login",
                content);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                context.Owners.Add(new Owner
                {
                    Id = 100,
                    Name = "Test Buyer",
                    Email = "valid@test.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("ValidPassword123!"),
                    Role = "Buyer"
                });

                await context.SaveChangesAsync();
            }

            var loginData = new
            {
                email = "valid@test.com",
                password = "ValidPassword123!"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                "/api/Owners/login",
                content);

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.Contains("token", responseBody);
        }
        [Fact]
        public async Task Buyer_ShouldAccessMyFavorites_WithValidToken()
        {
            // Arrange
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "100"),
        new Claim(ClaimTypes.Email, "valid@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.GetAsync(
                "/api/Properties/my-favorites");

            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }
        [Fact]
        public async Task GetAdminStatistics_ShouldReturnForbidden_WhenUserIsOwner()
        {
            // Arrange
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "20"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.GetAsync(
                "/api/Properties/admin-statistics");

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }
        [Fact]
        public async Task Owner_ShouldCreateProperty_WithPendingStatus()
        {
            // Arrange
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var propertyData = new
            {
                title = "Modern Apartment",
                description = "Test apartment for integration testing",
                price = 1500000,
                address = "Alexandria",
                neighborhood = "Smouha",
                bedrooms = 3,
                bathrooms = 2,
                area = 150
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(propertyData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                "/api/Properties",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.Contains("\"status\":0", responseBody);
        }
        [Fact]
        public async Task PublicProperties_ShouldNotReturnPendingProperty()
        {
            // Act
            var response = await _client.GetAsync("/api/Properties");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain(
                "Modern Apartment",
                responseBody);
        }
        [Fact]
        public async Task Admin_ShouldApprovePendingProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property To Approve",
                    Description = "Integration test property",
                    Price = 2000000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 160,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Admin JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/approve/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var approvedProperty = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(approvedProperty);
                Assert.Equal(
                    PropertyStatus.Approved,
                    approvedProperty.Status);
            }
        }
        [Fact]
        public async Task Admin_ShouldRejectPendingProperty_WithRejectionReason()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property To Reject",
                    Description = "Integration test property",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Admin JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var rejectionReason = "\"Price is too high\"";

            var content = new StringContent(
                rejectionReason,
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/reject/{propertyId}",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var rejectedProperty = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(rejectedProperty);

                Assert.Equal(
                    PropertyStatus.Rejected,
                    rejectedProperty.Status);

                Assert.Equal(
                    "Price is too high",
                    rejectedProperty.RejectionReason);
            }
        }
        [Fact]
        public async Task Owner_ShouldResubmitRejectedProperty_AndSetStatusToPending()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Rejected Property",
                    Description = "Property for resubmission test",
                    Price = 1700000,
                    Address = "Alexandria",
                    Neighborhood = "Sidi Gaber",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 140,
                    Status = PropertyStatus.Rejected,
                    RejectionReason = "Price is too high",
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/resubmit/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var propertyAfterResubmit = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(propertyAfterResubmit);

                Assert.Equal(
                    PropertyStatus.Pending,
                    propertyAfterResubmit.Status);

                Assert.Null(
                    propertyAfterResubmit.RejectionReason);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotUpdatePropertyOwnedByAnotherOwner()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Another Owner Property",
                    Description = "Property owned by another user",
                    Price = 1500000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 2,
                    Bathrooms = 2,
                    Area = 120,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var updateData = new
            {
                title = "Unauthorized Update",
                description = "This is a valid description for the integration test.",
                price = 999999,
                address = "Alexandria",
                neighborhood = "Smouha",
                bedrooms = 2,
                bathrooms = 1,
                area = 80
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/{propertyId}",
                content);

            // Assert
            var responseBody =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"Status: {response.StatusCode}\nResponse: {responseBody}");

            Assert.Equal(
               System.Net.HttpStatusCode.Forbidden,
               response.StatusCode);
        }
        [Fact]
        public async Task Owner_ShouldUpdateOwnProperty_AndSetStatusToPending()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Original Property",
                    Description = "Original property description",
                    Price = 1500000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var updateData = new
            {
                title = "Updated Property",
                description = "This is the updated property description.",
                price = 1700000,
                address = "Alexandria",
                neighborhood = "Smouha",
                bedrooms = 3,
                bathrooms = 2,
                area = 160
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/{propertyId}",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var updatedProperty = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(updatedProperty);

                Assert.Equal(
                    "Updated Property",
                    updatedProperty.Title);

                Assert.Equal(
                    1700000,
                    updatedProperty.Price);

                Assert.Equal(
                    PropertyStatus.Pending,
                    updatedProperty.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldMarkOwnApprovedPropertyAsSold()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Apartment For Sale",
                    Description = "A beautiful approved apartment in Alexandria.",
                    Price = 2000000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 160,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/{propertyId}/sold",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var soldProperty = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(soldProperty);

                Assert.Equal(
                    PropertyStatus.Sold,
                    soldProperty.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotMarkAnotherOwnersPropertyAsSold()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Another Owner Apartment",
                    Description = "A beautiful approved apartment in Alexandria.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 140,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/{propertyId}/sold",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property was NOT marked as sold
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Approved,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotMarkPendingPropertyAsSold()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Pending Apartment",
                    Description = "A beautiful pending apartment in Alexandria.",
                    Price = 1600000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/{propertyId}/sold",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the property is still Pending
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldDeleteOwnProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property To Delete",
                    Description = "A property that will be deleted.",
                    Price = 1500000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            // Verify that the property was deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var deletedProperty = await context.Properties
                    .FindAsync(propertyId);

                Assert.Null(deletedProperty);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotDeleteAnotherOwnersProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Another Owner Property",
                    Description = "A property owned by another owner.",
                    Price = 1700000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 140,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property still exists
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    300,
                    property.OwnerId);
            }
        }
        [Fact]
        public async Task Admin_ShouldDeleteAnyProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Owner Property For Admin Delete",
                    Description = "A property that should be deletable by admin.",
                    Price = 1900000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 155,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Admin JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "999"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            // Verify that the property was deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var deletedProperty = await context.Properties
                    .FindAsync(propertyId);

                Assert.Null(deletedProperty);
            }
        }
        [Fact]
        public async Task Buyer_ShouldOnlySeeOwnFavorites()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Favorite Test Apartment",
                    Description = "A beautiful approved apartment for favorite testing.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                context.Favorites.Add(new Favorite
                {
                    BuyerId = 500,
                    PropertyId = propertyId
                });

                await context.SaveChangesAsync();
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.GetAsync(
                "/api/Properties/my-favorites");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            Assert.DoesNotContain(
                propertyId.ToString(),
                responseContent);
        }
        [Fact]
        public async Task Buyer_ShouldNotAddSamePropertyToFavoritesTwice()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Favorite Apartment",
                    Description = "A beautiful approved apartment for favorite testing.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act - First attempt
            var firstResponse = await _client.PostAsync(
                $"/api/Properties/{propertyId}/favorite",
                null);

            // Act - Second attempt
            var secondResponse = await _client.PostAsync(
                $"/api/Properties/{propertyId}/favorite",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                firstResponse.StatusCode);

            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                secondResponse.StatusCode);

            // Verify that only one favorite exists
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var favoritesCount = await context.Favorites
                    .CountAsync(f =>
                        f.BuyerId == 600 &&
                        f.PropertyId == propertyId);

                Assert.Equal(1, favoritesCount);
            }
        }
        [Fact]
        public async Task Buyer_ShouldRemoveOwnFavorite()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Favorite To Remove",
                    Description = "A beautiful approved apartment for favorite testing.",
                    Price = 1750000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                context.Favorites.Add(new Favorite
                {
                    BuyerId = 600,
                    PropertyId = propertyId
                });

                await context.SaveChangesAsync();
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}/favorite");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            // Verify that the favorite was removed
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var favorite = await context.Favorites
                    .FirstOrDefaultAsync(f =>
                        f.BuyerId == 600 &&
                        f.PropertyId == propertyId);

                Assert.Null(favorite);
            }
        }
        [Fact]
        public async Task Buyer_ShouldNotRemoveAnotherBuyersFavorite()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Another Buyers Favorite",
                    Description = "A beautiful approved apartment for favorite testing.",
                    Price = 1750000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                // Favorite belongs to Buyer 500
                context.Favorites.Add(new Favorite
                {
                    BuyerId = 500,
                    PropertyId = propertyId
                });

                await context.SaveChangesAsync();
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}/favorite");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.NotFound,
                response.StatusCode);

            // Verify that Buyer 500's favorite still exists
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var favorite = await context.Favorites
                    .FirstOrDefaultAsync(f =>
                        f.BuyerId == 500 &&
                        f.PropertyId == propertyId);

                Assert.NotNull(favorite);
            }
        }
        [Fact]
        public async Task Owner_ShouldAddPropertyImage()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Apartment With Image",
                    Description = "A beautiful approved apartment with image.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var imageData = new
            {
                imageUrl = "https://example.com/apartment-image.jpg"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(imageData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                $"/api/Properties/{propertyId}/images",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            // Verify that the image was saved
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var image = await context.PropertyImages
                    .FirstOrDefaultAsync(i =>
                        i.PropertyId == propertyId &&
                        i.ImageUrl ==
                        "https://example.com/apartment-image.jpg");

                Assert.NotNull(image);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotAddImageToAnotherOwnersProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Another Owners Property",
                    Description = "A property owned by another owner.",
                    Price = 1900000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var imageData = new
            {
                imageUrl = "https://example.com/unauthorized-image.jpg"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(imageData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                $"/api/Properties/{propertyId}/images",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that no image was added
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var image = await context.PropertyImages
                    .FirstOrDefaultAsync(i =>
                        i.PropertyId == propertyId &&
                        i.ImageUrl ==
                        "https://example.com/unauthorized-image.jpg");

                Assert.Null(image);
            }
        }
        [Fact]
        public async Task Owner_ShouldAddTenthImage_AndSetApprovedPropertyToPending()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property With Ten Images",
                    Description = "A beautiful approved apartment with images.",
                    Price = 2000000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 160,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                // Add 9 existing images
                for (int i = 1; i <= 9; i++)
                {
                    context.PropertyImages.Add(new PropertyImage
                    {
                        ImageUrl = $"https://example.com/image-{i}.jpg",
                        PropertyId = propertyId
                    });
                }

                await context.SaveChangesAsync();
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var imageData = new
            {
                imageUrl = "https://example.com/image-10.jpg"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(imageData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                $"/api/Properties/{propertyId}/images",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var imagesCount = await context.PropertyImages
                    .CountAsync(i => i.PropertyId == propertyId);

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.Equal(10, imagesCount);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotAddMoreThanTenImages()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property With Maximum Images",
                    Description = "A property that already has ten images.",
                    Price = 2100000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 160,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                // Add 10 existing images
                for (int i = 1; i <= 10; i++)
                {
                    context.PropertyImages.Add(new PropertyImage
                    {
                        ImageUrl = $"https://example.com/image-{i}.jpg",
                        PropertyId = propertyId
                    });
                }

                await context.SaveChangesAsync();
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var imageData = new
            {
                imageUrl = "https://example.com/image-11.jpg"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(imageData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                $"/api/Properties/{propertyId}/images",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the image count is still 10
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var imagesCount = await context.PropertyImages
                    .CountAsync(i => i.PropertyId == propertyId);

                Assert.Equal(10, imagesCount);
            }
        }
        [Fact]
        public async Task Owner_ShouldDeleteOwnPropertyImage()
        {
            // Arrange
            int propertyId;
            int imageId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property With Image To Delete",
                    Description = "A property with an image that will be deleted.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                var image = new PropertyImage
                {
                    ImageUrl = "https://example.com/image-to-delete.jpg",
                    PropertyId = propertyId
                };

                context.PropertyImages.Add(image);
                await context.SaveChangesAsync();

                imageId = image.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}/images/{imageId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            // Verify that the image was deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var deletedImage = await context.PropertyImages
                    .FindAsync(imageId);

                Assert.Null(deletedImage);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotDeleteAnotherOwnersPropertyImage()
        {
            // Arrange
            int propertyId;
            int imageId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Another Owners Property With Image",
                    Description = "A property owned by another owner.",
                    Price = 1900000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 300
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                var image = new PropertyImage
                {
                    ImageUrl = "https://example.com/protected-image.jpg",
                    PropertyId = propertyId
                };

                context.PropertyImages.Add(image);
                await context.SaveChangesAsync();

                imageId = image.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}/images/{imageId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the image still exists
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var image = await context.PropertyImages
                    .FindAsync(imageId);

                Assert.NotNull(image);

                Assert.Equal(
                    propertyId,
                    image.PropertyId);
            }
        }
        [Fact]
        public async Task Buyer_ShouldNotCreateProperty()
        {
            // Arrange
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            var propertyData = new
            {
                title = "Buyer Property",
                description = "This property should not be created by a buyer.",
                price = 1500000,
                address = "Alexandria",
                neighborhood = "Smouha",
                bedrooms = 3,
                bathrooms = 2,
                area = 150
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(propertyData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(
                "/api/Properties",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);
        }
        [Fact]
        public async Task Buyer_ShouldNotApproveProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Pending Property",
                    Description = "A pending property that requires admin approval.",
                    Price = 1600000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/approve/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property is still Pending
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotApproveProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Owner Pending Property",
                    Description = "A pending property owned by the current owner.",
                    Price = 1700000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/approve/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property is still Pending
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Buyer_ShouldNotRejectProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Pending Property For Rejection Test",
                    Description = "A pending property that should not be rejected by a buyer.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 160,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/reject/{propertyId}",
                new StringContent(
                    "\"Invalid property\"",
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property is still Pending
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotRejectProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Owner Pending Property",
                    Description = "A pending property for rejection authorization test.",
                    Price = 1900000,
                    Address = "Alexandria",
                    Neighborhood = "Gleem",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 170,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/reject/{propertyId}",
                new StringContent(
                    "\"Invalid property\"",
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property is still Pending
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Buyer_ShouldNotDeleteProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property For Buyer Delete Test",
                    Description = "A property that a buyer should not be able to delete.",
                    Price = 1500000,
                    Address = "Alexandria",
                    Neighborhood = "Stanley",
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Area = 120,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the property still exists
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);
            }
        }
        [Fact]
        public async Task Buyer_ShouldNotAddPropertyImage()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property For Buyer Image Test",
                    Description = "A property that a buyer should not modify.",
                    Price = 1600000,
                    Address = "Alexandria",
                    Neighborhood = "Sidi Gaber",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var content = new StringContent(
                """
        {
            "imageUrl": "https://example.com/property.jpg"
        }
        """,
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync(
                $"/api/Properties/{propertyId}/images",
                content);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that no image was added
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var imageCount = await context.PropertyImages
                    .CountAsync(i => i.PropertyId == propertyId);

                Assert.Equal(0, imageCount);
            }
        }
        [Fact]
        public async Task Buyer_ShouldNotDeletePropertyImage()
        {
            // Arrange
            int propertyId;
            int imageId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property For Buyer Image Delete Test",
                    Description = "A property that a buyer should not modify.",
                    Price = 1650000,
                    Address = "Alexandria",
                    Neighborhood = "Stanley",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;

                var image = new PropertyImage
                {
                    ImageUrl = "https://example.com/property.jpg",
                    PropertyId = propertyId
                };

                context.PropertyImages.Add(image);
                await context.SaveChangesAsync();

                imageId = image.Id;
            }

            // Buyer 600 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "600"),
        new Claim(ClaimTypes.Email, "buyer600@test.com"),
        new Claim(ClaimTypes.Role, "Buyer")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.DeleteAsync(
                $"/api/Properties/{propertyId}/images/{imageId}");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that the image still exists
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var image = await context.PropertyImages
                    .FindAsync(imageId);

                Assert.NotNull(image);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotResubmitApprovedProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Approved Property",
                    Description = "An approved property that should not be resubmitted.",
                    Price = 2000000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 160,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/resubmit/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the property is still Approved
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Approved,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotResubmitPendingProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Pending Property",
                    Description = "A pending property that should not be resubmitted.",
                    Price = 2100000,
                    Address = "Alexandria",
                    Neighborhood = "Stanley",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 170,
                    Status = PropertyStatus.Pending,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/resubmit/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the property is still Pending
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Pending,
                    property.Status);
            }
        }
        [Fact]
        public async Task Owner_ShouldNotMarkRejectedPropertyAsSold()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Rejected Property",
                    Description = "A rejected property that cannot be sold.",
                    Price = 1750000,
                    Address = "Alexandria",
                    Neighborhood = "Miami",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Rejected,
                    RejectionReason = "Missing required information.",
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Owner 200 JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "200"),
        new Claim(ClaimTypes.Email, "owner@test.com"),
        new Claim(ClaimTypes.Role, "Owner")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/{propertyId}/sold",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the property is still Rejected
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Rejected,
                    property.Status);
            }
        }
        [Fact]
        public async Task Admin_ShouldNotApproveSoldProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Sold Property",
                    Description = "A sold property that should not be approved again.",
                    Price = 2200000,
                    Address = "Alexandria",
                    Neighborhood = "Smouha",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 180,
                    Status = PropertyStatus.Sold,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Admin JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires : DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/approve/{propertyId}",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the property is still Sold
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Sold,
                    property.Status);
            }
        }
        [Fact]
        public async Task Admin_ShouldNotRejectSoldProperty()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Sold Property For Rejection Test",
                    Description = "A sold property that should not be rejected.",
                    Price = 2300000,
                    Address = "Alexandria",
                    Neighborhood = "Gleem",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 180,
                    Status = PropertyStatus.Sold,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Admin JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PutAsync(
                $"/api/Properties/reject/{propertyId}",
                new StringContent(
                    "\"Sold property cannot be rejected.\"",
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.BadRequest,
                response.StatusCode);

            // Verify that the property is still Sold
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = await context.Properties
                    .FindAsync(propertyId);

                Assert.NotNull(property);

                Assert.Equal(
                    PropertyStatus.Sold,
                    property.Status);

                Assert.Null(property.RejectionReason);
            }
        }
        [Fact]
        public async Task Admin_ShouldNotAddFavorite()
        {
            // Arrange
            int propertyId;

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var property = new Property
                {
                    Title = "Property For Admin Favorite Test",
                    Description = "An approved property for authorization testing.",
                    Price = 1800000,
                    Address = "Alexandria",
                    Neighborhood = "Stanley",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 150,
                    Status = PropertyStatus.Approved,
                    OwnerId = 200
                };

                context.Properties.Add(property);
                await context.SaveChangesAsync();

                propertyId = property.Id;
            }

            // Admin JWT
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Email, "admin@test.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "IntegrationTestSecretKey12345678901234567890"));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);

            // Act
            var response = await _client.PostAsync(
                $"/api/Properties/{propertyId}/favorite",
                null);

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.Forbidden,
                response.StatusCode);

            // Verify that no favorite was created
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var favoriteExists = await context.Favorites
                    .AnyAsync(f =>
                        f.PropertyId == propertyId &&
                        f.BuyerId == 1);

                Assert.False(favoriteExists);
            }
        }
    }
}
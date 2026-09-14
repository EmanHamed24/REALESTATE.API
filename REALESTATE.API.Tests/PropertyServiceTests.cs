using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.Data;
using REALESTATE_.API.DTos;
using REALESTATE_.API.Services;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Tests
{
    public class PropertyServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(
            Guid.NewGuid().ToString())
            .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task UpdatePropertyAsync_ShouldUpdate_WhenOwnerOwnsProperty()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Old Apartment",
                Description = "Old description",
                Price = 1000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 2,
                Bathrooms = 1,
                Area = 120,
                Status = PropertyStatus.Approved,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            var updateDto = new UpdatePropertyDto
            {
                Title = "Updated Apartment",
                Description = "Updated description",
                Price = 1200000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150
            };

            var result = await service.UpdatePropertyAsync(
                1,
                updateDto,
                5);

            Assert.True(result);

            var property = await context.Properties
                .FirstAsync(p => p.Id == 1);

            Assert.Equal("Updated Apartment", property.Title);
            Assert.Equal("Updated description", property.Description);
            Assert.Equal(1200000, property.Price);
            Assert.Equal(3, property.Bedrooms);
            Assert.Equal(2, property.Bathrooms);
            Assert.Equal(150, property.Area);

            // Updating an approved property sends it back for admin review.
            Assert.Equal(
                PropertyStatus.Pending,
                property.Status);
        }

        [Fact]
        public async Task UpdatePropertyAsync_ShouldThrow_WhenOwnerDoesNotOwnProperty()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Test Apartment",
                Description = "Test apartment description",
                Price = 1000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 2,
                Bathrooms = 1,
                Area = 120,
                Status = PropertyStatus.Approved,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            var updateDto = new UpdatePropertyDto
            {
                Title = "Updated Apartment",
                Description = "Updated apartment description",
                Price = 1200000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.UpdatePropertyAsync(
                    1,
                    updateDto,
                    10));
        }

        [Fact]
        public async Task UpdatePropertyAsync_ShouldThrow_WhenPropertyIsPending()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Pending Apartment",
                Description = "Pending apartment description",
                Price = 1000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 2,
                Bathrooms = 1,
                Area = 120,
                Status = PropertyStatus.Pending,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            var updateDto = new UpdatePropertyDto
            {
                Title = "Updated Apartment",
                Description = "Updated apartment description",
                Price = 1200000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdatePropertyAsync(
                    1,
                    updateDto,
                    5));

            Assert.Equal(
                "Pending Properties cannot be edited.",
                exception.Message);
        }

        [Fact]
        public async Task UpdatePropertyAsync_ShouldReturnFalse_WhenPropertyDoesNotExist()
        {
            using var context = CreateContext();

            var service = new PropertyService(context);

            var updateDto = new UpdatePropertyDto
            {
                Title = "Updated Apartment",
                Description = "Updated description",
                Price = 1200000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150
            };

            var result = await service.UpdatePropertyAsync(
                999,
                updateDto,
                5);

            Assert.False(result);
        }

        [Fact]
        public async Task MarkPropertyAsSoldAsync_ShouldMarkApprovedPropertyAsSold()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Approved Apartment",
                Description = "Approved apartment description",
                Price = 2000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150,
                Status = PropertyStatus.Approved,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            var result = await service.MarkPropertyAsSoldAsync(
                1,
                5);

            Assert.True(result);

            var property = await context.Properties
                .FirstAsync(p => p.Id == 1);

            Assert.Equal(
                PropertyStatus.Sold,
                property.Status);
        }

        [Fact]
        public async Task MarkPropertyAsSoldAsync_ShouldThrow_WhenPropertyIsNotApproved()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Pending Apartment",
                Description = "Pending apartment description",
                Price = 1000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 2,
                Bathrooms = 1,
                Area = 120,
                Status = PropertyStatus.Pending,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.MarkPropertyAsSoldAsync(
                    1,
                    5));

            Assert.Equal(
                "Only approved properties can be marked as sold.",
                exception.Message);
        }

        [Fact]
        public async Task MarkPropertyAsSoldAsync_ShouldThrow_WhenOwnerDoesNotOwnProperty()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Approved Apartment",
                Description = "Approved apartment description",
                Price = 2000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150,
                Status = PropertyStatus.Approved,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.MarkPropertyAsSoldAsync(
                    1,
                    10));
        }

        [Fact]
        public async Task MarkPropertyAsSoldAsync_ShouldReturnFalse_WhenPropertyDoesNotExist()
        {
            using var context = CreateContext();

            var service = new PropertyService(context);

            var result = await service.MarkPropertyAsSoldAsync(
                999,
                5);

            Assert.False(result);
        }
    }

}

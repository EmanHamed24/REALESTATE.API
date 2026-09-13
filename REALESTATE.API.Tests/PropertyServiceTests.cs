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
        public async Task MarkPropertyAsSoldAsync_ShouldThrow_WhenPropertyIsNotApproved()
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
                Status = PropertyStatus.Pending,
                OwnerId = 5
            });

            await context.SaveChangesAsync();

            var service = new PropertyService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.MarkPropertyAsSoldAsync(
                    1,
                    5));
        }
    }
}

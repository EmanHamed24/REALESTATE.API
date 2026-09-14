using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.Data;
using REALESTATE_.API.Services;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Tests
{
    public class FavoriteServiceTests
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
        public async Task AddFavoriteAsync_ShouldAddFavorite()
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
                OwnerId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            await service.AddFavoriteAsync(1, 10);

            var favorite = await context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.BuyerId == 10 &&
                    f.PropertyId == 1);

            Assert.NotNull(favorite);
            Assert.Equal(10, favorite.BuyerId);
            Assert.Equal(1, favorite.PropertyId);
        }

        [Fact]
        public async Task AddFavoriteAsync_ShouldThrow_WhenFavoriteAlreadyExists()
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
                OwnerId = 1
            });

            context.Favorites.Add(new Favorite
            {
                Id = 1,
                BuyerId = 10,
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddFavoriteAsync(1, 10));

            Assert.Equal(
                "Property is already in favorites.",
                exception.Message);
        }

        [Fact]
        public async Task AddFavoriteAsync_ShouldThrow_WhenPropertyIsNotApproved()
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

            var service = new FavoriteService(context);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.AddFavoriteAsync(1, 10));

            Assert.Equal(
                "Approved property not found.",
                exception.Message);
        }

        [Fact]
        public async Task GetMyFavoritesAsync_ShouldReturnBuyerFavorites()
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
                OwnerId = 1
            });

            context.Favorites.Add(new Favorite
            {
                Id = 1,
                BuyerId = 10,
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            var result = await service.GetMyFavoritesAsync(10);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Approved Apartment", result[0].Title);
        }

        [Fact]
        public async Task GetMyFavoritesAsync_ShouldNotReturnAnotherBuyerFavorites()
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
                OwnerId = 1
            });

            context.Favorites.Add(new Favorite
            {
                Id = 1,
                BuyerId = 20,
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            var result = await service.GetMyFavoritesAsync(10);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMyFavoritesAsync_ShouldNotReturnPendingProperty()
        {
            using var context = CreateContext();

            context.Properties.Add(new Property
            {
                Id = 1,
                Title = "Pending Apartment",
                Description = "Pending apartment description",
                Price = 2000000,
                Address = "Alexandria",
                Neighborhood = "Smouha",
                Bedrooms = 3,
                Bathrooms = 2,
                Area = 150,
                Status = PropertyStatus.Pending,
                OwnerId = 1
            });

            context.Favorites.Add(new Favorite
            {
                Id = 1,
                BuyerId = 10,
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            var result = await service.GetMyFavoritesAsync(10);

            Assert.Empty(result);
        }

        [Fact]
        public async Task RemoveFavoriteAsync_ShouldRemoveFavorite()
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
                OwnerId = 1
            });

            context.Favorites.Add(new Favorite
            {
                Id = 1,
                BuyerId = 10,
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            var result = await service.RemoveFavoriteAsync(1, 10);

            Assert.True(result);

            var favorite = await context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.BuyerId == 10 &&
                    f.PropertyId == 1);

            Assert.Null(favorite);
        }

        [Fact]
        public async Task RemoveFavoriteAsync_ShouldThrow_WhenFavoriteDoesNotExist()
        {
            using var context = CreateContext();

            var service = new FavoriteService(context);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.RemoveFavoriteAsync(1, 10));

            Assert.Equal(
                "Property is not in your favorites.",
                exception.Message);
        }
    }
}
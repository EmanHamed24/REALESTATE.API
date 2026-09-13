using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.Data;
using REALESTATE_.API.DTos;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AppDbContext _context;

        public FavoriteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> AddFavoriteAsync(
            int propertyId,
            int buyerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId &&
                    p.Status == PropertyStatus.Approved);

            if (property == null)
            {
                throw new KeyNotFoundException(
                    "Approved property not found.");
            }

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.BuyerId == buyerId &&
                    f.PropertyId == propertyId);

            if (existingFavorite != null)
            {
                throw new InvalidOperationException(
                    "Property is already in favorites.");
            }

            var favorite = new Favorite
            {
                BuyerId = buyerId,
                PropertyId = propertyId
            };

            _context.Favorites.Add(favorite);

            await _context.SaveChangesAsync();

            return new
            {
                message = "Property added to favorites.",
                favoriteId = favorite.Id
            };
        }

        public async Task<List<PropertyDto>> GetMyFavoritesAsync(
            int buyerId)
        {
            return await _context.Favorites
                .Where(f =>
                    f.BuyerId == buyerId &&
                    f.Property != null &&
                    f.Property.Status == PropertyStatus.Approved)
                .Select(f => new PropertyDto
                {
                    Id = f.Property!.Id,
                    Title = f.Property.Title,
                    Description = f.Property.Description,
                    Price = f.Property.Price,
                    Address = f.Property.Address,
                    Neighborhood = f.Property.Neighborhood,
                    Bedrooms = f.Property.Bedrooms,
                    Bathrooms = f.Property.Bathrooms,
                    Area = f.Property.Area,
                    Status = f.Property.Status,
                    OwnerId = f.Property.OwnerId,

                    Images = f.Property.Images
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> RemoveFavoriteAsync(
            int propertyId,
            int buyerId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.BuyerId == buyerId &&
                    f.PropertyId == propertyId);

            if (favorite == null)
            {
                throw new KeyNotFoundException(
                    "Property is not in your favorites.");
            }

            _context.Favorites.Remove(favorite);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}

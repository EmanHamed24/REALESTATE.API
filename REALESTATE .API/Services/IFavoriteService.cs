using REALESTATE_.API.DTos;

namespace REALESTATE_.API.Services
{
    public interface IFavoriteService
    {
        Task<object> AddFavoriteAsync(
            int propertyId,
            int buyerId);

        Task<List<PropertyDto>> GetMyFavoritesAsync(
            int buyerId);

        Task<bool> RemoveFavoriteAsync(
            int propertyId,
            int buyerId);
    }
}

using REALESTATE_.API.DTos;

namespace REALESTATE_.API.Services
{
    public interface IPropertyImageService
    {
        Task<object> AddPropertyImageAsync(
            int propertyId,
            int ownerId,
            CreatePropertyImageDto dto);

        Task<object> UploadPropertyImageAsync(
            int propertyId,
            int ownerId,
            UploadPropertyImageDto dto);

        Task<List<object>> GetPropertyImagesAsync(
            int propertyId);

        Task<bool> DeletePropertyImageAsync(
            int propertyId,
            int imageId,
            int ownerId);
    }
}

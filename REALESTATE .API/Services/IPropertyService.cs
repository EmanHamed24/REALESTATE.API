using REALESTATE_.API.DTos;
using REALESTATE_.API.Services;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Services
{
    public interface IPropertyService
    {
        Task<PropertyDto?> GetPropertyByIdAsync(int id);

        Task<PageResultDto<PropertyDto>> GetPropertiesAsync(
            int pageNumber,
            int pageSize,
            string? neighborhood,
            decimal? minPrice,
            decimal? maxPrice,
            int? bedrooms,
            int? bathrooms,
            double? minArea,
            double? maxArea,
            string? sortBy);

        Task<List<PropertyDto>> GetMyPropertiesAsync(int ownerId);

        Task<PageResultDto<PropertyDto>> GetPendingPropertiesAsync(
           int pageNumber,
           int pageSize,
           string? neighborhood,
           decimal? minPrice,
           decimal? maxPrice,
           int? bedrooms,
           int? bathrooms,
           double? minArea,
           double? maxArea,
           string? sortBy);

        Task<PageResultDto<PropertyDto>> GetAdminPropertiesAsync(
            int pageNumber,
            int pageSize,
            PropertyStatus? status,
            string? neighborhood,
            decimal? minPrice,
            decimal? maxPrice,
            int? bedrooms,
            int? bathrooms,
            double? minArea,
            double? maxArea,
            string? sortBy);

        Task<object> GetAdminStatisticsAsync();

        Task<PropertyDto> CreatePropertyAsync(
          CreatePropertyDto propertyDto, int ownerId);

        Task<bool> UpdatePropertyAsync(int id, UpdatePropertyDto propertyDto, int ownerId);

        Task<bool> MarkPropertyAsSoldAsync(int id, int ownerId);

        Task<bool> DeletePropertyAsync(
            int id,
            int userId,
            string userRole);

        Task<bool> ResubmitPropertyAsync(
              int id,
              int ownerId);

        Task<bool> ApprovePropertyAsync(int id);

        Task<bool> RejectPropertyAsync(
             int id,
             string rejectionReason);
    }
}
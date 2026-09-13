using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REALESTATE_.API.DTos;
using REALESTATE_.API.models;
using REALESTATE_.API.Services;
using System.Security.Claims;

namespace REALESTATE_.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        
        private readonly IPropertyService _propertyService;
        private readonly IPropertyImageService _propertyImageService;
        private readonly IFavoriteService _favoriteService;

        public PropertiesController(
           IPropertyService propertyService,
           IPropertyImageService propertyImageService,
           IFavoriteService favoriteService)
        {
            _propertyService = propertyService;
            _propertyImageService = propertyImageService;
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<ActionResult<PageResultDto<PropertyDto>>> GetProperties(
    int pageNumber = 1,
    int pageSize = 10,
    string? neighborhood = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    int? bedrooms = null,
    int? bathrooms = null,
    double? minArea = null,
    double? maxArea = null,
    string? sortBy = null)
        {
            // Validate pagination
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 50)
                return BadRequest("Page size must be between 1 and 50.");

            // Validate price filters
            if (minPrice.HasValue && minPrice.Value < 0)
                return BadRequest("Minimum price cannot be negative.");

            if (maxPrice.HasValue && maxPrice.Value < 0)
                return BadRequest("Maximum price cannot be negative.");

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                return BadRequest(
                    "Minimum price cannot be greater than Maximum price.");
            }

            // Validate room filters
            if (bedrooms.HasValue && bedrooms.Value < 0)
                return BadRequest("Bedrooms cannot be negative.");

            if (bathrooms.HasValue && bathrooms.Value < 0)
                return BadRequest("Bathrooms cannot be negative.");

            // Validate area filters
            if (minArea.HasValue && minArea.Value < 0)
                return BadRequest("Minimum Area cannot be negative.");

            if (maxArea.HasValue && maxArea.Value < 0)
                return BadRequest("Maximum Area cannot be negative.");

            if (minArea.HasValue &&
                maxArea.HasValue &&
                minArea.Value > maxArea.Value)
            {
                return BadRequest(
                    "Minimum Area cannot be greater than Maximum Area.");
            }

            // Validate sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var allowedSortOptions = new[]
                {
            "price_asc",
            "price_desc",
            "area_asc",
            "area_desc",
            "newest"
        };

                if (!allowedSortOptions.Contains(sortBy.ToLower()))
                {
                    return BadRequest(
                        "Invalid sort option. Allowed values: price_asc, price_desc, area_asc, area_desc, newest.");
                }
            }

            var result = await _propertyService.GetPropertiesAsync(
                pageNumber,
                pageSize,
                neighborhood,
                minPrice,
                maxPrice,
                bedrooms,
                bathrooms,
                minArea,
                maxArea,
                sortBy);

            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<PropertyDto>> GetProperty(int id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            return Ok(property);
        }
        [Authorize(Roles = "Owner")]
        [HttpGet("my-properties")]
        public async Task<ActionResult<List<PropertyDto>>> GetMyProperties()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(ownerIdClaim, out int ownerId))
            {
                return Unauthorized("Invalid owner ID.");
            }

            var properties = await _propertyService.GetMyPropertiesAsync(ownerId);

            return Ok(properties);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<ActionResult<PageResultDto<PropertyDto>>> GetPendingProperties(
             int pageNumber = 1,
             int pageSize = 10,
             string? neighborhood = null,
             decimal? minPrice = null,
             decimal? maxPrice = null,
             int? bedrooms = null,
             int? bathrooms = null,
             double? minArea = null,
             double? maxArea = null,
             string? sortBy = null)
        {
            // Validate pagination
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 50)
                return BadRequest("Page size must be between 1 and 50.");

            // Validate price filters
            if (minPrice.HasValue && minPrice.Value < 0)
                return BadRequest("Minimum price cannot be negative.");

            if (maxPrice.HasValue && maxPrice.Value < 0)
                return BadRequest("Maximum price cannot be negative.");

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                return BadRequest(
                    "Minimum price cannot be greater than Maximum price.");
            }

            // Validate room filters
            if (bedrooms.HasValue && bedrooms.Value < 0)
                return BadRequest("Bedrooms cannot be negative.");

            if (bathrooms.HasValue && bathrooms.Value < 0)
                return BadRequest("Bathrooms cannot be negative.");

            // Validate area filters
            if (minArea.HasValue && minArea.Value < 0)
                return BadRequest("Minimum Area cannot be negative.");

            if (maxArea.HasValue && maxArea.Value < 0)
                return BadRequest("Maximum Area cannot be negative.");

            if (minArea.HasValue &&
                maxArea.HasValue &&
                minArea.Value > maxArea.Value)
            {
                return BadRequest(
                    "Minimum Area cannot be greater than Maximum Area.");
            }

            // Validate sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var allowedSortOptions = new[]
                {
            "price_asc",
            "price_desc",
            "area_asc",
            "area_desc",
            "newest"
        };

                if (!allowedSortOptions.Contains(sortBy.ToLower()))
                {
                    return BadRequest(
                        "Invalid sort option. Allowed values: price_asc, price_desc, area_asc, area_desc, newest.");
                }
            }

            var result = await _propertyService.GetPendingPropertiesAsync(
                pageNumber,
                pageSize,
                neighborhood,
                minPrice,
                maxPrice,
                bedrooms,
                bathrooms,
                minArea,
                maxArea,
                sortBy);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/properties")]
        public async Task<ActionResult<PageResultDto<PropertyDto>>> GetAdminProperties(
    int pageNumber = 1,
    int pageSize = 10,
    PropertyStatus? status = null,
    string? neighborhood = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    int? bedrooms = null,
    int? bathrooms = null,
    double? minArea = null,
    double? maxArea = null,
    string? sortBy = null)
        {
            // Validate pagination
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 50)
                return BadRequest("Page size must be between 1 and 50.");

            // Validate price filters
            if (minPrice.HasValue && minPrice.Value < 0)
                return BadRequest("Minimum price cannot be negative.");

            if (maxPrice.HasValue && maxPrice.Value < 0)
                return BadRequest("Maximum price cannot be negative.");

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                return BadRequest(
                    "Minimum price cannot be greater than Maximum price.");
            }

            // Validate room filters
            if (bedrooms.HasValue && bedrooms.Value < 0)
                return BadRequest("Bedrooms cannot be negative.");

            if (bathrooms.HasValue && bathrooms.Value < 0)
                return BadRequest("Bathrooms cannot be negative.");

            // Validate area filters
            if (minArea.HasValue && minArea.Value < 0)
                return BadRequest("Minimum Area cannot be negative.");

            if (maxArea.HasValue && maxArea.Value < 0)
                return BadRequest("Maximum Area cannot be negative.");

            if (minArea.HasValue &&
                maxArea.HasValue &&
                minArea.Value > maxArea.Value)
            {
                return BadRequest(
                    "Minimum Area cannot be greater than Maximum Area.");
            }

            // Validate sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var allowedSortOptions = new[]
                {
            "price_asc",
            "price_desc",
            "area_asc",
            "area_desc",
            "oldest",
            "newest"
        };

                if (!allowedSortOptions.Contains(sortBy.ToLower()))
                {
                    return BadRequest(
                        "Invalid sort option. Allowed values: price_asc, price_desc, area_asc, area_desc, oldest, newest.");
                }
            }

            var result = await _propertyService.GetAdminPropertiesAsync(
                pageNumber,
                pageSize,
                status,
                neighborhood,
                minPrice,
                maxPrice,
                bedrooms,
                bathrooms,
                minArea,
                maxArea,
                sortBy);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-statistics")]
        public async Task<ActionResult<object>> GetAdminStatistics()
        {
            var statistics = await _propertyService.GetAdminStatisticsAsync();

            return Ok(statistics);
        }
        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<ActionResult<PropertyDto>> CreateProperty(
            CreatePropertyDto propertyDto)
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (ownerId == null || !int.TryParse(ownerId, out int currentOwnerId))
            {
                return Unauthorized();
            }

            var property = await _propertyService.CreatePropertyAsync(
                propertyDto,
                currentOwnerId);

            return Ok(property);
        }
        [Authorize(Roles = "Owner")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProperty(
            int id,
            UpdatePropertyDto propertyDto)
        {
            var ownerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null ||
                !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var updated = await _propertyService
                .UpdatePropertyAsync(
                    id,
                    propertyDto,
                    ownerId);

            if (!updated)
            {
                return NotFound("Property not found.");
            }

            return Ok(new
            {
                message = "Property updated successfully and is pending approval."
            });
        }
        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/sold")]
        public async Task<ActionResult> MarkPropertyAsSold(int id)
        {
            var ownerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null ||
                !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var sold = await _propertyService
                .MarkPropertyAsSoldAsync(
                    id,
                    ownerId);

            if (!sold)
            {
                return NotFound("Property not found.");
            }

            return Ok(new
            {
                message = "Property marked as sold successfully."
            });
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProperty(int id)
        {
            var userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var userRole = User.FindFirst(
                ClaimTypes.Role)?.Value;

            if (userIdValue == null ||
                !int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            if (userRole == null)
            {
                return Unauthorized();
            }

            var deleted = await _propertyService
                .DeletePropertyAsync(
                    id,
                    userId,
                    userRole);

            if (!deleted)
            {
                return NotFound("Property not found.");
            }

            return Ok(new
            {
                message = "Property deleted successfully."
            });
        }
        [Authorize(Roles = "Owner")]
        [HttpPut("resubmit/{id}")]
        public async Task<ActionResult> ResubmitProperty(int id)
        {
            var ownerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null ||
                !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var resubmitted = await _propertyService
                .ResubmitPropertyAsync(
                    id,
                    ownerId);

            if (!resubmitted)
            {
                return NotFound("Property not found.");
            }

            return Ok(new
            {
                message = "Property resubmitted successfully and is pending approval."
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id}")]
        public async Task<ActionResult> ApproveProperty(int id)
        {
            var approved = await _propertyService
                .ApprovePropertyAsync(id);

            if (!approved)
            {
                return NotFound("Property not found.");
            }

            return Ok(new
            {
                message = "Property approved successfully."
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("reject/{id}")]
        public async Task<ActionResult> RejectProperty(
             int id,
             [FromBody] string rejectionReason)
        {
            var rejected = await _propertyService
                .RejectPropertyAsync(
                    id,
                    rejectionReason);

            if (!rejected)
            {
                return NotFound("Property not found.");
            }

            return Ok(new
            {
                message = "Property rejected successfully."
            });
        }
        [Authorize(Roles = "Owner")]
        [HttpPost("{id}/images")]
        public async Task<ActionResult> AddPropertyImage(
             int id,
             CreatePropertyImageDto dto)
        {
            var ownerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null ||
                !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var result = await _propertyImageService
                .AddPropertyImageAsync(
                    id,
                    ownerId,
                    dto);

            return Ok(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpPost("{id}/images/upload")]
        public async Task<ActionResult> UploadPropertyImage(
            int id,
            [FromForm] UploadPropertyImageDto dto)
        {
            var ownerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null ||
                !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var result = await _propertyImageService
                .UploadPropertyImageAsync(
                    id,
                    ownerId,
                    dto);

            return Ok(result);
        }
        [HttpGet("{id}/images")]
        public async Task<ActionResult<IEnumerable<object>>> GetPropertyImages(int id)
        {
            var images = await _propertyImageService
                .GetPropertyImagesAsync(id);

            return Ok(images);
        }
        [Authorize(Roles = "Owner")]
        [HttpDelete("{propertyId}/images/{imageId}")]
        public async Task<ActionResult> DeletePropertyImage(
             int propertyId,
             int imageId)
        {
            var ownerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null ||
                !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var deleted = await _propertyImageService
                .DeletePropertyImageAsync(
                    propertyId,
                    imageId,
                    ownerId);

            if (!deleted)
            {
                return NotFound("Image not found.");
            }

            return Ok(new
            {
                message = "Image deleted successfully."
            });
        }
        [Authorize(Roles = "Buyer")]
        [HttpPost("{propertyId}/favorite")]
        public async Task<ActionResult> AddFavorite(int propertyId)
        {
            var buyerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (buyerIdValue == null ||
                !int.TryParse(buyerIdValue, out int buyerId))
            {
                return Unauthorized();
            }

            var result = await _favoriteService
                .AddFavoriteAsync(
                    propertyId,
                    buyerId);

            return Ok(result);
        }
        [Authorize(Roles = "Buyer")]
        [HttpGet("my-favorites")]
        public async Task<ActionResult<IEnumerable<PropertyDto>>> GetMyFavorites()
        {
            var buyerIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (buyerIdValue == null ||
                !int.TryParse(buyerIdValue, out int buyerId))
            {
                return Unauthorized();
            }

            var favorites = await _favoriteService
                .GetMyFavoritesAsync(buyerId);

            return Ok(favorites);
        }
        [Authorize(Roles = "Buyer")]
        [HttpDelete("{propertyId}/favorite")]
        public async Task<ActionResult> RemoveFavorite(int propertyId)
        {
            var buyerIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (buyerIdValue == null ||
                !int.TryParse(buyerIdValue, out int buyerId))
            {
                return Unauthorized();
            }

            var removed = await _favoriteService
                .RemoveFavoriteAsync(
                    propertyId,
                    buyerId);

            if (!removed)
            {
                return NotFound(
                    "Property is not in your favorites.");
            }

            return Ok(new
            {
                message = "Property removed from favorites."
            });
        }
    }

}


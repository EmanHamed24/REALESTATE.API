using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.Data;
using REALESTATE_.API.DTos;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly AppDbContext _context;

        public PropertyService(AppDbContext context)
        {
            _context = context;
        }

        // Get a single approved property by ID
        public async Task<PropertyDto?> GetPropertyByIdAsync(int id)
        {
            return await _context.Properties
                .Where(p =>
                    p.Id == id &&
                    p.Status == PropertyStatus.Approved)
                .Select(p => new PropertyDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Address = p.Address,
                    Neighborhood = p.Neighborhood,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    Area = p.Area,
                    Status = p.Status,
                    OwnerId = p.OwnerId,
                    RejectionReason = p.RejectionReason,

                    Owner = p.Owner == null ? null : new OwnerDto
                    {
                        Id = p.Owner.Id,
                        Name = p.Owner.Name,
                        Email = p.Owner.Email,
                        Role = p.Owner.Role
                    },

                    Images = p.Images
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        // Get approved properties with filtering, sorting and pagination
        public async Task<PageResultDto<PropertyDto>> GetPropertiesAsync(
            int pageNumber,
            int pageSize,
            string? neighborhood,
            decimal? minPrice,
            decimal? maxPrice,
            int? bedrooms,
            int? bathrooms,
            double? minArea,
            double? maxArea,
            string? sortBy)
        {
            var query = _context.Properties
                .Where(p => p.Status == PropertyStatus.Approved);

            if (!string.IsNullOrWhiteSpace(neighborhood))
                query = query.Where(p =>
                    p.Neighborhood.Contains(neighborhood));

            if (minPrice.HasValue)
                query = query.Where(p =>
                    p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p =>
                    p.Price <= maxPrice.Value);

            if (bedrooms.HasValue)
                query = query.Where(p =>
                    p.Bedrooms == bedrooms.Value);

            if (bathrooms.HasValue)
                query = query.Where(p =>
                    p.Bathrooms == bathrooms.Value);

            if (minArea.HasValue)
                query = query.Where(p =>
                    p.Area >= minArea.Value);

            if (maxArea.HasValue)
                query = query.Where(p =>
                    p.Area <= maxArea.Value);

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            var orderedQuery = sortBy?.ToLower() switch
            {
                "price_asc" =>
                    query.OrderBy(p => p.Price),

                "price_desc" =>
                    query.OrderByDescending(p => p.Price),

                "area_asc" =>
                    query.OrderBy(p => p.Area),

                "area_desc" =>
                    query.OrderByDescending(p => p.Area),

                _ =>
                    query.OrderByDescending(p => p.Id)
            };

            var properties = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PropertyDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Address = p.Address,
                    Neighborhood = p.Neighborhood,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    Area = p.Area,
                    Status = p.Status,
                    OwnerId = p.OwnerId,

                    Images = p.Images
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .ToListAsync();

            return new PageResultDto<PropertyDto>
            {
                Items = properties,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        // Get all properties belonging to the logged-in owner
        public async Task<List<PropertyDto>> GetMyPropertiesAsync(int ownerId)
        {
            return await _context.Properties
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.Id)
                .Select(p => new PropertyDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Address = p.Address,
                    Neighborhood = p.Neighborhood,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    Area = p.Area,
                    Status = p.Status,
                    OwnerId = p.OwnerId,
                    RejectionReason = p.RejectionReason,

                    Images = p.Images
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .ToListAsync();
        }

        // Get pending properties for Admin
        public async Task<PageResultDto<PropertyDto>> GetPendingPropertiesAsync(
            int pageNumber,
            int pageSize,
            string? neighborhood,
            decimal? minPrice,
            decimal? maxPrice,
            int? bedrooms,
            int? bathrooms,
            double? minArea,
            double? maxArea,
            string? sortBy)
        {
            var query = _context.Properties
                .Where(p => p.Status == PropertyStatus.Pending);

            if (!string.IsNullOrWhiteSpace(neighborhood))
                query = query.Where(p =>
                    p.Neighborhood.Contains(neighborhood));

            if (minPrice.HasValue)
                query = query.Where(p =>
                    p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p =>
                    p.Price <= maxPrice.Value);

            if (bedrooms.HasValue)
                query = query.Where(p =>
                    p.Bedrooms == bedrooms.Value);

            if (bathrooms.HasValue)
                query = query.Where(p =>
                    p.Bathrooms == bathrooms.Value);

            if (minArea.HasValue)
                query = query.Where(p =>
                    p.Area >= minArea.Value);

            if (maxArea.HasValue)
                query = query.Where(p =>
                    p.Area <= maxArea.Value);

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            var orderedQuery = sortBy?.ToLower() switch
            {
                "price_asc" =>
                    query.OrderBy(p => p.Price),

                "price_desc" =>
                    query.OrderByDescending(p => p.Price),

                "area_asc" =>
                    query.OrderBy(p => p.Area),

                "area_desc" =>
                    query.OrderByDescending(p => p.Area),

                _ =>
                    query.OrderByDescending(p => p.Id)
            };

            var properties = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PropertyDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Address = p.Address,
                    Neighborhood = p.Neighborhood,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    Area = p.Area,
                    Status = p.Status,
                    OwnerId = p.OwnerId,
                    RejectionReason = p.RejectionReason,

                    Owner = p.Owner == null ? null : new OwnerDto
                    {
                        Id = p.Owner.Id,
                        Name = p.Owner.Name,
                        Email = p.Owner.Email,
                        Role = p.Owner.Role
                    },

                    Images = p.Images
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .ToListAsync();

            return new PageResultDto<PropertyDto>
            {
                Items = properties,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        // Get all properties for Admin
        public async Task<PageResultDto<PropertyDto>> GetAdminPropertiesAsync(
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
            string? sortBy)
        {
            var query = _context.Properties.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p =>
                    p.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(neighborhood))
            {
                query = query.Where(p =>
                    p.Neighborhood.Contains(neighborhood));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p =>
                    p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p =>
                    p.Price <= maxPrice.Value);
            }

            if (bedrooms.HasValue)
            {
                query = query.Where(p =>
                    p.Bedrooms == bedrooms.Value);
            }

            if (bathrooms.HasValue)
            {
                query = query.Where(p =>
                    p.Bathrooms == bathrooms.Value);
            }

            if (minArea.HasValue)
            {
                query = query.Where(p =>
                    p.Area >= minArea.Value);
            }

            if (maxArea.HasValue)
            {
                query = query.Where(p =>
                    p.Area <= maxArea.Value);
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            var orderedQuery = sortBy?.ToLower() switch
            {
                "price_asc" =>
                    query.OrderBy(p => p.Price),

                "price_desc" =>
                    query.OrderByDescending(p => p.Price),

                "area_asc" =>
                    query.OrderBy(p => p.Area),

                "area_desc" =>
                    query.OrderByDescending(p => p.Area),

                "oldest" =>
                    query.OrderBy(p => p.Id),

                _ =>
                    query.OrderByDescending(p => p.Id)
            };

            var properties = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PropertyDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Address = p.Address,
                    Neighborhood = p.Neighborhood,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    Area = p.Area,
                    Status = p.Status,
                    OwnerId = p.OwnerId,
                    RejectionReason = p.RejectionReason,

                    Owner = p.Owner == null ? null : new OwnerDto
                    {
                        Id = p.Owner.Id,
                        Name = p.Owner.Name,
                        Email = p.Owner.Email,
                        Role = p.Owner.Role
                    },

                    Images = p.Images
                        .Select(i => i.ImageUrl)
                        .ToList()
                })
                .ToListAsync();

            return new PageResultDto<PropertyDto>
            {
                Items = properties,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        // Get Admin statistics
        public async Task<object> GetAdminStatisticsAsync()
        {
            var totalProperties = await _context.Properties.CountAsync();

            var pendingProperties = await _context.Properties
                .CountAsync(p => p.Status == PropertyStatus.Pending);

            var approvedProperties = await _context.Properties
                .CountAsync(p => p.Status == PropertyStatus.Approved);

            var rejectedProperties = await _context.Properties
                .CountAsync(p => p.Status == PropertyStatus.Rejected);

            var soldProperties = await _context.Properties
                .CountAsync(p => p.Status == PropertyStatus.Sold);

            var totalUsers = await _context.Owners.CountAsync();

            var totalOwners = await _context.Owners
                .CountAsync(o => o.Role == "Owner");

            var totalBuyers = await _context.Owners
                .CountAsync(o => o.Role == "Buyer");

            var totalAdmins = await _context.Owners
                .CountAsync(o => o.Role == "Admin");

            return new
            {
                TotalProperties = totalProperties,
                PendingProperties = pendingProperties,
                ApprovedProperties = approvedProperties,
                RejectedProperties = rejectedProperties,
                SoldProperties = soldProperties,
                TotalUsers = totalUsers,
                TotalOwners = totalOwners,
                TotalBuyers = totalBuyers,
                TotalAdmins = totalAdmins
            };
        }
        public async Task<PropertyDto> CreatePropertyAsync(
    CreatePropertyDto propertyDto,
    int ownerId)
        {
            var property = new Property
            {
                Title = propertyDto.Title,
                Description = propertyDto.Description,
                Price = propertyDto.Price,
                Address = propertyDto.Address,
                Neighborhood = propertyDto.Neighborhood,
                Bedrooms = propertyDto.Bedrooms,
                Bathrooms = propertyDto.Bathrooms,
                Area = propertyDto.Area,
                OwnerId = ownerId,
                Status = PropertyStatus.Pending
            };

            _context.Properties.Add(property);

            await _context.SaveChangesAsync();

            return new PropertyDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Address = property.Address,
                Neighborhood = property.Neighborhood,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Area = property.Area,
                Status = property.Status,
                OwnerId = property.OwnerId
            };
        }
        public async Task<bool> UpdatePropertyAsync(
    int id,
    UpdatePropertyDto propertyDto,
    int ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            if (property.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException();
            }

            if (property.Status == PropertyStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Pending Properties cannot be edited.");
            }

            property.Title = propertyDto.Title;
            property.Description = propertyDto.Description;
            property.Price = propertyDto.Price;
            property.Address = propertyDto.Address;
            property.Neighborhood = propertyDto.Neighborhood;
            property.Bedrooms = propertyDto.Bedrooms;
            property.Bathrooms = propertyDto.Bathrooms;
            property.Area = propertyDto.Area;

            property.Status = PropertyStatus.Pending;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> MarkPropertyAsSoldAsync(
              int id,
              int ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            if (property.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException();
            }

            if (property.Status != PropertyStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved properties can be marked as sold.");
            }

            property.Status = PropertyStatus.Sold;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeletePropertyAsync(
    int id,
    int userId,
    string userRole)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            if (userRole != "Admin" &&
                property.OwnerId != userId)
            {
                throw new UnauthorizedAccessException();
            }

            // Delete physical image files
            foreach (var image in property.Images)
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    image.ImageUrl
                        .TrimStart('/')
                        .Replace(
                            "/",
                            Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Properties.Remove(property);

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> ResubmitPropertyAsync(
             int id,
             int ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            if (property.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException();
            }

            if (property.Status != PropertyStatus.Rejected)
            {
                throw new InvalidOperationException(
                    "Only rejected properties can be resubmitted.");
            }

            property.Status = PropertyStatus.Pending;
            property.RejectionReason = null;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> ApprovePropertyAsync(int id)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            if (property.Status == PropertyStatus.Sold)
            {
                throw new InvalidOperationException(
                    "Sold properties cannot be modified.");
            }

            if (property.Status != PropertyStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending properties can be approved.");
            }

            property.Status = PropertyStatus.Approved;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RejectPropertyAsync(
             int id,
             string rejectionReason)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            if (property.Status == PropertyStatus.Sold)
            {
                throw new InvalidOperationException(
                    "Sold properties cannot be modified.");
            }

            if (property.Status != PropertyStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending properties can be rejected.");
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                throw new ArgumentException(
                    "Rejection reason is required.");
            }

            property.Status = PropertyStatus.Rejected;
            property.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.Data;
using REALESTATE_.API.DTos;
using REALESTATE_.API.models;

namespace REALESTATE_.API.Services
{
    public class PropertyImageService : IPropertyImageService
    {
        private readonly AppDbContext _context;

        public PropertyImageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> AddPropertyImageAsync(
            int propertyId,
            int ownerId,
            CreatePropertyImageDto dto)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                throw new KeyNotFoundException("Property not found.");

            if (property.OwnerId != ownerId)
                throw new UnauthorizedAccessException();

            if (property.Status == PropertyStatus.Sold)
                throw new InvalidOperationException(
                    "Sold properties cannot be modified.");

            if (string.IsNullOrWhiteSpace(dto.ImageUrl))
                throw new ArgumentException(
                    "Image URL is required.");

            var imagesCount = await _context.PropertyImages
                .CountAsync(i => i.PropertyId == propertyId);

            if (imagesCount >= 10)
                throw new InvalidOperationException(
                    "A property can have a maximum of 10 images.");

            var image = new PropertyImage
            {
                ImageUrl = dto.ImageUrl,
                PropertyId = propertyId
            };

            _context.PropertyImages.Add(image);

            if (property.Status == PropertyStatus.Approved)
            {
                property.Status = PropertyStatus.Pending;
            }

            await _context.SaveChangesAsync();

            return new
            {
                message = "Image added successfully.",
                imageId = image.Id,
                imageUrl = image.ImageUrl
            };
        }

        public async Task<object> UploadPropertyImageAsync(
            int propertyId,
            int ownerId,
            UploadPropertyImageDto dto)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                throw new KeyNotFoundException(
                    "Property not found.");

            if (property.OwnerId != ownerId)
                throw new UnauthorizedAccessException();

            if (property.Status == PropertyStatus.Sold)
                throw new InvalidOperationException(
                    "Sold properties cannot be modified.");

            if (dto.Image == null || dto.Image.Length == 0)
                throw new ArgumentException(
                    "Image is required.");

            const long maxFileSize = 5 * 1024 * 1024;

            if (dto.Image.Length > maxFileSize)
                throw new InvalidOperationException(
                    "Image size cannot exceed 5 MB.");

            var imagesCount = await _context.PropertyImages
                .CountAsync(i => i.PropertyId == propertyId);

            if (imagesCount >= 10)
                throw new InvalidOperationException(
                    "A property can have a maximum of 10 images.");

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            var extension = Path.GetExtension(dto.Image.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException(
                    "Only JPG, JPEG, PNG, and WEBP images are allowed.");

            if (!allowedContentTypes.Contains(
                    dto.Image.ContentType.ToLowerInvariant()))
            {
                throw new InvalidOperationException(
                    "Invalid image content type.");
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "properties");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                uploadsFolder,
                fileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            var imageUrl = $"/images/properties/{fileName}";

            var image = new PropertyImage
            {
                ImageUrl = imageUrl,
                PropertyId = propertyId
            };

            _context.PropertyImages.Add(image);

            if (property.Status == PropertyStatus.Approved)
            {
                property.Status = PropertyStatus.Pending;
            }

            await _context.SaveChangesAsync();

            return new
            {
                message = "Image uploaded successfully.",
                imageId = image.Id,
                imageUrl = imageUrl
            };
        }

        public async Task<List<object>> GetPropertyImagesAsync(
            int propertyId)
        {
            var propertyExists = await _context.Properties
                .AnyAsync(p =>
                    p.Id == propertyId &&
                    p.Status == PropertyStatus.Approved);

            if (!propertyExists)
                throw new KeyNotFoundException(
                    "Property not found.");

            var images = await _context.PropertyImages
                .Where(i => i.PropertyId == propertyId)
                .Select(i => (object)new
                {
                    i.Id,
                    i.ImageUrl
                })
                .ToListAsync();

            return images;
        }

        public async Task<bool> DeletePropertyImageAsync(
            int propertyId,
            int imageId,
            int ownerId)
        {
            var image = await _context.PropertyImages
                .FirstOrDefaultAsync(i =>
                    i.Id == imageId &&
                    i.PropertyId == propertyId);

            if (image == null)
                throw new KeyNotFoundException(
                    "Image not found.");

            var property = await _context.Properties
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId);

            if (property == null)
                throw new KeyNotFoundException(
                    "Property not found.");

            if (property.OwnerId != ownerId)
                throw new UnauthorizedAccessException();

            if (property.Status == PropertyStatus.Sold)
                throw new InvalidOperationException(
                    "Sold properties cannot be modified.");

            if (property.Status == PropertyStatus.Approved)
            {
                property.Status = PropertyStatus.Pending;
            }

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                image.ImageUrl
                    .TrimStart('/')
                    .Replace(
                        "/",
                        Path.DirectorySeparatorChar.ToString())
            );

            _context.PropertyImages.Remove(image);

            await _context.SaveChangesAsync();

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return true;
        }
    }
}

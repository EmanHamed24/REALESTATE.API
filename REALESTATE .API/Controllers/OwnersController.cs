using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REALESTATE_.API.Data;
using REALESTATE_.API.models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using REALESTATE_.API.DTos;
namespace REALESTATE_.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public OwnersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<OwnerDto>> CreateOwner(RegisterDto registerDto)
        {
            var existingOwner = await _context.Owners
                .FirstOrDefaultAsync(o => o.Email == registerDto.Email);

            if (existingOwner != null)
            {
                return BadRequest("Email already exists.");
            }

            var owner = new Owner
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role
            };

            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();

            var OwnerDto = new OwnerDto
            {
                Id = owner.Id,
                Name = owner.Name,
                Email = owner.Email,
                Role = owner.Role,
            };
            return Ok(OwnerDto);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<OwnerDto>> GetCurrentOwner()
        {
            var ownerIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (ownerIdValue == null || !int.TryParse(ownerIdValue, out int ownerId))
            {
                return Unauthorized();
            }

            var owner = await _context.Owners
                .FirstOrDefaultAsync(o => o.Id == ownerId);

            if (owner == null)
            {
                return NotFound();
            }

            var OwnerDto = new OwnerDto
            {
                Id = owner.Id,
                Name = owner.Name,
                Email = owner.Email,
                Role = owner.Role,
            };

            return Ok(OwnerDto);
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<ActionResult> DeleteMyAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            var user = await _context.Owners
                .FirstOrDefaultAsync(o => o.Id == id);

            if (user == null)
                return NotFound("User not found.");

            // If the user is an Owner, delete physical property image files
            if (user.Role == "Owner")
            {
                var properties = await _context.Properties
                    .Include(p => p.Images)
                    .Where(p => p.OwnerId == id)
                    .ToListAsync();

                foreach (var property in properties)
                {
                    foreach (var image in property.Images)
                    {
                        var filePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            image.ImageUrl.TrimStart('/')
                                .Replace("/", Path.DirectorySeparatorChar.ToString())
                        );

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }
            }

            _context.Owners.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Account deleted successfully."
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<ActionResult> DeleteUserByAdmin(int id)
        {
            var user = await _context.Owners
                .FirstOrDefaultAsync(o => o.Id == id);

            if (user == null)
                return NotFound("User not found.");

            // If the user is an Owner, delete physical property image files
            if (user.Role == "Owner")
            {
                var properties = await _context.Properties
                    .Include(p => p.Images)
                    .Where(p => p.OwnerId == id)
                    .ToListAsync();

                foreach (var property in properties)
                {
                    foreach (var image in property.Images)
                    {
                        var filePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            image.ImageUrl.TrimStart('/')
                                .Replace("/", Path.DirectorySeparatorChar.ToString())
                        );

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }
            }

            _context.Owners.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{user.Role} account deleted successfully."
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/users")]
        public async Task<ActionResult<PageResultDto<OwnerDto>>> GetAllUsers(
             [FromQuery] string? role,
             [FromQuery] string? search,
             [FromQuery] int pageNumber = 1,
             [FromQuery] int pageSize = 10)
        {
            var query = _context.Owners.AsQueryable();

            if (pageNumber < 1)
            {
                return BadRequest("Page number must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 50)
            {
                return BadRequest("Page size must be between 1 and 50.");
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    o.Name.Contains(search) ||
                    o.Email.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(o => o.Role == role);
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize
            );

            var users = await query
               .OrderBy(o => o.Id)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .Select(o => new OwnerDto
           {
                Id = o.Id,
                Name = o.Name,
                Email = o.Email,
                Role = o.Role
           })
                  .ToListAsync();

            return Ok(new PageResultDto<OwnerDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            var owner = await _context.Owners
                .FirstOrDefaultAsync(o => o.Email == loginDto.Email);

            if (owner == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                loginDto.Password,
                owner.Password
            );

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, owner.Id.ToString()),
    new Claim(ClaimTypes.Email, owner.Email),
    new Claim(ClaimTypes.Role, owner.Role)
};

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
             );  

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "Login successful",
                token = jwt
            });
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            var user = await _context.Owners
                .FirstOrDefaultAsync(o => o.Id == id);

            if (user == null)
                return NotFound("User not found.");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest("Current password and new password are required.");
            }

            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(
                dto.CurrentPassword,
                user.Password
            );

            if (!isCurrentPasswordValid)
                return BadRequest("Current password is incorrect.");

            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest("New password must be different from current password.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Password changed successfully."
            });
        }
    }
}


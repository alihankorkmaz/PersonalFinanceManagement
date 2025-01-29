using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManagement.Models;

namespace PersonalFinanceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly FinanceContext _context;

        public AdminController(FinanceContext context)
        {
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // PUT: api/admin/users/{id}
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized(new { message = "Authentication required." });

            if (id != user.Id)
                return BadRequest(new { message = "User ID mismatch" });

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound(new { message = "User not found" });

            existingUser.Email = user.Email;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.Name = user.Name;

            _context.Entry(existingUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized(new { message = "Authentication required." });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("update-key")]
        public async Task<IActionResult> UpdateKey([FromBody] KeyUpdateDto keyUpdateDto)
        {
            if (string.IsNullOrEmpty(keyUpdateDto.Key))
            {
                return BadRequest(new { message = "Key cannot be empty." });
            }

            // Assuming you have a service to handle your data operations
            var adminSettings = await _context.AdminSettings.FirstOrDefaultAsync();

            if (adminSettings == null)
            {
                // Create a new entry if it doesn't exist
                adminSettings = new AdminSettings
                {
                    RegistrationKey = keyUpdateDto.Key,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(keyUpdateDto.ExpiresIn)
                };
                _context.AdminSettings.Add(adminSettings);
            }
            else
            {
                // Update existing key and expiration time
                adminSettings.RegistrationKey = keyUpdateDto.Key;
                adminSettings.ExpirationTime = DateTime.UtcNow.AddMinutes(keyUpdateDto.ExpiresIn);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration key updated successfully." });
        }

        [AllowAnonymous]
        [HttpGet("current-key")]
        public async Task<IActionResult> GetCurrentKey()
        {
            var adminSettings = await _context.AdminSettings.FirstOrDefaultAsync();
            if (adminSettings == null)
            {
                return NotFound(new { message = "No registration key found." });
            }
            return Ok(new { key = adminSettings.RegistrationKey });
        }

    }

}
public class KeyUpdateDto
{
    public string Key { get; set; }
    public int ExpiresIn { get; set; } // in minutes
}

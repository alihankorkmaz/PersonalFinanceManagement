using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManagement.Models;
using System.Security.Claims;
using System.Text;


namespace PersonalFinanceManagement.Controllers
{
    
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminAccountController : ControllerBase
    {
        private readonly FinanceContext _context;

        public AdminAccountController(FinanceContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var adminId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new { message = "Admin is not authenticated." });
            }

            var admin = await _context.Admins.FindAsync(int.Parse(adminId));
            if (admin == null)
            {
                return NotFound(new { message = "Admin not found." });
            }

            return Ok(new
            {
                admin.Id,
                admin.Name,
                admin.Email
            });
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var adminId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new { message = "Admin is not authenticated." });
            }

            var admin = await _context.Admins.FindAsync(int.Parse(adminId));
            if (admin == null)
            {
                return NotFound(new { message = "Admin not found." });
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin account deleted successfully." });
        }

    }
   

}


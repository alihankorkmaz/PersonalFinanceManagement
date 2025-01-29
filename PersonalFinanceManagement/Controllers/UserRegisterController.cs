using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using PersonalFinanceManagement.Dtos;
using PersonalFinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace PersonalFinanceManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserRegisterController : ControllerBase
    {
        private readonly FinanceContext _context;

        public UserRegisterController(FinanceContext context)
        {
            _context = context;
        }

        [HttpPost("register")]       
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Email == userRegisterDto.Email);
            var adminExists = await _context.Admins.AnyAsync(u=>u.Email == userRegisterDto.Email);
            if (userExists || adminExists)
            {
                return BadRequest(new { message = "Email is already in use." });
            }

            var user = new User
            {
                Name = userRegisterDto.Name,
                Email = userRegisterDto.Email,
                PasswordHash = userRegisterDto.Password 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }
    }
}
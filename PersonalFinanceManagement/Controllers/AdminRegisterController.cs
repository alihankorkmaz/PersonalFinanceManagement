using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManagement.Dtos;
using PersonalFinanceManagement.Models;


namespace PersonalFinanceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminRegisterController : ControllerBase
    {
        private readonly FinanceContext _context;

        public AdminRegisterController(FinanceContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto adminRegisterDto)
        {
            var adminExists = await _context.Admins.AnyAsync(a => a.Email == adminRegisterDto.Email);
            var userExist = await _context.Users.AnyAsync(a=>a.Email == adminRegisterDto.Email);
            if (adminExists || userExist)
            {
                return BadRequest(new ResponseDto { Message = "Email is already in use." });
            }

            var admin = new Admin
            {
                Name = adminRegisterDto.Name,
                Email = adminRegisterDto.Email,
                PasswordHash = adminRegisterDto.Password
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { Message = "Admin registered successfully." });
        }
    }
}
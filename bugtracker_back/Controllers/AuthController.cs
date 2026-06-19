using bugtracker_back.DTOs;
using bugtracker_back.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace bugtracker_back.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        private string GenerateJwtToken(AppUser user, string role)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Role, role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("Invalid email format");

            if (dto.Role != "Manager" && dto.Role != "Tester")
                return BadRequest("Role must be Manager or Tester");

            AppUser user = dto.Role == "Manager"
                ? new Manager { UserName = dto.Username, Email = dto.Email }
                : new Tester { UserName = dto.Username, Email = dto.Email };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            var token = GenerateJwtToken(user, dto.Role);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                Username = user.UserName!,
                Role = dto.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid email or password!");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Tester";

            var token = GenerateJwtToken(user, role);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                Username = user.UserName!,
                Role = role
            });
        }
    }
}
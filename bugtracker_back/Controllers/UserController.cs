using bugtracker_back.DTOs;
using bugtracker_back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace bugtracker_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public UserController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Tester";

            var bugs = await _db.Bugs
                .Where(b => b.OwnerId == userId)
                .Select(b => new UserBugDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Status = b.Status,
                    Severity = b.Severity,
                    Priority = b.Priority,
                    Platform = b.Platform,
                    Description = b.Description,
                    ImageUrl = b.ImageUrl,
                    DateAdded = b.DateAdded,
                    DateFixed = b.DateFixed,
                    ProjectId = b.ProjectId,
                    ProjectName = b.Project.Name
                })
                .ToListAsync();

            var projects = new List<ProjectResponseDto>();

            if (role == "Manager")
            {
                projects = await _db.Projects
                    .Where(p => p.OwnerId == userId)
                    .Select(p => new ProjectResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Status = p.Status,
                        OwnerId = p.OwnerId,
                        OwnerName = user.UserName!, 
                        BugCount = p.Bugs!.Count,
                        BugsSum = p.Bugs!.Select(b => new BugSummaryDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Status = b.Status,
                            Severity = b.Severity
                        }).ToList()
                    })
                    .ToListAsync();
            }

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Role = role,
                Bugs = bugs,
                Projects = projects
            };

            return Ok(profile);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UpdateUserDto dto)
        {
            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("Invalid email format");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            if (dto.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser is not null)
                    return BadRequest("Email is already taken");
            }

            if (dto.Username != user.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(dto.Username);
                if (existingUser is not null)
                    return BadRequest("Username is already taken");
            }

            user.Email = dto.Email;
            user.UserName = dto.Username;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var userBugs = await _db.Bugs.Where(b => b.OwnerId == userId).ToListAsync();
            _db.Bugs.RemoveRange(userBugs);

            var userProjects = await _db.Projects.Where(p => p.OwnerId == userId).ToListAsync();
            if (userProjects.Any())
            {
                var projectIds = userProjects.Select(p => p.Id).ToList();
                var projectBugs = await _db.Bugs
                    .Where(b => projectIds.Contains(b.ProjectId))
                    .ToListAsync();
                _db.Bugs.RemoveRange(projectBugs);
                _db.Projects.RemoveRange(userProjects);
            }

            await _db.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Account deleted successfully" });
        }
    }
}

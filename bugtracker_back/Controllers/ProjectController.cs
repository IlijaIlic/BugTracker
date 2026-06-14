using bugtracker_back.DTOs;
using bugtracker_back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bugtracker_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ProjectController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _db.Projects
                .Include(p => p.Owner)
                .Include(p => p.Bugs)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner.UserName!,
                    Status = p.Status,
                    BugCount = p.Bugs == null ? 0 : p.Bugs.Count,
                    Description = p.Description,
                    BugsSum = p.Bugs == null ? new List<BugSummaryDto>()
                        : p.Bugs.Select(b => new BugSummaryDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Status = b.Status,
                            Severity = b.Severity
                        }).ToList()
                }).ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _db.Projects
                .Include(p => p.Owner)
                .Include(p => p.Bugs)
                .Where(p => p.Id == id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerId = p.OwnerId,
                    Description = p.Description,
                    OwnerName = p.Owner.UserName!,
                    Status = p.Status,
                    BugCount = p.Bugs == null ? 0 : p.Bugs.Count,
                    Bugs = p.Bugs == null ? new List<BugResponseDto>()
                        : p.Bugs.Select(b => new BugResponseDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Platform = b.Platform,
                            Priority = b.Priority,
                            Severity = b.Severity,
                            Status = b.Status,
                            Description = b.Description,
                            ImageUrl = b.ImageUrl,
                            ProjectId = b.ProjectId,
                            ProjectName = b.Project.Name,
                            DateAdded = b.DateAdded,
                            DateFixed = b.DateFixed,
                            OwnerId = b.OwnerId,
                            OwnerName = b.Owner.UserName
                        }).ToList()
                }).FirstOrDefaultAsync();

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var projects = await _db.Projects
                .Include(p => p.Owner)
                .Include(p => p.Bugs)
                .Where(p => p.OwnerId == userId)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner.UserName!,
                    Status = p.Status,
                    BugCount = p.Bugs == null ? 0 : p.Bugs.Count,
                    Description = p.Description,
                    BugsSum = p.Bugs == null ? new List<BugSummaryDto>()
                        : p.Bugs.Select(b => new BugSummaryDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Status = b.Status,
                            Severity = b.Severity
                        }).ToList()
                }).ToListAsync();

            return Ok(projects);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var project = new Project
            {
                Id = 0,
                Name = dto.Name,
                OwnerId = userId,
                Status = dto.Status,
                Owner = null,
                Description = dto.Description               
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project is null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (project.OwnerId != userId)
                return Forbid();


            project.Name = dto.Name;
            project.Description = dto.Description;
            project.Status = dto.Status;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project is null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (project.OwnerId != userId)
                return Forbid();

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

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
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {

            var query = _db.Projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));

            var raw = await query
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.OwnerId,
                    OwnerName = p.Owner.UserName,
                    p.Status,
                    p.Description,
                    Bugs = p.Bugs!.Select(b => new BugSummaryDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Status = b.Status,
                        Severity = b.Severity
                    }).ToList()
                })
                .ToListAsync();

            var projects = raw.Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                OwnerId = p.OwnerId,
                OwnerName = p.OwnerName!,
                Status = p.Status,
                Description = p.Description,
                BugsSum = p.Bugs,
                BugCount = p.Bugs.Count
            }).ToList();

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
                    BugCount = p.Bugs == null ? 0 : p.Bugs.Count
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

            var raw = await _db.Projects
                .Where(p => p.OwnerId == userId)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.OwnerId,
                    OwnerName = p.Owner.UserName,
                    p.Status,
                    p.Description,
                    Bugs = p.Bugs!.Select(b => new BugSummaryDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Status = b.Status,
                        Severity = b.Severity
                    }).ToList()
                })
                .ToListAsync();

            var projects = raw.Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                OwnerId = p.OwnerId,
                OwnerName = p.OwnerName!,
                Status = p.Status,
                Description = p.Description,
                BugsSum = p.Bugs,
                BugCount = p.Bugs.Count
            }).ToList();

            return Ok(projects);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Project name cannot be empty");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var managerExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!managerExists)
                return BadRequest("Owner does not exist");

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
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Project name cannot be empty");

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

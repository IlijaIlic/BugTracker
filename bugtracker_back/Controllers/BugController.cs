using bugtracker_back.DTOs;
using bugtracker_back.Models;
using bugtracker_back.Services;
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
    public class BugController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly ImageService _imageService;

        public BugController(AppDbContext db, UserManager<AppUser> userManager, ImageService imageService)
        {
            _db = db;
            _userManager = userManager;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {


            var bugs = await _db.Bugs
                .Select(b => new BugResponseDto
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
                })
                .ToListAsync();

            return Ok(bugs);
        }

        // GET /api/bugs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bug = await _db.Bugs
                .Where(b => b.Id == id)
                .Select(b => new BugResponseDto
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
                })
                .FirstOrDefaultAsync();

            if (bug is null) return NotFound();
            return Ok(bug);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId, [FromQuery] string? search = null)
        {
            Console.WriteLine($"projectId: {projectId}, search: '{search}'");
            var query = _db.Bugs
                .Where(b => b.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Name.Contains(search));

            var bugs = await query
                .Select(b => new BugResponseDto
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
                })
                .ToListAsync();
            Console.WriteLine($"returned {bugs.Count} bugs"); // ← and this

            return Ok(bugs);
        }

        // GET /api/bugs/project/my/5
        [HttpGet("project/my/{projectId}")]
        public async Task<IActionResult> GetMyBugsByProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var bugs = await _db.Bugs
                .Where(b => b.ProjectId == projectId && b.OwnerId == userId)
                .Select(b => new BugResponseDto
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
                })
                .ToListAsync();

            return Ok(bugs);
        }


        // POST /api/bugs
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateBugDto dto)
        {
            var project = await _db.Projects.FindAsync(dto.ProjectId);
            if (project is null) return NotFound("Project not found");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            string? imageUrl = null;
            if (dto.Image is not null)
                imageUrl = await _imageService.SaveImageAsync(dto.Image);

            var bug = new Bug
            {
                Id = 0,
                Name = dto.Name,
                Description = dto.Description,
                Platform = dto.Platform,
                Priority = dto.Priority,
                Severity = dto.Severity,
                ImageUrl = imageUrl,
                Status = BugStatus.Active,
                DateAdded = DateTime.UtcNow,
                ProjectId = dto.ProjectId,
                Project = project,
                OwnerId = userId,
                Owner = null! // EF resolves via OwnerId
            };

            _db.Bugs.Add(bug);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = bug.Id }, new { id = bug.Id });
        }

        // PUT /api/bugs/5
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBugDto dto)
        {
            var bug = await _db.Bugs.FindAsync(id);
            if (bug is null) return NotFound();

            bug.Name = dto.Name;
            bug.Description = dto.Description;
            bug.Platform = dto.Platform;
            bug.Priority = dto.Priority;
            bug.Severity = dto.Severity;

            if (dto.DateFixed != null)
            {
                if (dto.DateFixed.Value <= bug.DateAdded)
                {
                    return BadRequest("Date error");
                }
                bug.DateFixed = dto.DateFixed;
                bug.Status = BugStatus.Fixed;
            }
            else if (dto.DateFixed == null && bug.Status == BugStatus.Fixed)
            {
                bug.DateFixed = null;
                bug.Status = BugStatus.Active;
            }


            if (dto.Image is not null)
            {
                if (bug.ImageUrl is not null)
                    _imageService.DeleteImage(bug.ImageUrl);

                bug.ImageUrl = await _imageService.SaveImageAsync(dto.Image);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/bugs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bug = await _db.Bugs.FindAsync(id);
            if (bug is null) return NotFound();

            if (bug.ImageUrl is not null)
                _imageService.DeleteImage(bug.ImageUrl);

            _db.Bugs.Remove(bug);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

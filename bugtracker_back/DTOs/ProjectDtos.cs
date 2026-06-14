using bugtracker_back.Models;

namespace bugtracker_back.DTOs
{
    public class CreateProjectDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }

    }

    public class UpdateProjectDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required ProjectStatus Status { get; set; }  
    }

    public class ProjectResponseDto { 
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required ProjectStatus Status { get; set; }
        public required string OwnerId {  get; set; }
        public required string OwnerName { get; set; }
        public int BugCount { get; set;  }
        public List<BugResponseDto>? Bugs { get; set; } = new();
        public List<BugSummaryDto>? BugsSum { get; set; } = new();
    }
}

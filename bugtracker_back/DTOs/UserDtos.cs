using bugtracker_back.Models;

namespace bugtracker_back.DTOs
{
    public class UserProfileDto
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public List<ProjectResponseDto> Projects { get; set; } = new();
        public List<UserBugDto> Bugs { get; set; } = new();
    }

    public class UserBugDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required BugStatus Status { get; set; }
        public required Severity Severity { get; set; }
        public required Priority Priority { get; set; }
        public required Platform Platform { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateFixed { get; set; }
        public required int ProjectId { get; set; }
        public required string ProjectName { get; set; }
    }

    public class UpdateUserDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}

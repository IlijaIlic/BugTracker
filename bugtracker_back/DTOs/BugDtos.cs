using bugtracker_back.Models;

namespace bugtracker_back.DTOs
{
    public class CreateBugDto
    {
        public required string Name { get; set; }
        public required Platform Platform { get; set; }
        public required Priority Priority { get; set; }
        public required Severity Severity {  get; set; }
        public string? Description { get; set; }
        public required int ProjectId { get; set; }

        //IMAGE
        public IFormFile? Image { get; set; } 
    }

    public class UpdateBugDto
    {
        public required string Name { get; set; }
        public required Platform Platform { get; set; }
        public required Priority Priority { get; set; }
        public required Severity Severity { get; set; }
        public string? Description { get; set; }
        public DateTime? DateFixed { get; set; }

        //IMAGE
        public IFormFile? Image { get; set; }
    }

    public class BugResponseDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required Platform Platform { get; set; }
        public required Priority Priority { get; set; }
        public required Severity Severity { get; set; }
        public required BugStatus Status { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateFixed { get; set; }

        public string OwnerId { get; set; }
        public string? OwnerName{ get; set; }

    }

    public class BugSummaryDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required BugStatus Status { get; set; }
        public required Severity Severity { get; set; }
    }
}

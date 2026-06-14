using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bugtracker_back.Models;

public class Bug
{
    [Key]
    public required int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public DateTime? DateFixed { get; set; }

    public required BugStatus Status { get; set; }
    public required Severity Severity { get; set; }
    public required Priority Priority { get; set; }
    public required Platform Platform { get; set; }

    // PROJECT
    [ForeignKey(nameof(ProjectId))]
    public required Project Project { get; set; }
    [Required]
    public required int ProjectId { get; set;  }

    // OWNER
    [ForeignKey(nameof(OwnerId))]
    public required AppUser Owner { get; set; }
    [Required]
    public required string OwnerId { get; set; }
}

public enum BugStatus
{
    Fixed, Active
}

public enum Severity
{
    Low, Moderate, Major, Critical
}

public enum Priority
{
    Low, Medium, High
}

public enum Platform
{
    Android, iOS, Web, MobileWeb
}
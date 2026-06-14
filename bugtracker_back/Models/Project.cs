using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bugtracker_back.Models;

public class Project {

	[Key]
	public required int Id { get; set; }

	[Required, MaxLength(100)]
	public required string Name { get; set; }

	[MaxLength(1000)]
	public string? Description { get; set; }

	// OWNER
	[Required]
	public required string OwnerId { get; set; }

	[ForeignKey(nameof(OwnerId))]
	public Manager Owner { get; set; }
	
	// STATUS
	public required ProjectStatus Status { get; set; }
	
	public ICollection<Bug>? Bugs { get; set; }
}

public enum ProjectStatus
{
	Planning,
	Active,
	Blocked
}
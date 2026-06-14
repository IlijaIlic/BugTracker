using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace bugtracker_back.Models;

public class AppUser : IdentityUser { }

public class Manager : AppUser
{
    public ICollection<Project>? Projects { get; set; }
}

public class Tester: AppUser { }
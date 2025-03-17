﻿using Microsoft.AspNetCore.Identity;
using System;

public class ApplicationUser : IdentityUser<int>
{
    public string Fullname { get; set; } 

    public string Photo { get; set; }

    public bool ConfirmedEmail { get; set; } = false;

    public string BatteryAPI { get; set; }

    public ViewAccount ViewAccount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

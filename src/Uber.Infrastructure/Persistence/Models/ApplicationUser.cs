using Microsoft.AspNetCore.Identity;
using Uber.Domain.Enums;

namespace Uber.DAL.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
}

using Microsoft.AspNetCore.Identity;

namespace MisOAuth.IdentityServer.Models;

public class User : IdentityUser<long>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CodeMelli { get; set; }
    public override string? PhoneNumber { get; set; }
    public override string? Email { get; set; }
}

public class ApplicationClaim
{
    public string Type { get; set; }
    public string Value { get; set; }
}

public class ApplicationUserSeedData
{
    public User ApplicationUser { get; set; }
    public List<ApplicationClaim> Claims { get; set; } = new List<ApplicationClaim>();
}
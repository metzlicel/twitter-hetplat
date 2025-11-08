using Microsoft.AspNetCore.Identity;

namespace Twitter.Models
{
    public class AppUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
using Microsoft.AspNet.Identity.EntityFramework;

namespace BasicAuthentication.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
    }
}

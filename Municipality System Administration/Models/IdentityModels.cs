using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Municipality_System_Administration.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Staffs = new HashSet<Staff>();
            IsActive = true;
        }

        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Staff Staff { get; set; }

        public virtual ICollection<Staff> Staffs { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(
                this,
                DefaultAuthenticationTypes.ApplicationCookie);

            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<DisposalRequest> DisposalRequests { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
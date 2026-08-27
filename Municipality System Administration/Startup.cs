using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Municipality_System_Administration;
using Municipality_System_Administration.Models;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Owin;
using Unity;
using Unity.AspNet.Mvc;

[assembly: OwinStartup(typeof(Municipality_System_Administration.Startup))]

namespace Municipality_System_Administration
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Apply pending migrations automatically
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<
                    ApplicationDbContext,
                    Municipality_System_Administration.Migrations.Configuration>());

            using (var db = new ApplicationDbContext())
            {
                db.Database.Initialize(false);
            }

            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<RoleManager<IdentityRole>>(CreateRoleManager);
            app.CreatePerOwinContext<UserManager<ApplicationUser>>(CreateUserManager);

            CreateRolesAndAdminUser();
        }

        private RoleManager<IdentityRole> CreateRoleManager(
            IdentityFactoryOptions<RoleManager<IdentityRole>> options,
            IOwinContext context)
        {
            return new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));
        }

        private UserManager<ApplicationUser> CreateUserManager(
            IdentityFactoryOptions<UserManager<ApplicationUser>> options,
            IOwinContext context)
        {
            return new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
        }

        private void CreateRolesAndAdminUser()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context));

                var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(context));

                CreateRole(roleManager, "Admin");
                CreateRole(roleManager, "AssetManager");
                CreateRole(roleManager, "MunicipalEmployee");
                CreateRole(roleManager, "DepartmentHead");
                CreateRole(roleManager, "Technician");
                CreateRole(roleManager, "FinanceOfficer");

                var admin = userManager.FindByEmail("systemadmin@gmail.com");

                if (admin == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "systemadmin@gmail.com",
                        Email = "systemadmin@gmail.com",
                        EmailConfirmed = true
                    };

                    var result = userManager.Create(user, "Admin@26");

                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }
            }
        }

        private void CreateRole(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }
        }
    }
}
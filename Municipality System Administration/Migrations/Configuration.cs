namespace Municipality_System_Administration.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration
        : DbMigrationsConfiguration<Municipality_System_Administration.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;

            ContextKey = "Municipality_System_Administration.Models.ApplicationDbContext";
        }

        protected override void Seed(Municipality_System_Administration.Models.ApplicationDbContext context)
        {
            // This method runs after migrations are applied.

            // Roles and Admin user are created in Startup.cs:
            //
            // Admin
            // AssetManager
            // MunicipalEmployee
            // DepartmentHead
            // Technician
            // FinanceOfficer
        }
    }
}

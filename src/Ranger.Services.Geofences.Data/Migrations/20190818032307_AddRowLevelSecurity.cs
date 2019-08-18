using Microsoft.EntityFrameworkCore.Migrations;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data.Migrations {
    public partial class AddRowLevelSecurity : Migration {
        protected override void Up (MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql (MultiTenantMigrationMethods.CreateTenantLoginRole ());
            migrationBuilder.Sql (MultiTenantMigrationMethods.CreateTenantLoginRolePermissions ());
            migrationBuilder.Sql (MultiTenantMigrationMethods.CreateTenantPolicy ());
        }

        protected override void Down (MigrationBuilder migrationBuilder) {

        }
    }
}
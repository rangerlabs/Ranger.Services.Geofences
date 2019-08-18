using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data {
    public class GeofencesDbContextInitializer : IGeofencesDbContextInitializer {
        private readonly GeofencesDbContext context;

        public GeofencesDbContextInitializer (GeofencesDbContext context) {
            this.context = context;
        }

        public bool EnsureCreated () {
            return context.Database.EnsureCreated ();
        }

        public void Migrate () {
            context.Database.Migrate ();
        }

        public async Task EnsureRowLevelSecurityApplied () {
            var tables = Enum.GetNames (typeof (RowLevelSecureTablesEnum));
            var loginRoleRepository = new LoginRoleRepository<GeofencesDbContext> (context);
            foreach (var table in tables) {
                await loginRoleRepository.CreateTenantLoginPolicy (table);
            }
        }
    }

    public interface IGeofencesDbContextInitializer {
        bool EnsureCreated ();
        void Migrate ();
        Task EnsureRowLevelSecurityApplied ();
    }
}
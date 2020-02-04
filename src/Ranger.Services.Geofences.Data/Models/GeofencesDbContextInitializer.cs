using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Geofences.Data
{
    public class GeofencesDbContextInitializer : IGeofencesDbContextInitializer
    {
        private readonly GeofencesDbContext context;

        public GeofencesDbContextInitializer(GeofencesDbContext context)
        {
            this.context = context;
        }

        public bool EnsureCreated()
        {
            return context.Database.EnsureCreated();
        }

        public void Migrate()
        {
            context.Database.Migrate();
        }
    }

    public interface IGeofencesDbContextInitializer
    {
        bool EnsureCreated();
        void Migrate();
    }
}
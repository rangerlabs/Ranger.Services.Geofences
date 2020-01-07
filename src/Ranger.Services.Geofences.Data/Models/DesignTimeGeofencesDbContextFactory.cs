using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ranger.Services.Geofences.Data
{
    public class DesignTimeGeofencesDbContextFactory : IDesignTimeDbContextFactory<GeofencesDbContext>
    {
        public GeofencesDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<GeofencesDbContext>();
            options.UseNpgsql(config["cloudSql:ConnectionString"]);

            return new GeofencesDbContext(options.Options);
        }
    }
}
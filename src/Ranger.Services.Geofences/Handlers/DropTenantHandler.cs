using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Services.Geofences.Data;
using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    public class DropTenantHandler : ICommandHandler<DropTenant>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILoginRoleRepository<GeofencesDbContext> loginRoleRepository;
        private readonly GeofencesDbContext identityDbContext;
        private readonly ILogger<InitializeTenantHandler> logger;

        public DropTenantHandler(IBusPublisher busPublisher, ILoginRoleRepository<GeofencesDbContext> loginRoleRepository, GeofencesDbContext identityDbContext, ILogger<InitializeTenantHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.loginRoleRepository = loginRoleRepository;
            this.identityDbContext = identityDbContext;
            this.logger = logger;
        }

        public async Task HandleAsync(DropTenant command, ICorrelationContext context)
        {
            var tables = Enum.GetNames(typeof(RowLevelSecureTablesEnum)).Concat(Enum.GetNames(typeof(PublicTablesEnum)));
            foreach (var table in tables)
            {
                logger.LogInformation($"Revoking tenant '{command.DatabaseUsername}' permissions on table: '{table}'.");
                await this.loginRoleRepository.RevokeTenantLoginRoleTablePermissions(command.DatabaseUsername, table);
            }

            logger.LogInformation("Revoking tenant '{command.DatabaseUsername}' sequence permissions");
            await this.loginRoleRepository.RevokeTenantLoginRoleSequencePermissions(command.DatabaseUsername);

            logger.LogInformation($"Dropping tenant '{command.DatabaseUsername}' from Identity database.");
            await this.loginRoleRepository.DropTenantLoginRole(command.DatabaseUsername);

            logger.LogInformation($"Geofences tenant dropped successfully.");
        }
    }
}
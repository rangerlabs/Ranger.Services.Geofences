using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences
{
    [MessageNamespace("operations")]
    public class DropTenant : ICommand
    {
        public string DatabaseUsername { get; }

        public DropTenant(string databaseUsername)
        {
            this.DatabaseUsername = databaseUsername;
        }
    }
}
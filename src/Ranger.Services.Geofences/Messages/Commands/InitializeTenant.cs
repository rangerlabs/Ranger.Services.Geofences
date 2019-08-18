using Ranger.RabbitMQ;

namespace Ranger.Services.Geofences {
    [MessageNamespace ("operations")]
    public class InitializeTenant : ICommand {
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }

        public InitializeTenant (string databaseUsername, string databasePassword) {
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
        }
    }
}
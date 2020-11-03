using System;

namespace Ranger.Services.Geofences.Tests.IntegrationTests
{
    public class FixtureResolver : IDisposable
    {
        public readonly CustomWebApplicationFactory Factory;
        public readonly GeofencesFixture Fixture;

        public FixtureResolver()
        {
            Factory = new CustomWebApplicationFactory();
            Fixture = new GeofencesFixture(Factory);
       }

        public void Dispose()
        {
            Fixture.ClearMongoDb();
        }
    }
}
using System;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ranger.Services.Geofences.Tests
{
    public class ValidationFixture
    {
        public IServiceProvider serviceProvider;

        public ValidationFixture()
        {
            var services = new ServiceCollection();

            services.AddControllers().AddFluentValidation(o =>
                {
                    o.ImplicitlyValidateChildProperties = false;
                    o.RegisterValidatorsFromAssemblyContaining<Startup>();
                });
            serviceProvider = services.BuildServiceProvider();
        }
    }
}
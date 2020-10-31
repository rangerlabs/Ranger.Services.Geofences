using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Ranger.Common;
using Ranger.Services.Geofences;

namespace Ranger.Services.Geofences
{
    public class GeofenceRequestParamsValidator : AbstractValidator<GeofenceRequestParams>
    {
        public GeofenceRequestParamsValidator()
        {
            RuleSet("Get", () => {
                RuleFor(x => x.GeofenceSortOrder)
                    .NotEmpty()
                    .Must((x) => GetGeofenceSortOrder().Contains(x, StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage($"OrderBy must be one of {String.Join(',', GetGeofenceSortOrder())}");
                RuleFor(x => x.OrderByOption)
                    .NotEmpty()
                     .Must((x) => GetOrderByOptions().Contains(x, StringComparer.InvariantCultureIgnoreCase))
                    .WithMessage($"SortOrder must be one of {String.Join(',', GetOrderByOptions())}");
                RuleFor(x => x.Page)
                    .GreaterThanOrEqualTo(0);
                RuleFor(x => x.PageCount)
                    .NotEmpty()
                    .GreaterThan(0)
                    .LessThanOrEqualTo(1000);
            });
        }

        private IEnumerable<string> GetOrderByOptions() {
            return new List<string>{
                OrderByOptions.CreatedDateLowerInvariant,
                OrderByOptions.EnabledLowerInvariant,
                OrderByOptions.ExternalIdLowerInvariant,
                OrderByOptions.ShapeLowerInvariant,
                OrderByOptions.UpdatedDateLowerInvariant
            };
        }

        private IEnumerable<string> GetGeofenceSortOrder() {
            return new List<string> {
                GeofenceSortOrders.AscendingLowerInvariant,
                GeofenceSortOrders.DescendingLowerInvariant
            };
        }
    }


}
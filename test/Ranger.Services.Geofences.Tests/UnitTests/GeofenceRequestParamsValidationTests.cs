using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using NodaTime;
using Shouldly;
using Xunit;

namespace Ranger.Services.Geofences.Tests
{

    [Collection("Validation collection")]
    public class GeofenceRequestParamsValidationTests
    {
        private readonly IValidator<GeofenceRequestParams> paramsValidator;
        public GeofenceRequestParamsValidationTests(ValidationFixture fixture)
        {
            this.paramsValidator = fixture.serviceProvider.GetRequiredServiceForTest<IValidator<GeofenceRequestParams>>();
        }

        [Fact]
        public void GeofenceSortOrder_HasValidationError_WhenEmpty()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.GeofenceSortOrder);
        }

        [Fact]
        public void GeofenceSortOrder_HasValidationError_WhenNotAscOrDesc()
        {
            var geofenceRequestParams = new GeofenceRequestParams("asdf", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.GeofenceSortOrder);
        }

        [Fact]
        public void GeofenceSortOrder_HasNoValidationError_WhenAsc()
        {
            var geofenceRequestParams = new GeofenceRequestParams("asc", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.GeofenceSortOrder);
        }

        [Fact]
        public void GeofenceSortOrder_HasNoValidationError_WhenDesc()
        {
            var geofenceRequestParams = new GeofenceRequestParams("desc", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.GeofenceSortOrder);
        }

        [Fact]
        public void OrderByOptions_HasValidationError_WhenEmpty()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.OrderByOption);
        }

        [Fact]
        public void OrderByOptions_HasValidationError_WhenNotInOptions()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "asdf", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.OrderByOption);
        }

        [Fact]
        public void OrderByOptions_HasNoValidationError_WhenExternalId()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "ExternalId", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.OrderByOption);
        }

        [Fact]
        public void OrderByOptions_HasNoValidationError_WhenShape()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "Shape", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.OrderByOption);
        }

        [Fact]
        public void OrderByOptions_HasNoValidationError_WhenEnabled()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "Enabled", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.OrderByOption);
        }

        [Fact]
        public void OrderByOptions_HasNoValidationError_WhenCreatedDate()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "CreatedDate", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.OrderByOption);
        }
 
        [Fact]
        public void OrderByOptions_HasNoValidationError_WhenUpdatedDate()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "UpdatedDate", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.OrderByOption);
        }

        [Fact]
        public void Page_HasValidationError_WhenLessThanEqualTo0()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.Page);

            var geofenceRequestParams2 = new GeofenceRequestParams("", "", -1, 0);
            var result2 = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.Page);
        }

        [Fact]
        public void Page_HasNoValidationError_WhenGreaterThan0()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 1, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.Page);
        }

        [Fact]
        public void PageCount_HasValidationError_WhenLessThanEqualTo0()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 0, 0);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.PageCount);

            var geofenceRequestParams2 = new GeofenceRequestParams("", "", 0, -1);
            var result2 = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.PageCount);
        }

        [Fact]
        public void PageCount_HasValidationError_WhenGreaterThan1000()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 0, 1001);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldHaveValidationErrorFor(r => r.PageCount);
        }


        [Fact]
        public void Page_HasNoValidationError_WhenGreaterThan0LessThan1000()
        {
            var geofenceRequestParams = new GeofenceRequestParams("", "", 1, 1);
            var result = paramsValidator.TestValidate(geofenceRequestParams, "Get");
            result.ShouldNotHaveValidationErrorFor(r => r.PageCount);
        }
    }
}

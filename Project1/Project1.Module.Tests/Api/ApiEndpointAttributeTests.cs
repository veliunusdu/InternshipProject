using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.Blazor.Server.Controllers;
using Xunit;

namespace Project1.Module.Tests.Api
{
    public class ApiEndpointAttributeTests
    {
        [Fact]
        public void NotesApiController_ShouldHaveAllowAnonymousAndEnableCorsAttributes()
        {
            var type = typeof(NotesApiController);
            type.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            type.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
            var corsAttr = type.GetCustomAttribute<EnableCorsAttribute>();
            corsAttr.Should().NotBeNull();
            corsAttr!.PolicyName.Should().Be("AllowAll");
        }

        [Fact]
        public void SystemStatusApiController_ShouldHaveAllowAnonymousAndEnableCorsAttributes()
        {
            var type = typeof(SystemStatusApiController);
            type.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            type.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
            var corsAttr = type.GetCustomAttribute<EnableCorsAttribute>();
            corsAttr.Should().NotBeNull();
            corsAttr!.PolicyName.Should().Be("AllowAll");
        }
    }
}

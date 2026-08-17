using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Project1.Module.Tests.Api
{
    public class ApiEndpointAttributeTests
    {
        private static string FindControllerFile(string fileName)
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var directory = new DirectoryInfo(currentDir);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InternshipProject.sln")))
            {
                directory = directory.Parent;
            }

            if (directory == null)
            {
                throw new FileNotFoundException("InternshipProject.sln not found");
            }

            var filePath = Path.Combine(directory.FullName, "Project1", "Project1.Blazor.Server", "Controllers", fileName);
            File.Exists(filePath).Should().BeTrue($"Controller file {fileName} should exist at {filePath}");
            return File.ReadAllText(filePath);
        }

        [Fact]
        public void NotesApiController_ShouldHaveAllowAnonymousAndEnableCorsAttributes()
        {
            string content = FindControllerFile("NotesApiController.cs");
            content.Should().Contain("[AllowAnonymous]");
            content.Should().Contain("[EnableCors(\"AllowAll\")]");
            content.Should().Contain("[Route(\"api/notes\")]");
            content.Should().Contain("[ApiController]");
        }

        [Fact]
        public void SystemStatusApiController_ShouldHaveAllowAnonymousAndEnableCorsAttributes()
        {
            string content = FindControllerFile("SystemStatusControllers.cs");
            content.Should().Contain("[AllowAnonymous]");
            content.Should().Contain("[EnableCors(\"AllowAll\")]");
            content.Should().Contain("[Route(\"api/systemstatus\")]");
            content.Should().Contain("[ApiController]");
        }
    }
}

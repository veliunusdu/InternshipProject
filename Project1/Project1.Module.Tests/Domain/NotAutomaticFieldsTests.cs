#nullable enable
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Domain
{
    public class NotAutomaticFieldsTests
    {
        private Session CreateInMemorySession()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new Session(dataLayer);
        }

        [Fact]
        public void Not_AfterConstruction_ShouldAutomaticallySetCreatedDate()
        {
            // Arrange & Act
            using var session = CreateInMemorySession();
            var not = new Not(session);

            // Assert
            not.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }
    }
}

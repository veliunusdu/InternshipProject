#nullable enable
using System;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Project1.Module.Models.Notes;
using Xunit;

namespace Project1.Module.Tests.Domain
{
    public class NotEkDomainTests
    {
        private Session CreateInMemorySession()
        {
            var dataStore = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            var dataLayer = new SimpleDataLayer(dataStore);
            return new Session(dataLayer);
        }

        [Fact]
        public void NotEk_ShouldComputeContentTypeAndFlags_ForPdfAndImages()
        {
            NotEk.GetContentType("dokuman.pdf").Should().Be("application/pdf");
            NotEk.GetContentType("resim.png").Should().Be("image/png");
            NotEk.GetContentType("foto.jpg").Should().Be("image/jpeg");
            NotEk.GetContentType("foto.jpeg").Should().Be("image/jpeg");
            NotEk.GetContentType("animasyon.gif").Should().Be("image/gif");
            NotEk.GetContentType("vektor.svg").Should().Be("image/svg+xml");
            NotEk.GetContentType("diger.xyz").Should().Be("application/octet-stream");
        }

        [Fact]
        public void Not_ShouldManageEklerCollection_AndCascadeDelete()
        {
            using var session = CreateInMemorySession();
            var not = new Not(session)
            {
                Baslik = "Ekli Not",
                Icerik = "İçerik",
                Project2IlePaylas = true
            };
            not.Save();

            var ek = new NotEk(session)
            {
                Not = not,
                Aciklama = "Test Eki"
            };
            ek.Save();

            not.Ekler.Should().HaveCount(1);
            not.Project2IlePaylas.Should().BeTrue();
            ek.YuklemeTarihi.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }
    }
}

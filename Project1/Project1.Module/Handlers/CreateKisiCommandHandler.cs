using MediatR;
using DevExpress.ExpressApp;
using Project1.Core.Commands;
using Project1.Module.Models.Customers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Module.Handlers
{
    public class CreateKisiCommandHandler : IRequestHandler<CreateKisiCommand, Guid>
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public CreateKisiCommandHandler(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        public Task<Guid> Handle(CreateKisiCommand request, CancellationToken cancellationToken)
        {
            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(Kisi));
            
            Kisi yeniKisi = objectSpace.CreateObject<Kisi>();
            yeniKisi.Ad = request.Ad;
            yeniKisi.Soyad = request.Soyad;
            yeniKisi.Email = request.Email;
            yeniKisi.Telefon = request.Telefon;

            if (request.MusteriId.HasValue)
            {
                yeniKisi.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriId.Value);
            }

            // Arka planda eklendiği için UI kurallarını set ediyoruz
            if (yeniKisi.Musteri != null) yeniKisi.IsMusteriHidden = true;

            objectSpace.CommitChanges();

            return Task.FromResult(yeniKisi.Oid);
        }
    }
}

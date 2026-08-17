using MediatR;
using DevExpress.ExpressApp;
using Project1.Core.Commands;
using Project1.Module.Models.Notes;
using Project1.Module.Models.Customers;
using Project1.Module.BusinessObjects.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Module.Handlers
{
    public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Guid>
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public CreateNoteCommandHandler(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        public Task<Guid> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(Not));
            
            Not yeniNot = objectSpace.CreateObject<Not>();
            yeniNot.Baslik = request.Baslik;
            yeniNot.Icerik = request.Icerik;
            yeniNot.Derece = (NotDerecesi)request.Derece;

            if (request.MusteriId.HasValue)
            {
                yeniNot.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriId.Value);
            }

            if (request.KisiId.HasValue)
            {
                yeniNot.Kisi = objectSpace.GetObjectByKey<Kisi>(request.KisiId.Value);
            }

            // Arka planda eklendiği için UI kurallarını manuel set ediyoruz
            if (yeniNot.Musteri != null) yeniNot.IsMusteriHidden = true;
            if (yeniNot.Kisi != null) yeniNot.IsKisiHidden = true;

            objectSpace.CommitChanges();

            return Task.FromResult(yeniNot.Oid);
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using Project1.Module.BusinessObjects.Notes;
using Project1.Module.DTOs;
using Project1.Module.Services.Interfaces;

namespace Project1.Module.Services.Implementations
{
    public class NoteService : INoteService
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public NoteService(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory ?? throw new ArgumentNullException(nameof(objectSpaceFactory));
        }

        public Task<IEnumerable<NoteDto>> GetNotesAsync(CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(Not));
            var notes = objectSpace.GetObjectsQuery<Not>()
                .Select(n => new NoteDto(
                    n.Oid,
                    n.Baslik ?? string.Empty,
                    n.Icerik ?? string.Empty,
                    n.Derece.ToString(),
                    n.Musteri != null ? n.Musteri.Ad : string.Empty,
                    n.Kisi != null ? (n.Kisi.Ad + " " + n.Kisi.Soyad).Trim() : string.Empty,
                    n.IsEmailSent
                ))
                .ToList();

            return Task.FromResult<IEnumerable<NoteDto>>(notes);
        }

        public Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(Not));
            var n = objectSpace.GetObjectByKey<Not>(id);
            if (n == null) return Task.FromResult<NoteDto?>(null);

            var dto = new NoteDto(
                n.Oid,
                n.Baslik ?? string.Empty,
                n.Icerik ?? string.Empty,
                n.Derece.ToString(),
                n.Musteri != null ? n.Musteri.Ad : string.Empty,
                n.Kisi != null ? (n.Kisi.Ad + " " + n.Kisi.Soyad).Trim() : string.Empty,
                n.IsEmailSent
            );

            return Task.FromResult<NoteDto?>(dto);
        }

        public Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace(typeof(Not));
            var not = objectSpace.CreateObject<Not>();
            not.Baslik = request.Baslik;
            not.Icerik = request.Icerik;
            not.Derece = (BusinessObjects.Enums.NotDerecesi)request.Derece;

            objectSpace.CommitChanges();

            var dto = new NoteDto(
                not.Oid,
                not.Baslik,
                not.Icerik,
                not.Derece.ToString(),
                string.Empty,
                string.Empty,
                not.IsEmailSent
            );

            return Task.FromResult(dto);
        }
    }
}

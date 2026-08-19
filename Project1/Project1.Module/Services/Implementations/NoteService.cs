#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Project1.Module.Models.Customers;
using Project1.Module.Models.Notes;
using Project1.DTOs.Notes;
using Project1.Core.Services.Interfaces;

namespace Project1.Module.Services.Implementations
{
    public class NoteService : INoteService
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly INonSecuredObjectSpaceFactory? _nonSecuredObjectSpaceFactory;

        public NoteService(
            IObjectSpaceFactory objectSpaceFactory,
            INonSecuredObjectSpaceFactory? nonSecuredObjectSpaceFactory = null)
        {
            _objectSpaceFactory = objectSpaceFactory ?? throw new ArgumentNullException(nameof(objectSpaceFactory));
            _nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
        }

        private IObjectSpace CreateObjectSpace()
        {
            if (_nonSecuredObjectSpaceFactory != null)
            {
                return _nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(typeof(Not));
            }
            return _objectSpaceFactory.CreateObjectSpace(typeof(Not));
        }

        private NoteDto MapToDto(Not n)
        {
            var eklerDto = n.Ekler?
                .Select(e => new NoteAttachmentDto(
                    e.Oid,
                    e.DosyaAdi,
                    e.ContentType,
                    e.BoyutBytes,
                    e.YuklemeTarihi,
                    $"/api/attachments/{e.Oid}/download",
                    e.IsImage,
                    e.IsPdf
                ))
                .ToList() ?? new List<NoteAttachmentDto>();

            return new NoteDto(
                n.Oid,
                n.Baslik ?? string.Empty,
                n.Icerik ?? string.Empty,
                n.Derece.ToString(),
                n.Musteri != null ? n.Musteri.Ad : string.Empty,
                n.Kisi != null ? (n.Kisi.Ad + " " + n.Kisi.Soyad).Trim() : string.Empty,
                n.IsEmailSent,
                n.CreatedDate,
                n.MailDurumu.ToString(),
                n.MailGonderilmeTarihi,
                n.MailIletilmeTarihi,
                n.MailOkunmaTarihi,
                n.Project2IlePaylas,
                eklerDto
            );
        }

        public Task<IEnumerable<NoteDto>> GetNotesAsync(bool? onlyShared = null, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();
            var query = objectSpace.GetObjectsQuery<Not>();
            if (onlyShared.HasValue && onlyShared.Value)
            {
                query = query.Where(n => n.Project2IlePaylas);
            }

            var notes = query
                .AsEnumerable()
                .Select(n => MapToDto(n))
                .ToList();

            return Task.FromResult<IEnumerable<NoteDto>>(notes);
        }

        public Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();
            var n = objectSpace.GetObjectByKey<Not>(id);
            if (n == null) return Task.FromResult<NoteDto?>(null);

            return Task.FromResult<NoteDto?>(MapToDto(n));
        }

        public Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using IObjectSpace objectSpace = CreateObjectSpace();
            var not = objectSpace.CreateObject<Not>();
            not.Baslik = request.Baslik;
            not.Icerik = request.Icerik;
            not.Derece = (BusinessObjects.Enums.NotDerecesi)request.Derece;
            not.Project2IlePaylas = request.Project2IlePaylas;

            if (request.MusteriOid.HasValue && request.MusteriOid.Value != Guid.Empty)
            {
                not.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriOid.Value);
            }

            if (request.KisiOid.HasValue && request.KisiOid.Value != Guid.Empty)
            {
                not.Kisi = objectSpace.GetObjectByKey<Kisi>(request.KisiOid.Value);
            }

            objectSpace.CommitChanges();

            return Task.FromResult(MapToDto(not));
        }

        public Task<(byte[] Bytes, string FileName, string ContentType)?> GetAttachmentFileAsync(Guid attachmentId, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();
            var ek = objectSpace.GetObjectByKey<NotEk>(attachmentId);
            if (ek?.Dosya?.Content == null || ek.Dosya.Content.Length == 0)
            {
                return Task.FromResult<(byte[] Bytes, string FileName, string ContentType)?>(null);
            }

            return Task.FromResult<(byte[] Bytes, string FileName, string ContentType)?>((
                ek.Dosya.Content,
                ek.DosyaAdi,
                ek.ContentType
            ));
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Project1.Core.Mapping;
using Project1.Core.Services.Interfaces;
using Project1.DTOs.Notes;
using Project1.Module.Models.Customers;
using Project1.Module.BusinessObjects.Enums;
using Project1.Module.Models.Notes;

namespace Project1.Business.Services.Implementations
{
    public class NoteService : INoteService
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly IMapper<Not, NoteDto> _noteMapper;
        private readonly INonSecuredObjectSpaceFactory? _nonSecuredObjectSpaceFactory;

        public NoteService(
            IObjectSpaceFactory objectSpaceFactory,
            IMapper<Not, NoteDto> noteMapper,
            INonSecuredObjectSpaceFactory? nonSecuredObjectSpaceFactory = null)
        {
            _objectSpaceFactory = objectSpaceFactory ?? throw new ArgumentNullException(nameof(objectSpaceFactory));
            _noteMapper = noteMapper ?? throw new ArgumentNullException(nameof(noteMapper));
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

        public Task<IEnumerable<NoteDto>> GetNotesAsync(bool? onlyShared = null, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();
            IQueryable<Not> query = objectSpace.GetObjectsQuery<Not>();

            if (onlyShared.HasValue && onlyShared.Value)
            {
                query = query.Where(n => n.Project2IlePaylas);
            }

            var notes = query
                .AsEnumerable()
                .Select(_noteMapper.Map)
                .ToList();

            return Task.FromResult<IEnumerable<NoteDto>>(notes);
        }

        public Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();
            var n = objectSpace.GetObjectByKey<Not>(id);
            if (n == null) return Task.FromResult<NoteDto?>(null);

            return Task.FromResult<NoteDto?>(_noteMapper.Map(n));
        }

        public Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();

            var not = objectSpace.CreateObject<Not>();
            not.Baslik = request.Baslik;
            not.Icerik = request.Icerik;
            not.Derece = (NotDerecesi)request.Derece;
            not.Project2IlePaylas = request.Project2IlePaylas;
            not.CreatedDate = DateTime.Now;

            if (request.MusteriOid.HasValue)
            {
                not.Musteri = objectSpace.GetObjectByKey<Musteri>(request.MusteriOid.Value);
            }

            if (request.KisiOid.HasValue)
            {
                not.Kisi = objectSpace.GetObjectByKey<Kisi>(request.KisiOid.Value);
            }

            objectSpace.CommitChanges();

            return Task.FromResult(_noteMapper.Map(not));
        }

        public Task<(byte[] Bytes, string FileName, string ContentType)?> GetAttachmentFileAsync(Guid attachmentId, CancellationToken cancellationToken = default)
        {
            using IObjectSpace objectSpace = CreateObjectSpace();
            var not = objectSpace.GetObjectByKey<Not>(attachmentId);

            if (not?.Dosya == null || not.Dosya.Content == null)
            {
                return Task.FromResult<(byte[] Bytes, string FileName, string ContentType)?>(null);
            }

            return Task.FromResult<(byte[] Bytes, string FileName, string ContentType)?>((
                not.Dosya.Content,
                not.DosyaAdi,
                not.ContentType
            ));
        }
    }
}

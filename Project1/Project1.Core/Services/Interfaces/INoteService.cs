#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Project1.DTOs.Notes;

namespace Project1.Core.Services.Interfaces
{
    public interface INoteService
    {
        Task<IEnumerable<NoteDto>> GetNotesAsync(CancellationToken cancellationToken = default);
        Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<NoteDto> CreateNoteAsync(CreateNoteRequestDto request, CancellationToken cancellationToken = default);
    }
}

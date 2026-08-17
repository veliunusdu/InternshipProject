using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project1.DTOs.Notes;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/notes")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public class NotesApiController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesApiController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            var notes = await _noteService.GetNotesAsync();
            return Ok(notes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetNoteById(Guid id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequestDto request)
        {
            if (request == null) return BadRequest();
            var createdNote = await _noteService.CreateNoteAsync(request);
            return CreatedAtAction(nameof(GetNoteById), new { id = createdNote.Oid }, createdNote);
        }
    }
}

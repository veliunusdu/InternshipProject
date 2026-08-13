using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Project2.DTOs;

namespace Project2.ViewModels
{
    public class NoteListViewModel
    {
        private readonly HttpClient _http;

        public List<NoteDto> Notes { get; private set; } = new();
        public bool IsLoading { get; private set; } = false;
        public string? ErrorMessage { get; private set; }

        public NoteListViewModel(HttpClient http)
        {
            _http = http;
        }

        public async Task LoadNotesAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var result = await _http.GetFromJsonAsync<List<NoteDto>>("api/notes");
                Notes = result ?? new List<NoteDto>();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Notlar yüklenirken hata oluştu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

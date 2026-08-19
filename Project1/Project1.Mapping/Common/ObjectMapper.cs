#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.DTOs.Notes;
using Project1.Mapping.Notes;
using Project1.Module.Models.Notes;

namespace Project1.Mapping.Common
{
    public class ObjectMapper : IObjectMapper
    {
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source is Not note && typeof(TDestination) == typeof(NoteDto))
            {
                return (TDestination)(object)note.ToDto();
            }

            throw new NotSupportedException($"Mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} is not registered.");
        }

        public IEnumerable<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> sources)
        {
            if (sources == null) return Enumerable.Empty<TDestination>();

            if (typeof(TSource) == typeof(Not) && typeof(TDestination) == typeof(NoteDto))
            {
                var notes = (IEnumerable<Not>)sources;
                return (IEnumerable<TDestination>)notes.ToDtoList();
            }

            return sources.Select(s => Map<TSource, TDestination>(s));
        }
    }
}

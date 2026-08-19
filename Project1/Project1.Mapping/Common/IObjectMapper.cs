#nullable enable
using System.Collections.Generic;

namespace Project1.Mapping.Common
{
    public interface IObjectMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);
        IEnumerable<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> sources);
    }
}

#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project1.Core.Services.Interfaces
{
    public interface IMailTrackingService
    {
        Task<bool> ProcessDeliveredAsync(Guid noteId, CancellationToken cancellationToken = default);
        Task<bool> ProcessReadAsync(Guid noteId, CancellationToken cancellationToken = default);
    }
}

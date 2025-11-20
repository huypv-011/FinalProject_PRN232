using System.Threading;
using System.Threading.Tasks;

namespace BookingVilla.Services.OData;

public interface IODataClient
{
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}


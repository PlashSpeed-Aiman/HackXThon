using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services.Interfaces;

public interface IConfessionService
{
    Task<ConfessionResponse> GetAbsolutionAsync(string confession, CancellationToken cancellationToken = default);
}

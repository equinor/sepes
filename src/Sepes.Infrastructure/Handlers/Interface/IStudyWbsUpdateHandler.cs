using Sepes.Infrastructure.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers.Interface
{
    public interface IStudyWbsUpdateHandler
    {
        Task Handle(Study study, CancellationToken cancellationToken = default);
    }
}

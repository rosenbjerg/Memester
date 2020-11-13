using System.Threading.Tasks;

namespace Memester.Core
{
    public interface IAsyncInitialized
    {
        Task Initialize();
    }
}
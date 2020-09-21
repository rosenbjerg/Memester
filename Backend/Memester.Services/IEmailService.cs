using System.Threading.Tasks;

namespace Memester.Services
{
    public interface IEmailService
    {
        Task Send(string receiver, string subject, string message);
    }
}
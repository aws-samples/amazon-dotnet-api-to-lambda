using System.Threading.Tasks;

namespace DotnetToLambda.Core.Services
{
    public interface ICustomerService
    {
        Task<bool> CustomerExists(string customerIdentifier);
    }
}
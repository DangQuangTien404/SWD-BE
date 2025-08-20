using System.Threading.Tasks;
using BookLibwithSub.Service.Models;

namespace BookLibwithSub.Service.Interfaces
{
    public interface IZaloPayService
    {
        Task<ZaloPayCreateOrderResult> CreateOrderAsync(int transactionId, int userId);
        Task<ZaloPayCallbackResult> HandleCallbackAsync(ZaloPayCallbackRequest request);
    }
}

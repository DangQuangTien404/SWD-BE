using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using Microsoft.Extensions.Options;

namespace BookLibwithSub.Service.Service
{
    public class ZaloPayService : IZaloPayService
    {
        private readonly ZaloPayOptions _options;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly HttpClient _httpClient;

        public ZaloPayService(
            IOptions<ZaloPayOptions> options,
            ITransactionRepository transactionRepository,
            IPaymentService paymentService,
            IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<ZaloPayCreateOrderResult> CreateOrderAsync(int transactionId, int userId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null || transaction.UserID != userId)
                throw new InvalidOperationException("Transaction not found");

            var appId = _options.AppId;
            var appTransId = $"{DateTime.UtcNow:yyMMdd}_{transactionId}";
            var appUser = userId.ToString();
            var amount = (long)transaction.Amount;
            var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var embedData = JsonSerializer.Serialize(new { redirecturl = _options.RedirectUrl });
            var items = "[]";

            var data = $"{appId}|{appTransId}|{appUser}|{amount}|{appTime}|{embedData}|{items}";
            var mac = HmacSHA256(_options.Key1, data);

            var payload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("app_id", appId),
                new KeyValuePair<string, string>("app_trans_id", appTransId),
                new KeyValuePair<string, string>("app_user", appUser),
                new KeyValuePair<string, string>("app_time", appTime.ToString()),
                new KeyValuePair<string, string>("amount", amount.ToString()),
                new KeyValuePair<string, string>("callback_url", _options.CallbackUrl ?? string.Empty),
                new KeyValuePair<string, string>("embed_data", embedData),
                new KeyValuePair<string, string>("item", items),
                new KeyValuePair<string, string>("description", $"Payment for transaction #{transactionId}"),
                new KeyValuePair<string, string>("bank_code", "zalopayapp"),
                new KeyValuePair<string, string>("mac", mac)
            });

            using var response = await _httpClient.PostAsync(_options.CreateOrderUrl, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"ZaloPay request failed with status code {response.StatusCode}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ZaloPayCreateOrderResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException("Invalid response from ZaloPay");
            }

            if (result.ReturnCode != 1)
            {
                throw new InvalidOperationException($"ZaloPay returned error {result.ReturnCode}: {result.ReturnMessage}");
            }

            return result;
        }

        public async Task<ZaloPayCallbackResult> HandleCallbackAsync(ZaloPayCallbackRequest request)
        {
            var mac = HmacSHA256(_options.Key2, request.Data);
            if (!mac.Equals(request.Mac, StringComparison.OrdinalIgnoreCase))
            {
                return new ZaloPayCallbackResult { Success = false, Message = "Invalid MAC" };
            }

            using var doc = JsonDocument.Parse(request.Data);
            var root = doc.RootElement;
            var appTransId = root.GetProperty("app_trans_id").GetString();

            if (string.IsNullOrWhiteSpace(appTransId) || !TryParseTransactionId(appTransId, out var transactionId))
            {
                return new ZaloPayCallbackResult { Success = false, Message = "Invalid app_trans_id" };
            }

            // Parse additional fields from callback data
            if (!root.TryGetProperty("amount", out var amountElement) ||
                !root.TryGetProperty("zp_trans_id", out var zpTransIdElement))
            {
                return new ZaloPayCallbackResult { Success = false, Message = "Missing required fields" };
            }

            var amount = (decimal)amountElement.GetInt64();
            var zpTransId = zpTransIdElement.GetString() ?? string.Empty;

            // Fetch transaction and ensure the amount matches
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return new ZaloPayCallbackResult { Success = false, Message = "Transaction not found" };
            }

            if (transaction.Amount != amount)
            {
                return new ZaloPayCallbackResult { Success = false, Message = "Amount mismatch" };
            }

            // Determine provider status
            var status = 0;
            if (root.TryGetProperty("return_code", out var returnCodeElement))
            {
                status = returnCodeElement.GetInt32();
            }
            else if (root.TryGetProperty("status", out var statusElement))
            {
                status = statusElement.GetInt32();
            }

            if (status != 1)
            {
                return new ZaloPayCallbackResult { Success = false, Message = "Payment not successful" };
            }

            await _paymentService.MarkTransactionPaidAsync(transactionId);

            return new ZaloPayCallbackResult
            {
                Success = true,
                TransactionId = transactionId,
                ZpTransId = zpTransId,
                Message = "OK"
            };
        }

        private static string HmacSHA256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static bool TryParseTransactionId(string appTransId, out int transactionId)
        {
            // format: yyMMdd_transactionId
            transactionId = 0;
            var idx = appTransId.IndexOf('_');
            if (idx <= 0 || idx >= appTransId.Length - 1) return false;
            var idStr = appTransId[(idx + 1)..];
            return int.TryParse(idStr, out transactionId);
        }
    }
}



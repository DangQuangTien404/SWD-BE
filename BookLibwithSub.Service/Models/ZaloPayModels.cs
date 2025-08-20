using System.Text.Json.Serialization;

namespace BookLibwithSub.Service.Models
{
    public class ZaloPayOptions
    {
        public string AppId { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string CreateOrderUrl { get; set; }
        public string CallbackUrl { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class ZaloPayCreateOrderResult
    {
        [JsonPropertyName("order_url")] public string OrderUrl { get; set; }
        [JsonPropertyName("zp_trans_token")] public string ZpTransToken { get; set; }
        [JsonPropertyName("return_code")] public int ReturnCode { get; set; }
        [JsonPropertyName("return_message")] public string ReturnMessage { get; set; }
        [JsonPropertyName("order_id")] public string OrderId { get; set; }
    }

    public class ZaloPayCallbackRequest
    {
        [JsonPropertyName("data")] public string Data { get; set; }
        [JsonPropertyName("mac")] public string Mac { get; set; }
        [JsonPropertyName("type")] public int Type { get; set; }
    }

    public class ZaloPayCallbackResult
    {
        public bool Success { get; set; }
        public int TransactionId { get; set; }
        public string ZpTransId { get; set; }
        public string Message { get; set; }
    }
}

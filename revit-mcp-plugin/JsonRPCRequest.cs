using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin
{
    public class JsonRPCRequest
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public JObject Params { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public bool IsValid()
        {
            return JsonRpc == "2.0" && !string.IsNullOrEmpty(Method) && !string.IsNullOrEmpty(Id);
        }
    }

    // success response
    public class JsonRPCSuccessResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("result")]
        public JToken Result { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class JsonRPCErrorResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("error")]
        public JsonRPCError Error { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class JsonRPCError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }
    }

    public static class JsonRPCErrorCodes
    {
        public const int ParseError = -32700;
        public const int InvalidRequest = -32600;
        public const int MethodNotFound = -32601;
        public const int InvalidParams = -32602;
        public const int InternalError = -32603;
    }
}

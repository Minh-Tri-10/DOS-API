using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PaymentAPI.Utils
{
    public static class VnPayHelper
    {
        private static string HmacSHA512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2")); // lowercase
            return sb.ToString();
        }

        public static string CreateRequestUrl(string baseUrl, string secretKey, IDictionary<string, string> inputData)
        {
            var sorted = inputData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToList();

            // URL-encode khi build query string
            var queryList = sorted.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}");
            var query = string.Join("&", queryList);

            // URL-encode khi build chuỗi hash
            var hashData = string.Join("&", sorted.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
            var secureHash = HmacSHA512(secretKey, hashData);

            return $"{baseUrl}?{query}&vnp_SecureHashType=HmacSHA512&vnp_SecureHash={secureHash}";
        }


        // Verify signed query (remove vnp_SecureHash/vnp_SecureHashType before compute)
        public static bool VerifySignature(IDictionary<string, string> allParams, string secretKey)
        {
            if (!allParams.TryGetValue("vnp_SecureHash", out var recvHash)) return false;

            // Remove hash fields
            var filtered = allParams
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToList();

            // Encode đúng URL giống lúc tạo hash
            var hashData = string.Join("&", filtered.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));

            var myHash = HmacSHA512(secretKey, hashData);
            return string.Equals(myHash, recvHash, StringComparison.OrdinalIgnoreCase);
        }

    }
}

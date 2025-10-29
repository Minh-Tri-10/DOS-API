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
        // Hàm tạo chữ ký HMAC SHA512, dùng cho việc ký và xác thực dữ liệu VNPAY
        private static string HmacSHA512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);      // Convert secretKey sang byte
            var inputBytes = Encoding.UTF8.GetBytes(data);   // Convert dữ liệu cần ký sang byte

            using var hmac = new HMACSHA512(keyBytes);       // Khởi tạo HMAC SHA512
            var hash = hmac.ComputeHash(inputBytes);         // Hash data

            // Convert hash bytes về chuỗi hex dạng lowercase (format chuẩn của VNPAY)
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // Tạo URL request sang VNPAY (dùng cho bước thanh toán)
        public static string CreateRequestUrl(string baseUrl, string secretKey, IDictionary<string, string> inputData)
        {
            // B1: Sắp xếp tham số theo alphabet (chuẩn quy định của VNPAY)
            var sorted = inputData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))                 // Bỏ tham số null/empty
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)               // Sort theo key
                .ToList();

            // B2: Build query string (dùng URL-encode)
            var queryList = sorted.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}");
            var query = string.Join("&", queryList);

            // B3: Tạo rawData để hash (VNPAY yêu cầu key=value & nối lại, cũng encode value)
            var hashData = string.Join("&", sorted.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));

            // B4: Ký dữ liệu bằng secretKey để tạo vnp_SecureHash
            var secureHash = HmacSHA512(secretKey, hashData);

            // B5: Trả về URL đầy đủ để redirect sang VNPAY
            return $"{baseUrl}?{query}&vnp_SecureHashType=HmacSHA512&vnp_SecureHash={secureHash}";
        }


        // Xác minh chữ ký trả về từ VNPAY (dùng cho bước ReturnURL / IPN)
        public static bool VerifySignature(IDictionary<string, string> allParams, string secretKey)
        {
            // Nếu không có chữ ký thì fail
            if (!allParams.TryGetValue("vnp_SecureHash", out var recvHash)) return false;

            // B1: Loại bỏ 2 field hash trước khi tính toán lại
            var filtered = allParams
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToList();

            // B2: Build string theo đúng format ban đầu
            var hashData = string.Join("&", filtered.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));

            // B3: Hash lại bằng secretKey
            var myHash = HmacSHA512(secretKey, hashData);

            // B4: So sánh hash từ VNPAY và hash tự generate
            return string.Equals(myHash, recvHash, StringComparison.OrdinalIgnoreCase);
        }

    }
}

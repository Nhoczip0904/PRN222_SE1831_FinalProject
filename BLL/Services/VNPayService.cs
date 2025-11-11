using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BLL.Services;

public interface IVNPayService
{
    string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string returnUrl);
    bool ValidateSignature(Dictionary<string, string> queryParams, string secureHash);
}

public class VNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _baseUrl;
    private readonly string _version;
    private readonly string _command;

    public VNPayService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tmnCode = configuration["VNPay:TmnCode"] ?? "TSFZ2A2L";
        _hashSecret = configuration["VNPay:HashSecret"] ?? "SNIHOORBOFJ6USCIPO48W9H6NYPBAKI4";
        _baseUrl = configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        _version = configuration["VNPay:Version"] ?? "2.1.0";
        _command = configuration["VNPay:Command"] ?? "pay";
    }

    public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string returnUrl)
    {
        var vnpay = new VNPayLibrary();
        
        vnpay.AddRequestData("vnp_Version", _version);
        vnpay.AddRequestData("vnp_Command", _command);
        vnpay.AddRequestData("vnp_TmnCode", _tmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); // VNPay yêu cầu số tiền * 100
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", GetIpAddress());
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnpay.AddRequestData("vnp_TxnRef", orderId);

        string paymentUrl = vnpay.CreateRequestUrl(_baseUrl, _hashSecret);
        return paymentUrl;
    }

    public bool ValidateSignature(Dictionary<string, string> queryParams, string secureHash)
    {
        var vnpay = new VNPayLibrary();
        
        foreach (var param in queryParams)
        {
            if (!string.IsNullOrEmpty(param.Value) && param.Key != "vnp_SecureHash")
            {
                vnpay.AddResponseData(param.Key, param.Value);
            }
        }

        string calculatedHash = vnpay.CreateSecureHash(_hashSecret);
        return calculatedHash.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }
}

public class VNPayLibrary
{
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out string? value) ? value : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        StringBuilder data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        string queryString = data.ToString();
        if (queryString.Length > 0)
        {
            queryString = queryString.Remove(queryString.Length - 1, 1);
        }

        string signData = queryString;
        string vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
        
        return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;
    }

    public string CreateSecureHash(string hashSecret)
    {
        StringBuilder data = new StringBuilder();
        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        string signData = data.ToString();
        if (signData.Length > 0)
        {
            signData = signData.Remove(signData.Length - 1, 1);
        }

        return HmacSHA512(hashSecret, signData);
    }

    private string HmacSHA512(string key, string inputData)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }
    }
}

public class VNPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

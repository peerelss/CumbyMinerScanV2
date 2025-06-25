namespace CumbyMinerScanV2.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class HttpHelper
{
    private static string _userName = "root";
    private static string _userPassword = "root";
    private static string _httpPre = "http://";

    public static async Task<string> GetDigestProtectedResourceAsync(string url, string username, string password,
        string payload)
    {
        var handler = new HttpClientHandler
        {
            PreAuthenticate = true,
            Credentials = new CredentialCache
            {
                {
                    new Uri(url),
                    "Digest",
                    new NetworkCredential(username, password)
                }
            }
        };

        using var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json") // 也可以是 text/plain 等
        };

        // 可选：加一些头
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0");
        request.Headers.Accept.ParseAdd("application/json");

        HttpResponseMessage response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"请求失败，状态码: {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}
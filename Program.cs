using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Bredd.Http;

namespace UnitTest
{
    class Program
    {
        const string c_cookieDomain = "dicax.org";
        const string c_testUrl = "https://echo.dicax.org/test";

        static void Main(string[] args)
        {
            PerformTest();
        }

        static void PerformTest()
        {
            var cookies = new CookieContainer();
            using var handler = new HttpClientHandler { CookieContainer = cookies };
            using var client = new HttpClient(handler);

            cookies.Add(new Cookie("TestCookie", "TestCookieValue", null, c_cookieDomain));
            client.DefaultRequestHeaders.Add("ClientHeader", "ClientHeaderValue");

            var args = new List<KeyValuePair<string, string>>();
            args.Add(new KeyValuePair<string, string>("date", DateTime.UtcNow.ToString("O")));
            args.Add(new KeyValuePair<string, string>("application", "HttpTraceUnitTest"));

            var request = new HttpRequestMessage(HttpMethod.Post, c_testUrl);
            request.Headers.Add("RequestHeader", "RequestHeaderValue");
            request.Content = new FormUrlEncodedContent(args);
            client.Trace(request);
            var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.Trace();
        }

        static async Task Sample()
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://echo.dicax.org");
            client.Trace(request);
            var response = await client.SendAsync(request);
            response.Trace();
        }
    }
}

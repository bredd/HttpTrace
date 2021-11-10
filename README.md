## HttpTrace: C# Class for Tracing HTTP Requests and Responses

This class provides easy-to-use trace functions for [System.Net.Http.HttpRequestMessage](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage) and for [System.Net.HttpResponseMessage](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage).

### Sample Code

```cs
static async Task Sample()
{
    using var client = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Get, "https://echo.dicax.org");
    client.Trace(request);
    var response = await client.SendAsync(request);
    response.Trace();
}
```

## HttpTrace: C# CodeBit Class for Tracing HTTP Requests and Responses

This class provides easy-to-use trace functions for [System.Net.Http.HttpRequestMessage](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage) and for [System.Net.HttpResponseMessage](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage).

### Sample Code

```cs
using Bredd.Http;

static async Task Sample()
{
    using var client = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Get, "https://echo.dicax.org");
    client.Trace(request);
    var response = await client.SendAsync(request);
    response.Trace();
}
```

### Installation

Download [HttpTrace.cs](https://github.com/bredd/HttpTrace/raw/main/HttpTrace.cs) and include it in your C# project (.NET Framework or .NET Code).

## About CodeBits

A [CodeBit](http://FileMeta.org/CodeBit.html) is a way to share common code that's lighter weight than NuGet. Each CodeBit consists of a single source code file. A structured comment at the beginning of the file indicates where to find the master copy so that automated tools can retrieve and update CodeBits to the latest version.
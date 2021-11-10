/*
---
name: HttpTrace.cs
description: CodeBit class for tracing HTTP requests and responses
url: https://github.com/bredd/HttpTrace/raw/main/HttpTrace.cs
version: 1.0
keywords: CodeBit
dateModified: 2021-11-09
license: https://opensource.org/licenses/BSD-3-Clause
# Metadata in MicroYaml format. See http://filemeta.org/CodeBit.html
...
*/

/*
=== BSD 3 Clause License ===
https://opensource.org/licenses/BSD-3-Clause

Copyright 2021 Brandt Redd

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors
may be used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Bredd.Http
{
    static class HttpTrace
    {
        static FieldInfo s_httpClient_Handler;
        static ConstructorInfo s_httpRequestHeaders_Constructor;
        static MethodInfo s_httpRequestHeaders_AddHeaders;

        // Set these up in advance for better runtime performance.
        // We use reflection here in order to get accurate traces. That makes it fragile
        // but, I expect the relevant parts of .NET are pretty stable. The traces will
        // gracefully degrade if reflection doesn't work. Information for the
        // reflection calls is obtained from source posted here:
        // https://github.com/microsoft/referencesource
        static HttpTrace()
        {
            s_httpClient_Handler = GetPrivateField(typeof(HttpClient), "_handler"); // .NET Core
            if (s_httpClient_Handler == null)
                s_httpClient_Handler = GetPrivateField(typeof(HttpClient), "handler"); // .NET Framework
            s_httpRequestHeaders_Constructor = GetPrivateConstructor(typeof(HttpRequestHeaders));
            s_httpRequestHeaders_AddHeaders = GetPrivateMethod(typeof(HttpRequestHeaders), "AddHeaders", new Type[] { typeof(HttpHeaders) });
        }

        static readonly Type[] c_emptyArgs = new Type[0];

        static FieldInfo GetPrivateField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo fi = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null) return fi;
                type = type.BaseType;
            }
            return null;
        }

        static ConstructorInfo GetPrivateConstructor(Type type, Type[] arguments = null)
        {
            if (arguments == null) arguments = c_emptyArgs;
            return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, arguments, null);
        }

        static MethodInfo GetPrivateMethod(Type type, string name, Type[] arguments=null)
        {
            if (arguments == null) arguments = c_emptyArgs;
            while (type != null)
            {
                MethodInfo mi = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, arguments, null);
                if (mi != null) return mi;
                type = type.BaseType;
            }
            return null;
        }

        public static void Trace(this HttpRequestMessage req, HttpClient client = null, TextWriter log = null)
        {
            Trace(client, req, log);
        }

        public static void Trace(this HttpClient client, HttpRequestMessage req, TextWriter log = null)
        {
            if (log == null) log = Console.Error;
            log.WriteLine("=== HttpRequest =========");
            log.WriteLine($"{req.Method} {req.RequestUri.PathAndQuery} HTTP/1.1");

            // Headers
            if (s_httpRequestHeaders_Constructor != null && s_httpRequestHeaders_AddHeaders != null)
            {
                HttpRequestHeaders headers = (HttpRequestHeaders)s_httpRequestHeaders_Constructor.Invoke(null);
                headers.Add("Host", req.RequestUri.Host);
                s_httpRequestHeaders_AddHeaders.Invoke(headers, new Object[] { req.Headers });
                if (client != null && client.DefaultRequestHeaders != null)
                {
                    s_httpRequestHeaders_AddHeaders.Invoke(headers, new Object[] { client.DefaultRequestHeaders });
                }
                log.Write(headers.ToString());
            }
            else
            {
                log.WriteLine("--- Unable to trace headers due to updates to the .NET runtime. ---");
            }

            // Cookie Header
            if (client != null)
            {
                if (s_httpClient_Handler != null)
                {
                    HttpClientHandler handler = s_httpClient_Handler.GetValue(client) as HttpClientHandler;
                    if (handler != null && handler.UseCookies && handler.CookieContainer != null)
                    {
                        var cookie = handler.CookieContainer.GetCookieHeader(req.RequestUri);
                        if (!string.IsNullOrEmpty(cookie))
                        {
                            log.WriteLine("Cookie: " + cookie);
                        }
                    }
                }
                else
                {
                    log.WriteLine("--- Unable to trace cookies due to updates to the .NET runtime. ---");
                }
            }

            // Content Headers
            if (req.Content != null && req.Content.Headers != null)
            {
                req.Content.Headers.ContentLength.HasValue.Equals(true); // This causes ContentLength to be determined so it is reported in the following line.
                log.Write(req.Content.Headers.ToString());
            }

            // End of headers
            log.WriteLine();

            // Body
            if (req.Content != null)
            {
                log.WriteLine(req.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
            log.WriteLine("=========================");
        }

        public static void Trace(this HttpClient client, HttpResponseMessage res, TextWriter log)
        {
            Trace(res, log);
        }

        public static void Trace(this HttpResponseMessage res, TextWriter log=null)
        {
            if (log == null) log = Console.Error;
            log.WriteLine("=== HttpResponse ========");
            log.WriteLine($"{(int)res.StatusCode} {res.ReasonPhrase}");
            log.WriteLine(res.Headers.ToString());
            log.WriteLine();
            log.WriteLine(res.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            log.WriteLine("=========================");
        }

        public static void Trace(this HttpContent content, System.IO.TextWriter log=null)
        {
            if (log == null) log = Console.Error;
            log.WriteLine("=== HttpContent =========");
            log.WriteLine(content.ReadAsStringAsync().GetAwaiter().GetResult());
            log.WriteLine("=========================");
        }

        public static void Trace(this Uri uri, System.IO.TextWriter log=null)
        {
            if (log == null) log = Console.Error;
            log.WriteLine(uri);
            if (!string.IsNullOrEmpty(uri.Query))
            {
                foreach (var part in uri.Query.TrimStart('?').Split('&'))
                {
                    var eq = part.IndexOf('=');
                    if (eq > 0)
                    {
                        log.WriteLine($"{part.Substring(0, eq)} = {Uri.UnescapeDataString(part.Substring(eq + 1))}");
                    }
                }
            }
        }

        private static void Trace(this HttpHeaders headers, TextWriter log)
        {
            foreach(var header in headers)
            {
                foreach(var val in header.Value)
                {
                    log.WriteLine($"{header.Key}: {val}");
                }
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZarinPal.SDK.IntegrationTests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public Queue<HttpResponseMessage> Responses { get; } = new();
    public List<HttpRequestMessage> Requests { get; } = new();
    public List<string> RequestBodies { get; } = new();

    public MockHttpMessageHandler EnqueueResponse(HttpStatusCode statusCode, string jsonResponseBody)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonResponseBody, Encoding.UTF8, "application/json")
        };
        Responses.Enqueue(response);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            RequestBodies.Add(body);
        }
        else
        {
            RequestBodies.Add(string.Empty);
        }

        if (Responses.Count > 0)
        {
            return Responses.Dequeue();
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
    }
}

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ZarinPal.Exceptions;
using ZarinPal.Models;

namespace ZarinPal.SDK.UnitTests;

public class ResponseParsingTests
{
    private class DelegatingFakeHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public DelegatingFakeHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task RequestAsync_EmptyBody_ThrowsResponseException()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        using var httpClient = new HttpClient(new DelegatingFakeHandler(httpResponse))
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com")
        };

        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        using var zarinpal = new ZarinPal(config, httpClient, httpClient);

        var act = () => zarinpal.RequestAsync("POST", "/test", new { });

        await act.Should().ThrowAsync<ResponseException>()
            .WithMessage("*body was empty*");
    }

    [Fact]
    public async Task RequestAsync_MalformedJson_ThrowsResponseException()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ not_valid_json ", Encoding.UTF8, "application/json")
        };

        using var httpClient = new HttpClient(new DelegatingFakeHandler(httpResponse))
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com")
        };

        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        using var zarinpal = new ZarinPal(config, httpClient, httpClient);

        var act = () => zarinpal.RequestAsync("POST", "/test", new { });

        await act.Should().ThrowAsync<ResponseException>()
            .WithMessage("*Failed to parse API response JSON*");
    }

    [Fact]
    public async Task RequestAsync_Http500Error_ThrowsResponseException()
    {
        var json = @"{ ""errors"": [{ ""message"": ""Internal Server Error"" }] }";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var httpClient = new HttpClient(new DelegatingFakeHandler(httpResponse))
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com")
        };

        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        using var zarinpal = new ZarinPal(config, httpClient, httpClient);

        var act = () => zarinpal.RequestAsync("POST", "/test", new { });

        var ex = await act.Should().ThrowAsync<ResponseException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        ex.Which.Message.Should().Be("Internal Server Error");
    }

    [Fact]
    public async Task RequestAsync_RestApiBusinessErrorCode_ThrowsZarinPalApiException()
    {
        var json = @"{ ""data"": { ""code"": -9, ""message"": ""Merchant ID is invalid"" } }";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var httpClient = new HttpClient(new DelegatingFakeHandler(httpResponse))
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com")
        };

        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        using var zarinpal = new ZarinPal(config, httpClient, httpClient);

        var act = () => zarinpal.RequestAsync("POST", "/test", new { });

        var ex = await act.Should().ThrowAsync<ZarinPalApiException>();
        ex.Which.Code.Should().Be(-9);
        ex.Which.Message.Should().Be("Merchant ID is invalid");
        ex.Which.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestAsync_RestErrorsEnvelope_ThrowsZarinPalApiException()
    {
        var json = @"{ ""errors"": [{ ""code"": -11, ""message"": ""Invalid amount"" }] }";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var httpClient = new HttpClient(new DelegatingFakeHandler(httpResponse))
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com")
        };

        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        using var zarinpal = new ZarinPal(config, httpClient, httpClient);

        var act = () => zarinpal.RequestAsync("POST", "/test", new { });

        var ex = await act.Should().ThrowAsync<ZarinPalApiException>();
        ex.Which.Code.Should().Be(-11);
        ex.Which.Message.Should().Be("Invalid amount");
    }

    [Fact]
    public async Task GraphqlAsync_GraphqlErrorsArray_ThrowsResponseException()
    {
        var json = @"{
            ""errors"": [
                { ""message"": ""Syntax Error GraphQL"" },
                { ""message"": ""Field not found"" }
            ]
        }";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var httpClient = new HttpClient(new DelegatingFakeHandler(httpResponse))
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com/api/v4/graphql/")
        };

        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        using var zarinpal = new ZarinPal(config, httpClient, httpClient);

        var act = () => zarinpal.GraphqlAsync("query { test }");

        var ex = await act.Should().ThrowAsync<ResponseException>();
        ex.Which.Message.Should().Contain("GraphQL error: Syntax Error GraphQL; Field not found");
    }
}

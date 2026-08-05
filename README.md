# ZarinPal .NET SDK

A comprehensive .NET SDK for integrating with ZarinPal payment gateway services. This SDK provides an easy-to-use interface for processing payments, refunds, transaction inquiries, and more.

> **Note:** All operations can be called directly on the `zarinPal` instance (e.g. `await zarinPal.CreateAsync(...)`). The resource-based methods (e.g. `await zarinPal.Payments.CreateAsync(...)`) **still work and are fully supported** for backward compatibility.

## Table of Contents
- [Target Frameworks](#target-frameworks)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
  - [Creating a Payment](#creating-a-payment)
  - [Verifying a Payment](#verifying-a-payment)
  - [Inquiring Transaction Status](#inquiring-transaction-status)
  - [Reversing a Transaction](#reversing-a-transaction)
  - [Listing Transactions](#listing-transactions)
  - [Listing Unverified Payments](#listing-unverified-payments)
  - [Creating a Refund](#creating-a-refund)
  - [Fee Calculation](#fee-calculation)
- [Error Handling](#error-handling)
- [Sandbox Environment](#sandbox-environment)
- [License](#license)

## Target Frameworks

This SDK multi-targets **`.NET 8.0`** and **`.NET Standard 2.0`**, making it compatible with:
- **.NET 8.0+** (.NET 8, .NET 9, etc.)
- **.NET Core 2.0+** / **.NET 5, 6, 7**
- **.NET Framework 4.6.1+**

## Installation

To install the ZarinPal SDK :

1- Easily add it via NuGet package manager gallery by searching:
`Ehsan.ZarinPal.SDK`

2- Run this command:
`dotnet add package Ehsan.ZarinPal.SDK`

3- Or in PMC:
`NuGet\Install-Package Ehsan.ZarinPal.SDK`

## Configuration

To use the SDK, you can configure it with Dependency Injection using `IHttpClientFactory`:

```csharp
using ZarinPal.Extensions;

builder.Services.AddZarinPal(config =>
{
    config.MerchantId = builder.Configuration["ZarinPal:MerchantId"] ?? "00000000-0000-0000-0000-000000000000";
    config.AccessToken = builder.Configuration["ZarinPal:AccessToken"] ?? "";
    config.Sandbox = builder.Configuration.GetValue<bool>("ZarinPal:Sandbox", true);
    config.UserAgent = "MyCustomApp/v1.0"; // Optional custom user-agent
    config.Timeout = TimeSpan.FromSeconds(30); // Optional timeout
});
```

To inject and use it in a controller or service:
```csharp
using ZarinPal.Interfaces;

public class PaymentController : ControllerBase
{
    private readonly IZarinPal _zarinPal;
    
    public PaymentController(IZarinPal zarinPal)
    {
        _zarinPal = zarinPal;
    }
}
```

### Configuration Options

- `MerchantId`: Your merchant ID provided by ZarinPal (UUID format)
- `AccessToken`: Access token for authentication (used for GraphQL requests)
- `Sandbox`: Whether to use the sandbox environment (default: false)
- `UserAgent`: Custom HTTP User-Agent header (default: `"ZarinPalSdk/v1 (.NET)"`)
- `Timeout`: Timeout for HTTP requests (default: 30 seconds)

## Usage

All operations are available directly on the `zarinPal` instance and return **strongly-typed response models**. All asynchronous methods accept an optional `CancellationToken`.

The table below maps each direct method to its equivalent resource-based method:

| Direct method | Resource-based equivalent |
|---|---|
| `zarinPal.CreateAsync(paymentRequest)` | `zarinPal.Payments.CreateAsync(paymentRequest)` |
| `zarinPal.CalculateFeeAsync(feeRequest)` | `zarinPal.Payments.FeeCalculationAsync(feeRequest)` |
| `zarinPal.GetRedirectUrl(authority)` | `zarinPal.Payments.GetRedirectUrl(authority)` |
| `zarinPal.VerifyAsync(verificationRequest)` | `zarinPal.Verifications.VerifyAsync(verificationRequest)` |
| `zarinPal.InquireAsync(inquiryRequest)` | `zarinPal.Inquiries.InquireAsync(inquiryRequest)` |
| `zarinPal.ReverseAsync(reversalRequest)` | `zarinPal.Reversals.ReverseAsync(reversalRequest)` |
| `zarinPal.ListTransactionsAsync(listRequest)` | `zarinPal.Transactions.ListAsync(listRequest)` |
| `zarinPal.ListUnverifiedAsync()` | `zarinPal.Unverified.ListAsync()` |
| `zarinPal.CreateRefundAsync(refundRequest)` | `zarinPal.Refunds.CreateAsync(refundRequest)` |
| `zarinPal.RetrieveRefundAsync(refundId)` | `zarinPal.Refunds.RetrieveAsync(refundId)` |
| `zarinPal.ListRefundsAsync(listRequest)` | `zarinPal.Refunds.ListAsync(listRequest)` |

### Creating a Payment

To create a payment request:

```csharp
using ZarinPal.Models;

var paymentRequest = new PaymentRequest
{
    Amount = 10000, // Amount in Rials
    CallbackUrl = "https://yoursite.com/callback",
    Description = "Payment description",
    Mobile = "09120000000", // Optional: Customer mobile number
    Email = "customer@example.com" // Optional: Customer email
};

try
{
    PaymentResult result = await zarinPal.CreateAsync(paymentRequest);
    
    // Redirect user to payment gateway page
    var paymentUrl = zarinPal.GetRedirectUrl(result.Authority);
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating payment: {ex.Message}");
}
```

### Verifying a Payment

After a payment attempt, verify the transaction:

```csharp
using ZarinPal.Models;

var verificationRequest = new VerificationRequest
{
    Amount = 10000, // Amount in Rials (must match payment amount)
    Authority = "A00000000000000000000000000000000000" // Authority from callback query
};

try
{
    VerifyResult result = await zarinPal.VerifyAsync(verificationRequest);
    
    if (result.Code == 100 || result.Code == 101) // Success code
    {
        Console.WriteLine($"Payment verified successfully. Ref ID: {result.RefId}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error verifying payment: {ex.Message}");
}
```

### Inquiring Transaction Status

To inquire about the status of a transaction:

```csharp
using ZarinPal.Models;

var inquiryRequest = new InquiryRequest
{
    Authority = "A00000000000000000000000000000000000"
};

try
{
    InquiryResult result = await zarinPal.InquireAsync(inquiryRequest);
    Console.WriteLine($"Transaction Code: {result.Code}, Status: {result.Status}, RefId: {result.RefId}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error inquiring transaction: {ex.Message}");
}
```

### Reversing a Transaction

To reverse a transaction:

```csharp
using ZarinPal.Models;

var reversalRequest = new ReversalRequest
{
    Authority = "A00000000000000000000000000000000000"
};

try
{
    ReversalResult result = await zarinPal.ReverseAsync(reversalRequest);
    Console.WriteLine($"Reversal Code: {result.Code}, Message: {result.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error reversing transaction: {ex.Message}");
}
```

### Listing Transactions

To retrieve a list of transactions via GraphQL:

```csharp
using ZarinPal.Models;

var transactionListRequest = new TransactionListRequest
{
    TerminalId = "TERMINAL_ID",
    Limit = 10,
    Offset = 0
};

try
{
    List<TransactionItem> items = await zarinPal.ListTransactionsAsync(transactionListRequest);
    foreach (var item in items)
    {
        Console.WriteLine($"ID: {item.Id}, Status: {item.Status}, Amount: {item.Amount}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error listing transactions: {ex.Message}");
}
```

### Listing Unverified Payments

To retrieve a list of unverified payments:

```csharp
try
{
    UnverifiedResult result = await zarinPal.ListUnverifiedAsync();
    if (result.Authorities != null)
    {
        foreach (var item in result.Authorities)
        {
            Console.WriteLine($"Authority: {item.Authority}, Amount: {item.Amount}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error listing unverified payments: {ex.Message}");
}
```

### Creating a Refund

To create a refund request via GraphQL:

```csharp
using ZarinPal.Models;
using ZarinPal.Enums;

var refundRequest = new RefundCreateRequest
{
    SessionId = "SESSION_ID",
    Amount = 1000,
    Description = "Refund description",
    Method = RefundMethod.PAYA,
    Reason = "CUSTOMER_REQUEST"
};

try
{
    RefundCreateResult result = await zarinPal.CreateRefundAsync(refundRequest);
    Console.WriteLine($"Refund created with ID: {result.Id}, Amount: {result.Amount}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating refund: {ex.Message}");
}
```

### Fee Calculation

To calculate the transaction fee before creating a payment:

```csharp
using ZarinPal.Models;

var feeCalculationRequest = new FeeCalculationRequest
{
    MerchantId = "YOUR_MERCHANT_ID", // Optional if configured globally
    Amount = 10000, // Amount in Rials
    Currency = "IRR"
};

try
{
    FeeCalculationResult result = await zarinPal.CalculateFeeAsync(feeCalculationRequest);
    Console.WriteLine($"Fee: {result.Fee}, FeeType: {result.FeeType}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error calculating fee: {ex.Message}");
}
```

## Error Handling

The SDK throws specific exceptions for different error scenarios:

- `ValidationException`: Thrown when input parameter validation fails.
- `ResponseException`: Thrown when HTTP or GraphQL responses contain network errors or unparseable bodies.
- `ZarinPalApiException`: Thrown when ZarinPal API returns a business error code (e.g., `code != 100` and `101`). Contains a `Code` property with the error code.

```csharp
try
{
    var result = await zarinPal.CreateAsync(paymentRequest);
}
catch (ZarinPal.Exceptions.ValidationException validationEx)
{
    Console.WriteLine($"Validation error: {validationEx.Message}");
}
catch (ZarinPal.Exceptions.ZarinPalApiException apiEx)
{
    Console.WriteLine($"ZarinPal API error code {apiEx.Code}: {apiEx.Message}");
}
catch (ZarinPal.Exceptions.ResponseException responseEx)
{
    Console.WriteLine($"API error: {responseEx.Message}, Status Code: {responseEx.StatusCode}");
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
```

## Sandbox Environment

For testing purposes, you can use ZarinPal's sandbox environment by setting `Sandbox = true` in the configuration:

```csharp
var config = new Config
{
    MerchantId = "YOUR_SANDBOX_MERCHANT_ID",
    AccessToken = "YOUR_SANDBOX_ACCESS_TOKEN",
    Sandbox = true // Use sandbox environment
};
```

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.
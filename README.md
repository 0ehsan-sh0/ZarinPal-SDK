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

1-Easily add it via Nuget package manager gallery just Search :
`Ehsan.ZarinPal.SDK`

2-Run this command :
`dotnet add package Ehsan.ZarinPal.SDK`

3-Or in the PMC :
`NuGet\Install-Package Ehsan.ZarinPal.SDK`

## Configuration

To use the SDK, you need to configure it with your ZarinPal credentials:

```csharp
using ZarinPal.Extensions;

builder.Services.AddZarinPal(config =>
{
    config.MerchantId = builder.Configuration["ZarinPal:MerchantId"] ?? "00000000-0000-0000-0000-000000000000";
    config.AccessToken = builder.Configuration["ZarinPal:AccessToken"] ?? "";
    config.Sandbox = builder.Configuration.GetValue<bool>("ZarinPal:Sandbox", true);
});
```

To use it in a class:
```csharp
using ZarinPal.Interfaces;

public class PaymentController : ControllerBase
{
    private readonly IZarinPal zarinPal;
    public PaymentController(IZarinPal zarinPalCons)
    {
        zarinPal = zarinPalCons;
    }
}
```

### Configuration Options

- `MerchantId`: Your merchant ID provided by ZarinPal (UUID format)
- `AccessToken`: Access token for authentication (used for GraphQL requests)
- `Sandbox`: Whether to use the sandbox environment (default: false)

## Usage

All operations are available directly on the `zarinPal` instance. The table below maps each direct method to its equivalent resource-based method (still supported):

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
    Amount = 1000, // Amount in Rials
    CallbackUrl = "https://yoursite.com/callback",
    Description = "Payment description",
    Mobile = "09120000000", // Optional: Customer mobile number
    Email = "customer@example.com" // Optional: Customer email
};

try
{
    var response = await zarinPal.CreateAsync(paymentRequest);
    
    // Extract the authority from the response
    var authority = response.GetProperty("data").GetProperty("authority").GetString();
    
    // Redirect user to payment page
    var paymentUrl = zarinPal.GetRedirectUrl(authority);
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error creating payment: {ex.Message}");
}
```

> Equivalent resource-based call (still supported): `zarinPal.Payments.CreateAsync(paymentRequest)`

### Verifying a Payment

After a successful payment, verify the transaction:

```csharp
using ZarinPal.Models;

var verificationRequest = new VerificationRequest
{
    Amount = 1000, // Amount in Rials (must match the payment amount)
    Authority = "A00000000000000000000000000000000000" // Authority from payment response
};

try
{
    var response = await zarinPal.VerifyAsync(verificationRequest);
    
    // Check if verification was successful
    var status = response.GetProperty("data").GetProperty("code").GetInt32();
    if (status == 100) // Success code
    {
        Console.WriteLine("Payment verified successfully");
        var refId = response.GetProperty("data").GetProperty("ref_id").GetString();
        Console.WriteLine($"Ref ID: {refId}");
    }
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error verifying payment: {ex.Message}");
}
```

### Inquiring Transaction Status

To inquire about the status of a transaction:

```csharp
using ZarinPal.Models;

var inquiryRequest = new InquiryRequest
{
    Authority = "A00000000000000000000000000000000000" // Authority from payment response
};

try
{
    var response = await zarinPal.InquireAsync(inquiryRequest);
    
    // Process the response
    var status = response.GetProperty("data").GetProperty("code").GetInt32();
    Console.WriteLine($"Transaction status: {status}");
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error inquiring transaction: {ex.Message}");
}
```

### Reversing a Transaction

To reverse a transaction:

```csharp
using ZarinPal.Models;

var reversalRequest = new ReversalRequest
{
    Authority = "A00000000000000000000000000000000000" // Authority from payment response
};

try
{
    var response = await zarinPal.ReverseAsync(reversalRequest);
    
    // Process the response
    var status = response.GetProperty("data").GetProperty("code").GetInt32();
    Console.WriteLine($"Reversal status: {status}");
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error reversing transaction: {ex.Message}");
}
```

### Listing Transactions

To retrieve a list of transactions via GraphQL:

```csharp
using ZarinPal.Models;

var transactionListRequest = new TransactionListRequest
{
    TerminalId = "TERMINAL_ID", // Your terminal ID
    Limit = 10, // Number of transactions to return
    Offset = 0 // Offset for pagination
};

try
{
    var response = await zarinPal.ListTransactionsAsync(transactionListRequest);
    
    // Process the response
    Console.WriteLine(response.ToString());
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error listing transactions: {ex.Message}");
}
```

### Listing Unverified Payments

To retrieve a list of unverified payments:

```csharp
try
{
    var response = await zarinPal.ListUnverifiedAsync();
    
    // Process the response
    Console.WriteLine(response.ToString());
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error listing unverified payments: {ex.Message}");
}
```

### Creating a Refund

To create a refund request via GraphQL:

```csharp
using ZarinPal.Models;
using ZarinPal.Enums

var refundRequest = new RefundCreateRequest
{
    SessionId = "SESSION_ID", // Session ID of the transaction to refund
    Amount = 1000, // Amount to refund in Rials
    Description = "Refund description", // Optional
    Method = RefundMethod.PAYA, // Optional: Refund method that can be PAYA or CARD
    Reason = "CUSTOMER_REQUEST" // Optional: Refund reason
};

try
{
    var response = await zarinPal.CreateRefundAsync(refundRequest);
    
    // Process the response
    Console.WriteLine(response.ToString());
}
catch (Exception ex)
{
    // Handle exception
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
    Amount = 1000, // Amount in Rials
    Currency = "IRR" // Optional: Currency code
};

try
{
    var response = await zarinPal.CalculateFeeAsync(feeCalculationRequest);
    
    // Process the response
    Console.WriteLine(response.ToString());
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error calculating fee: {ex.Message}");
}
```

## Error Handling

The SDK throws specific exceptions for different error scenarios:

- `ValidationException`: Thrown when input validation fails
- `ResponseException`: Thrown when HTTP or GraphQL responses contain errors
- `ZarinPalApiException`: Thrown when ZarinPal API returns a non-success business error code (e.g., `data.code != 100` and `101`). Contains a `Code` property with the error code.

```csharp
try
{
    // SDK operations (direct or resource-based calls both work)
    var response = await zarinPal.CreateAsync(paymentRequest);
}
catch (ZarinPal.Exceptions.ValidationException validationEx)
{
    // Handle validation errors
    Console.WriteLine($"Validation error: {validationEx.Message}");
}
catch (ZarinPal.Exceptions.ZarinPalApiException apiEx)
{
    // Handle ZarinPal business logic errors (e.g., invalid merchant, insufficient permissions)
    Console.WriteLine($"ZarinPal API error code {apiEx.Code}: {apiEx.Message}");
}
catch (ZarinPal.Exceptions.ResponseException responseEx)
{
    // Handle API HTTP response errors
    Console.WriteLine($"API error: {responseEx.Message}, Status Code: {responseEx.StatusCode}");
}
catch (Exception ex)
{
    // Handle other errors
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

This project is licensed under the MIT License - see the [LICENSE.txt](../LICENSE.txt) file for details.
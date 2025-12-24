# ZarinPal .NET SDK

A comprehensive .NET SDK for integrating with ZarinPal payment gateway services. This SDK provides an easy-to-use interface for processing payments, refunds, transaction inquiries, and more.

## Table of Contents
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

## Installation

To install the ZarinPal SDK, you can add the project reference to your .NET project:

```xml
<!-- Add project reference to your .csproj file -->
<ProjectReference Include="path/to/ZarinPal-SDK/ZarinPal-SDK.csproj" />
```

## Configuration

To use the SDK, you need to configure it with your ZarinPal credentials:

```csharp
using ZarinPal;

var config = new Config
{
    MerchantId = "YOUR_MERCHANT_ID", // Your merchant ID provided by ZarinPal
    AccessToken = "YOUR_ACCESS_TOKEN", // Access token for authentication (for GraphQL requests)
    Sandbox = true // Set to true to use sandbox environment, false for production
};

var zarinPal = new ZarinPal(config);
```

### Configuration Options

- `MerchantId`: Your merchant ID provided by ZarinPal (UUID format)
- `AccessToken`: Access token for authentication (used for GraphQL requests)
- `Sandbox`: Whether to use the sandbox environment (default: false)

## Usage

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
    var response = await zarinPal.Payments.CreateAsync(paymentRequest);
    
    // Extract the authority from the response
    var authority = response.GetProperty("data").GetProperty("authority").GetString();
    
    // Redirect user to payment page
    var paymentUrl = zarinPal.GetBaseUrl() + "/pg/StartPay/" + authority;
}
catch (Exception ex)
{
    // Handle exception
    Console.WriteLine($"Error creating payment: {ex.Message}");
}
```

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
    var response = await zarinPal.Verifications.VerifyAsync(verificationRequest);
    
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
    var response = await zarinPal.Inquiries.InquireAsync(inquiryRequest);
    
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
    var response = await zarinPal.Reversals.ReverseAsync(reversalRequest);
    
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
    var response = await zarinPal.Transactions.ListAsync(transactionListRequest);
    
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
    var response = await zarinPal.Unverified.ListAsync();
    
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
    var response = await zarinPal.Refunds.CreateAsync(refundRequest);
    
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
    MerchantId = "YOUR_MERCHANT_ID", // Your merchant ID
    Amount = 1000, // Amount in Rials
    Currency = "IRR" // Optional: Currency code
};

try
{
    var response = await zarinPal.Payments.FeeCalculationAsync(feeCalculationRequest);
    
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
- `ResponseException`: Thrown when API responses contain errors

```csharp
try
{
    // SDK operations
    var response = await zarinPal.Payments.CreateAsync(paymentRequest);
}
catch (ZarinPal.Exceptions.ValidationException validationEx)
{
    // Handle validation errors
    Console.WriteLine($"Validation error: {validationEx.Message}");
}
catch (ZarinPal.Exceptions.ResponseException responseEx)
{
    // Handle API response errors
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
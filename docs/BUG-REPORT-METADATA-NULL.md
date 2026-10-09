# Bug Report: `PaymentRequest.Metadata` null value triggers "The metadata must be an array." (Code -9)

**Report Date:** 2026-10-06  
**Package:** `Ehsan.ZarinPal.SDK` (v2.0.2)  
**Severity:** P0 — Correctness / Integration Failure  
**Component:** `ZarinPal-SDK/Models/PaymentModels.cs`, `ZarinPal-SDK/Resources/Payments.cs`, `ZarinPal-SDK/ZarinPal.cs`  

---

## 1. Description & Summary

When initiating a payment request using `Payments.CreateAsync(PaymentRequest)` without explicitly setting `request.Metadata`, the ZarinPal REST API endpoint `/pg/v4/payment/request.json` returns HTTP 400 (or validation error with code -9) with the following error message:

```json
{
  "data": {},
  "errors": {
    "message": "The metadata must be an array.",
    "code": -9,
    "validations": []
  }
}
```

This manifests to library consumers as a `ZarinPalApiException` with:
- **Code:** `-9`
- **Message:** `"The metadata must be an array."`

---

## 2. Root Cause Analysis

1. **Model Definition:**
   In `ZarinPal-SDK/Models/PaymentModels.cs` (lines 43–44):
   ```csharp
   [JsonPropertyName("metadata")]
   public object? Metadata { get; set; }
   ```
   When a caller instantiates `PaymentRequest`, `Metadata` defaults to C# `null`.

2. **JSON Serialization in `ZarinPal.RequestAsync`:**
   In `ZarinPal-SDK/ZarinPal.cs` (lines 195–214):
   ```csharp
   var node = JsonSerializer.SerializeToNode(data);
   jsonObject = node as JsonObject ?? new JsonObject();
   ```
   Because `JsonSerializer` does not omit null values by default, the payload serialized and sent to ZarinPal includes:
   ```json
   {
     "merchant_id": "00000000-0000-0000-0000-000000000000",
     "amount": 500000,
     "callback_url": "https://localhost:44301/payment/callback",
     "description": "...",
     "mobile": "09123456789",
     "email": null,
     "metadata": null
   }
   ```

3. **ZarinPal API Server Validation:**
   The backend of ZarinPal (`https://sandbox.zarinpal.com` & `https://payment.zarinpal.com`) is implemented in PHP/Laravel, where the validation rule for the request endpoint requires:
   ```php
   'metadata' => 'array'
   ```
   In PHP:
   - `is_array(json_decode('[]', true))` is `true`
   - `is_array(json_decode('{"mobile":"..."}', true))` is `true`
   - `is_array(json_decode('{}', true))` is `true`
   - `is_array(json_decode('null', true))` is `false`

   Because `"metadata": null` was explicitly sent in the JSON body, the validation fails with `"The metadata must be an array."`.

4. **Historical Note:**
   In `tests/ZarinPal.SDK.E2ETests/LiveSandboxE2ETests.cs` (line 33), the author had manually specified:
   ```csharp
   Metadata = new object[] { }
   ```
   which bypassed this issue in the test suite, but regular consumers who omit `Metadata` encounter the failure immediately.

---

## 3. Reproduction Steps

```csharp
var config = new Config
{
    MerchantId = "00000000-0000-0000-0000-000000000000",
    Sandbox = true
};
using var client = new ZarinPal(config);

// Omit Metadata (common standard usage)
var request = new PaymentRequest
{
    Amount = 50000,
    CallbackUrl = "https://example.com/callback",
    Description = "Order #1234",
    Mobile = "09123456789"
};

// Throws ZarinPalApiException: The metadata must be an array. (Code -9)
var result = await client.Payments.CreateAsync(request);
```

Direct curl verification:
```powershell
# Fails with {"errors":{"message":"The metadata must be an array.","code":-9}}:
Invoke-RestMethod -Uri "https://sandbox.zarinpal.com/pg/v4/payment/request.json" -Method Post -ContentType "application/json" -Body '{"merchant_id":"00000000-0000-0000-0000-000000000000","amount":10000,"callback_url":"https://example.com","description":"test","metadata":null}'

# Succeeds with Code 100:
Invoke-RestMethod -Uri "https://sandbox.zarinpal.com/pg/v4/payment/request.json" -Method Post -ContentType "application/json" -Body '{"merchant_id":"00000000-0000-0000-0000-000000000000","amount":10000,"callback_url":"https://example.com","description":"test","metadata":[]}'
```

---

## 4. Recommended Fixes for ZarinPal-SDK v2.0.3

### Fix 1: Default `Metadata` Property to Empty Array
In `ZarinPal-SDK/Models/PaymentModels.cs`:
```csharp
/// <summary>
/// Optional metadata payload. Defaults to an empty array to comply with ZarinPal API expectations.
/// </summary>
[JsonPropertyName("metadata")]
public object? Metadata { get; set; } = Array.Empty<object>();
```

### Fix 2: Auto-populate `Metadata` from `Mobile` / `Email` in `Payments.CreateAsync`
In `ZarinPal-SDK/Resources/Payments.cs`:
```csharp
public async Task<PaymentResult> CreateAsync(PaymentRequest data, CancellationToken cancellationToken = default)
{
    if (data == null) throw new ArgumentNullException(nameof(data));

    // Validate input data
    Validator.ValidateAmount(data.Amount);
    Validator.ValidateCallbackUrl(data.CallbackUrl);
    Validator.ValidateMobile(data.Mobile);
    Validator.ValidateEmail(data.Email);

    // Auto-normalize metadata if null or empty
    if (data.Metadata == null)
    {
        if (!string.IsNullOrEmpty(data.Mobile) || !string.IsNullOrEmpty(data.Email))
        {
            var meta = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(data.Mobile)) meta["mobile"] = data.Mobile;
            if (!string.IsNullOrEmpty(data.Email)) meta["email"] = data.Email;
            data.Metadata = meta;
        }
        else
        {
            data.Metadata = Array.Empty<object>();
        }
    }

    // Make the API request
    var result = await Client.RequestAsync<PaymentResult>("POST", Endpoints.PaymentRequest, data, cancellationToken);
    return result ?? throw new ResponseException("API returned an empty payment response.");
}
```

### Fix 3: Safeguard in `ZarinPal.RequestAsync`
In `ZarinPal-SDK/ZarinPal.cs`:
Ensure that if a JSON node property named `"metadata"` has a null value, it is either removed or replaced with an empty array `new JsonArray()`.

---

## 5. Workaround for Existing Consumers (v2.0.2)

Until v2.0.3 is published, callers must explicitly set `Metadata` on `PaymentRequest`:

```csharp
var request = new PaymentRequest
{
    Amount = amountInRials,
    CallbackUrl = callbackUrl,
    Description = description,
    Mobile = mobile,
    Email = email,
    Metadata = !string.IsNullOrEmpty(mobile) 
        ? new Dictionary<string, string> { ["mobile"] = mobile }
        : (object)Array.Empty<object>()
};
```

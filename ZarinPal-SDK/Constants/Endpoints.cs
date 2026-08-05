namespace ZarinPal.Constants;

/// <summary>
/// Centralized API endpoint routes for ZarinPal.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// REST endpoint for creating payment requests.
    /// </summary>
    public const string PaymentRequest = "/pg/v4/payment/request.json";

    /// <summary>
    /// Path prefix for payment redirection.
    /// </summary>
    public const string StartPay = "/pg/StartPay/";

    /// <summary>
    /// REST endpoint for calculating payment transaction fees.
    /// </summary>
    public const string FeeCalculation = "/pg/v4/payment/feeCalculation.json";

    /// <summary>
    /// REST endpoint for verifying payment transactions.
    /// </summary>
    public const string Verify = "/pg/v4/payment/verify.json";

    /// <summary>
    /// REST endpoint for inquiring payment status.
    /// </summary>
    public const string Inquiry = "/pg/v4/payment/inquiry.json";

    /// <summary>
    /// REST endpoint for reversing payment transactions.
    /// </summary>
    public const string Reverse = "/pg/v4/payment/reverse.json";

    /// <summary>
    /// REST endpoint for listing unverified payment transactions.
    /// </summary>
    public const string Unverified = "/pg/v4/payment/unVerified.json";
}

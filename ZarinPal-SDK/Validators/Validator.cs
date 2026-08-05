using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ZarinPal.Exceptions;
using ZarinPal.Enums;

namespace ZarinPal.Validators;

/// <summary>
/// Class containing validation methods for various input parameters
/// </summary>
public static partial class Validator
{
#if NET8_0_OR_GREATER
    [GeneratedRegex(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.IgnoreCase)]
    private static partial Regex MerchantIdRegex();

    [GeneratedRegex(@"^[AS][0-9a-zA-Z]{35}$")]
    private static partial Regex AuthorityRegex();

    [GeneratedRegex(@"^https?://[a-zA-Z0-9.-]+(?::[0-9]+)?(?:/.*)?$")]
    private static partial Regex CallbackUrlRegex();

    [GeneratedRegex(@"^09[0-9]{9}$")]
    private static partial Regex MobileRegex();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailRegex();
#else
    private static readonly Regex MerchantIdRegexInstance = new(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AuthorityRegexInstance = new(@"^[AS][0-9a-zA-Z]{35}$", RegexOptions.Compiled);
    private static readonly Regex CallbackUrlRegexInstance = new(@"^https?://[a-zA-Z0-9.-]+(?::[0-9]+)?(?:/.*)?$", RegexOptions.Compiled);
    private static readonly Regex MobileRegexInstance = new(@"^09[0-9]{9}$", RegexOptions.Compiled);
    private static readonly Regex EmailRegexInstance = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    private static Regex MerchantIdRegex() => MerchantIdRegexInstance;
    private static Regex AuthorityRegex() => AuthorityRegexInstance;
    private static Regex CallbackUrlRegex() => CallbackUrlRegexInstance;
    private static Regex MobileRegex() => MobileRegexInstance;
    private static Regex EmailRegex() => EmailRegexInstance;
#endif

    /// <summary>
    /// Validates the merchant ID format.
    /// </summary>
    public static void ValidateMerchantId(string? merchantId)
    {
        if (string.IsNullOrEmpty(merchantId) || !MerchantIdRegex().IsMatch(merchantId))
        {
            throw new ValidationException("Invalid merchant_id format. It should be a valid UUID.");
        }
    }

    /// <summary>
    /// Validates the authority string format.
    /// </summary>
    public static void ValidateAuthority(string authority)
    {
        if (string.IsNullOrEmpty(authority) || !AuthorityRegex().IsMatch(authority))
        {
            throw new ValidationException("Invalid authority format. It should be a string starting with \"A\" or \"S\" followed by 35 alphanumeric characters.");
        }
    }

    /// <summary>
    /// Validates payment amount.
    /// </summary>
    public static void ValidateAmount(decimal amount, decimal minAmount = 1000)
    {
        if (amount < minAmount)
        {
            throw new ValidationException($"Amount must be at least {minAmount}.");
        }
    }

    /// <summary>
    /// Validates callback URL format.
    /// </summary>
    public static void ValidateCallbackUrl(string callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl) || !CallbackUrlRegex().IsMatch(callbackUrl))
        {
            throw new ValidationException("Invalid callback URL format. It should start with http:// or https:// and include a valid host.");
        }
    }

    /// <summary>
    /// Validates mobile number format.
    /// </summary>
    public static void ValidateMobile(string? mobile)
    {
        if (!string.IsNullOrEmpty(mobile) && !MobileRegex().IsMatch(mobile))
        {
            throw new ValidationException("Invalid mobile number format.");
        }
    }

    /// <summary>
    /// Validates email address format.
    /// </summary>
    public static void ValidateEmail(string? email)
    {
        if (!string.IsNullOrEmpty(email) && !EmailRegex().IsMatch(email))
        {
            throw new ValidationException("Invalid email format.");
        }
    }

    /// <summary>
    /// Validates currency code.
    /// </summary>
    public static void ValidateCurrency(string? currency)
    {
        string[] validCurrencies = { "IRR", "IRT" };
        if (!string.IsNullOrEmpty(currency) && !validCurrencies.Contains(currency))
        {
            throw new ValidationException("Invalid currency format. Allowed values are \"IRR\" or \"IRT\".");
        }
    }

    /// <summary>
    /// Validates terminal ID parameter.
    /// </summary>
    public static void ValidateTerminalId(string? terminalId)
    {
        if (string.IsNullOrEmpty(terminalId))
        {
            throw new ValidationException("Terminal ID is required.");
        }
    }

    /// <summary>
    /// Validates transaction filter parameter.
    /// </summary>
    public static void ValidateFilter(string? filter)
    {
        string[] validFilters = { "PAID", "VERIFIED", "TRASH", "ACTIVE", "REFUNDED" };
        if (!string.IsNullOrEmpty(filter) && !validFilters.Contains(filter))
        {
            throw new ValidationException("Invalid filter value.");
        }
    }

    /// <summary>
    /// Validates pagination limit parameter.
    /// </summary>
    public static void ValidateLimit(int? limit)
    {
        if (limit != null && limit <= 0)
        {
            throw new ValidationException("Limit must be a positive integer.");
        }
    }

    /// <summary>
    /// Validates pagination offset parameter.
    /// </summary>
    public static void ValidateOffset(int? offset)
    {
        if (offset != null && offset < 0)
        {
            throw new ValidationException("Offset must be a non-negative integer.");
        }
    }

    /// <summary>
    /// Validates refund session ID.
    /// </summary>
    public static void ValidateSessionId(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new ValidationException("Session ID is required.");
        }
    }

    /// <summary>
    /// Validates refund method parameter.
    /// </summary>
    public static void ValidateMethod(RefundMethod? method)
    {
        if (method == null)
        {
            throw new ValidationException("Method is required. Allowed values are \"PAYA\" or \"CARD\".");
        }
    }

    /// <summary>
    /// Validates refund reason parameter.
    /// </summary>
    public static void ValidateReason(string reason)
    {
        string[] validReasons = {
            "CUSTOMER_REQUEST",
            "DUPLICATE_TRANSACTION",
            "SUSPICIOUS_TRANSACTION",
            "OTHER"
        };
        if (string.IsNullOrEmpty(reason) || !validReasons.Contains(reason))
        {
            throw new ValidationException("Invalid reason. Allowed values are \"CUSTOMER_REQUEST\", \"DUPLICATE_TRANSACTION\", \"SUSPICIOUS_TRANSACTION\", or \"OTHER\".");
        }
    }
}
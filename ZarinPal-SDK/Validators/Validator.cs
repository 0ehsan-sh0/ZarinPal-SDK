using System.Text.RegularExpressions;
using ZarinPal.Exceptions;
using ZarinPal.Enums;

namespace ZarinPal.Validators;

/// <summary>
/// Data class for wage information
/// </summary>
public class Wage
{
    public string Iban { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Class containing validation methods for various input parameters
/// </summary>
public static class Validator
{
    public static void ValidateMerchantId(string? merchantId)
    {
        if (string.IsNullOrEmpty(merchantId) || !Regex.IsMatch(merchantId, @"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$"))
        {
            throw new ValidationException("Invalid merchant_id format. It should be a valid UUID.");
        }
    }

    public static void ValidateAuthority(string authority)
    {
        if (string.IsNullOrEmpty(authority) || !Regex.IsMatch(authority, @"^[AS][0-9a-zA-Z]{35}$"))
        {
            throw new ValidationException("Invalid authority format. It should be a string starting with \"A\" or \"S\" followed by 35 alphanumeric characters.");
        }
    }

    public static void ValidateAmount(decimal amount, decimal minAmount = 1000)
    {
        if (amount < minAmount)
        {
            throw new ValidationException($"Amount must be at least {minAmount}.");
        }
    }

    public static void ValidateCallbackUrl(string callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl) || !Regex.IsMatch(callbackUrl, @"^https?://.*$"))
        {
            throw new ValidationException("Invalid callback URL format. It should start with http:// or https://.");
        }
    }

    public static void ValidateMobile(string? mobile)
    {
        if (!string.IsNullOrEmpty(mobile) && !Regex.IsMatch(mobile, @"^09[0-9]{9}$"))
        {
            throw new ValidationException("Invalid mobile number format.");
        }
    }

    public static void ValidateEmail(string? email)
    {
        if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            throw new ValidationException("Invalid email format.");
        }
    }

    public static void ValidateCurrency(string? currency)
    {
        string[] validCurrencies = { "IRR", "IRT" };
        if (!string.IsNullOrEmpty(currency) && !validCurrencies.Contains(currency))
        {
            throw new ValidationException("Invalid currency format. Allowed values are \"IRR\" or \"IRT\".");
        }
    }

    public static void ValidateWages(IEnumerable<Wage>? wages)
    {
        if (wages != null)
        {
            foreach (var wage in wages)
            {
                if (!Regex.IsMatch(wage.Iban, @"^[A-Z]{2}[0-9]{2}[0-9A-Z]{1,30}$"))
                {
                    throw new ValidationException("Invalid IBAN format in wages.");
                }
                if (wage.Amount <= 0)
                {
                    throw new ValidationException("Wage amount must be greater than zero.");
                }
                if (wage.Description.Length > 255)
                {
                    throw new ValidationException("Wage description must be provided and less than 255 characters.");
                }
            }
        }
    }

    public static void ValidateTerminalId(string? terminalId)
    {
        if (string.IsNullOrEmpty(terminalId))
        {
            throw new ValidationException("Terminal ID is required.");
        }
    }

    public static void ValidateFilter(string? filter)
    {
        string[] validFilters = { "PAID", "VERIFIED", "TRASH", "ACTIVE", "REFUNDED" };
        if (!string.IsNullOrEmpty(filter) && !validFilters.Contains(filter))
        {
            throw new ValidationException("Invalid filter value.");
        }
    }

    public static void ValidateLimit(int? limit)
    {
        if (limit != null && limit <= 0)
        {
            throw new ValidationException("Limit must be a positive integer.");
        }
    }

    public static void ValidateOffset(int? offset)
    {
        if (offset != null && offset < 0)
        {
            throw new ValidationException("Offset must be a non-negative integer.");
        }
    }

    public static void ValidateCardPan(string? cardPan)
    {
        if (!string.IsNullOrEmpty(cardPan) && !Regex.IsMatch(cardPan, @"^[0-9]{16}$"))
        {
            throw new ValidationException("Invalid card PAN format. It should be a 16-digit number.");
        }
    }

    public static void ValidateSessionId(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new ValidationException("Session ID is required.");
        }
    }

    public static void ValidateMethod(RefundMethod? method)
    {
        if (method == null)
        {
            throw new ValidationException("Method is required. Allowed values are \"PAYA\" or \"CARD\".");
        }
    }

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
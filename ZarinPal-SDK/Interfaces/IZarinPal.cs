using System;
using System.Collections.Generic;
using ZarinPal.Resources;

namespace ZarinPal.Interfaces;
    public interface IZarinPal
    {
        Payments Payments { get; }
        Refunds Refunds { get; }
        Transactions Transactions { get; }
        Verifications Verifications { get; }
        Reversals Reversals { get; }
        Unverified Unverified { get; }
        Inquiries Inquiries { get; }
    
        string GetBaseUrl();
    }


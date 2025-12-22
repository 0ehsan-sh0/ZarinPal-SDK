using ZarinPal.Interfaces;

namespace ZarinPal.Resources;

/// <summary>
/// Abstract base class for all resource classes
/// </summary>
public abstract class BaseResource
{
    /// <summary>
    /// The ZarinPal client instance
    /// </summary>
    protected IZarinPalClient Client { get; }

    /// <summary>
    /// Creates a new BaseResource instance
    /// </summary>
    /// <param name="client">The ZarinPal client instance</param>
    protected BaseResource(IZarinPalClient client)
    {
        Client = client;
    }
}
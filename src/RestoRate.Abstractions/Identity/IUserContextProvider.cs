namespace RestoRate.Abstractions.Identity;

public interface IUserContextProvider
{
    /// <summary>
    /// Higher values mean higher priority when multiple providers can provide a user context.
    /// </summary>
    int Priority { get; }

    bool TryGet(out IUserContext userContext);
}

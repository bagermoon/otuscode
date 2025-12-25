namespace RestoRate.Auth.Authentication.ClientCredentials;

public interface IClientCredentialsTokenProvider : IDisposable
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}

namespace RestoRate.Auth.Authentication.ClientCredentials;

public interface IClientCredentialsTokenClient
{
    Task<ClientCredentialsTokenResult> RequestTokenAsync(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken);
}

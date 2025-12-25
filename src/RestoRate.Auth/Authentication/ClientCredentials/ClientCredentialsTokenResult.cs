namespace RestoRate.Auth.Authentication.ClientCredentials;

public readonly record struct ClientCredentialsTokenResult(
    bool Success,
    string? AccessToken,
    int ExpiresInSeconds,
    string? ErrorMessage)
{
    public static ClientCredentialsTokenResult Failed(string? errorMessage = null)
        => new(false, null, 0, errorMessage);

    public static ClientCredentialsTokenResult Succeeded(string accessToken, int expiresInSeconds)
        => new(true, accessToken, expiresInSeconds, null);
}

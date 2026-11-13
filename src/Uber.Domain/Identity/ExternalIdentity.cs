using Uber.Domain.Common;
namespace Uber.Domain.Identity;
public sealed record ExternalIdentity
{
    private ExternalIdentity(string provider, string providerUserId) { Provider = provider; ProviderUserId = providerUserId; }
    public string Provider { get; }
    public string ProviderUserId { get; }
    public static Result<ExternalIdentity> Create(string provider, string providerUserId)
    {
        if (string.IsNullOrWhiteSpace(provider)) return Result<ExternalIdentity>.Failure(IdentityErrors.ExternalProviderRequired);
        if (string.IsNullOrWhiteSpace(providerUserId)) return Result<ExternalIdentity>.Failure(IdentityErrors.ExternalUserIdRequired);
        return Result<ExternalIdentity>.Success(new(provider.Trim(), providerUserId.Trim()));
    }
}

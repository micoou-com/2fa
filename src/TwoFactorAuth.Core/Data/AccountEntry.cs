namespace TwoFactorAuth.Core.Data;

public sealed class AccountEntry
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Issuer { get; init; } = "";
    public string AccountName { get; init; } = "";
    public string SecretBase32 { get; init; } = "";
    public int Digits { get; init; } = 6;
    public int PeriodSeconds { get; init; } = 30;
}

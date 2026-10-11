namespace JobApplication.Infrastructure.Settings;

public sealed class DistributedCacheSettings
{
    public const string SectionName = "DistributedCache";
    public string ConnectionString { get; set; } = null!;
    public string InstanceName { get; set; } = null!;
    public int DefaultExpirationMinutes { get; set; }
}

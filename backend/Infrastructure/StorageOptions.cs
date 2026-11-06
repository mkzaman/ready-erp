namespace ReadyErp.Api.Infrastructure;

/// <summary>
/// Resolved upload storage settings.
///
/// The path is configurable so the app can be containerised without code
/// changes: point <c>Storage:UploadPath</c> at a mounted volume (e.g.
/// <c>/app/data/uploads</c>) and uploads survive container recreation.
/// Put the SQLite file on the same volume.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>Absolute path on disk where uploaded files are written.</summary>
    public required string UploadPath { get; init; }

    /// <summary>Public URL prefix the files are served under.</summary>
    public string RequestPath { get; init; } = "/uploads";

    /// <summary>Maximum accepted upload size in bytes.</summary>
    public long MaxBytes { get; init; } = 5 * 1024 * 1024;

    /// <summary>Permitted file extensions, lowercase and dot-prefixed.</summary>
    public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    /// <summary>
    /// Builds the options from configuration, defaulting to
    /// <c>&lt;contentRoot&gt;/wwwroot/uploads</c>, and ensures the directory exists.
    /// </summary>
    public static StorageOptions Create(IConfiguration configuration, string contentRootPath)
    {
        var configured = configuration["Storage:UploadPath"];

        var path = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(contentRootPath, "wwwroot", "uploads")
            : Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(contentRootPath, configured);

        path = Path.GetFullPath(path);
        Directory.CreateDirectory(path);

        return new StorageOptions
        {
            UploadPath = path,
            RequestPath = configuration["Storage:RequestPath"] ?? "/uploads",
            MaxBytes = configuration.GetValue<long?>("Storage:MaxBytes") ?? 5 * 1024 * 1024
        };
    }
}

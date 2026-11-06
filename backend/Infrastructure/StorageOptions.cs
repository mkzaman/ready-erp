namespace ReadyErp.Api.Infrastructure;

/// <summary>
/// Resolved upload storage settings.
///
/// The path is configurable so the app can be containerised without code
/// changes: point <c>Storage:UploadPath</c> at a mounted volume (e.g.
/// <c>/data/uploads</c>) and uploads survive container recreation. Put the
/// SQLite file on the same volume.
///
/// Resolution is deliberately fault-tolerant. Image upload is a secondary
/// feature; a read-only or root-owned directory must never stop the API from
/// serving quotations and invoices. If no writable location can be found the
/// app still starts, <see cref="IsWritable"/> is false, and the upload
/// endpoints report a clear error instead of the process crashing on boot.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>Absolute path on disk where uploaded files are written.</summary>
    public required string UploadPath { get; init; }

    /// <summary>Public URL prefix the files are served under.</summary>
    public string RequestPath { get; init; } = "/uploads";

    /// <summary>Maximum accepted upload size in bytes.</summary>
    public long MaxBytes { get; init; } = 5 * 1024 * 1024;

    /// <summary>
    /// False when no writable upload directory could be resolved. Uploads are
    /// rejected with an explanatory message; everything else works normally.
    /// </summary>
    public bool IsWritable { get; init; }

    /// <summary>Why storage is unavailable, for logging and the API response.</summary>
    public string? UnavailableReason { get; init; }

    /// <summary>Permitted file extensions, lowercase and dot-prefixed.</summary>
    public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public static StorageOptions Create(IConfiguration configuration, string contentRootPath)
    {
        var requestPath = configuration["Storage:RequestPath"] ?? "/uploads";
        var maxBytes = configuration.GetValue<long?>("Storage:MaxBytes") ?? 5 * 1024 * 1024;
        var configured = configuration["Storage:UploadPath"];

        // Preference order. The temp directory is a last resort so that a
        // misconfigured container still boots and still serves images for the
        // lifetime of the process.
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(configured)) candidates.Add(Resolve(configured, contentRootPath));
        candidates.Add(Path.Combine(contentRootPath, "wwwroot", "uploads"));
        candidates.Add(Path.Combine(Path.GetTempPath(), "readyerp-uploads"));

        var failures = new List<string>();

        foreach (var candidate in candidates.Distinct())
        {
            if (TryPrepare(candidate, out var error))
            {
                if (failures.Count > 0)
                {
                    Console.Error.WriteLine(
                        $"[storage] Falling back to '{candidate}'. Earlier candidates failed: {string.Join("; ", failures)}");
                }

                return new StorageOptions
                {
                    UploadPath = candidate,
                    RequestPath = requestPath,
                    MaxBytes = maxBytes,
                    IsWritable = true
                };
            }

            failures.Add($"'{candidate}' -> {error}");
        }

        var reason = $"No writable upload directory. Tried: {string.Join("; ", failures)}";
        Console.Error.WriteLine(
            $"[storage] {reason}. Image upload is DISABLED; the rest of the API is unaffected. " +
            "Set Storage__UploadPath to a writable path (e.g. /data/uploads on a mounted volume).");

        return new StorageOptions
        {
            // Still a usable value so the static-file provider can be built.
            UploadPath = Path.Combine(Path.GetTempPath(), "readyerp-uploads"),
            RequestPath = requestPath,
            MaxBytes = maxBytes,
            IsWritable = false,
            UnavailableReason = reason
        };
    }

    private static string Resolve(string path, string contentRootPath) =>
        Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(contentRootPath, path));

    /// <summary>
    /// Creates the directory if needed and proves it is actually writable.
    /// Existence is not enough: a root-owned directory inside a container is
    /// readable but not writable by the non-root app user.
    /// </summary>
    private static bool TryPrepare(string path, out string error)
    {
        try
        {
            Directory.CreateDirectory(path);

            var probe = Path.Combine(path, $".write-probe-{Guid.NewGuid():N}");
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);

            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }
}

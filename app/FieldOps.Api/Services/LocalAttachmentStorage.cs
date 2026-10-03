namespace FieldOps.Api.Services;

public class LocalAttachmentStorage : IAttachmentStorage
{
    private readonly string _rootPath;

    public LocalAttachmentStorage(IWebHostEnvironment environment)
    {
        _rootPath = Path.Combine(
            environment.ContentRootPath,
            "storage",
            "attachments");

        Directory.CreateDirectory(_rootPath);
    }

    public async Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(blobPath);

        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await content.CopyToAsync(
            fileStream,
            cancellationToken);
    }

    public Task<Stream> DownloadAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(blobPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "Attachment was not found.",
                fullPath);
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(blobPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string blobPath)
    {
        var normalizedPath = blobPath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, normalizedPath));

        var root = Path.GetFullPath(_rootPath)
            .TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                root,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid attachment path.");
        }

        return fullPath;
    }
}
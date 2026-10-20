using System.Collections.Concurrent;
using FieldOps.Api.Services;

namespace FieldOps.Api.Tests;

public sealed class InMemoryAttachmentStorage : IAttachmentStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _files =
        new(StringComparer.Ordinal);

    public int FileCount => _files.Count;

    public bool Contains(string blobPath) =>
        _files.ContainsKey(blobPath);

    public async Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await using var memoryStream = new MemoryStream();

        await content.CopyToAsync(
            memoryStream,
            cancellationToken);

        _files[blobPath] = memoryStream.ToArray();
    }

    public Task<Stream> DownloadAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_files.TryGetValue(blobPath, out var bytes))
        {
            throw new FileNotFoundException(
                "Attachment was not found.",
                blobPath);
        }

        Stream stream = new MemoryStream(
            bytes,
            writable: false);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _files.TryRemove(blobPath, out _);

        return Task.CompletedTask;
    }
}
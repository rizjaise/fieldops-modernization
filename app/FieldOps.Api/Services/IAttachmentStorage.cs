namespace FieldOps.Api.Services;

public interface IAttachmentStorage
{
    Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string blobPath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string blobPath,
        CancellationToken cancellationToken = default);
}
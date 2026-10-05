namespace FieldOps.Api.Services;

public class AttachmentStorageOptions
{
    public string ContainerName { get; set; } = "attachments";

    public string StorageAccountName { get; set; } = string.Empty;
}
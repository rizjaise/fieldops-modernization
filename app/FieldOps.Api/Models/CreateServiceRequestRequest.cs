namespace FieldOps.Api.Models;

public class CreateServiceRequestRequest
{
    public int CustomerId { get; set; }

    public string Description { get; set; } = string.Empty;
}
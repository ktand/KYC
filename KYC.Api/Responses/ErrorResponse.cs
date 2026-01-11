namespace KYC.Controllers;

public partial class ErrorResponse
{

    [System.Text.Json.Serialization.JsonPropertyName("error")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public required string Error { get; set; }
}
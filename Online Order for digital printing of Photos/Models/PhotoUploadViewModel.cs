using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class PhotoUploadViewModel
{
    public int? UserId { get; set; }

    [Required]
    public IFormFile PhotoFile { get; set; }

    [Required]
    public int? PrintSizeId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }
}

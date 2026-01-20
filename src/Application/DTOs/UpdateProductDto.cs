using Domain.Enums;

namespace Application.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public ProductStatus? Status { get; set; }
}

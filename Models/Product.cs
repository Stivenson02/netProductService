namespace ProductService.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Features { get; set; } = "";
    public string? Description { get; set; }
    public int UserId { get; set; }
}

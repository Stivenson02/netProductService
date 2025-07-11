using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using System.Security.Claims;

namespace ProductService.Controllers;

[ApiController]
[Route("products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var products = await _db.Products
            .Where(p => p.UserId == userId)
            .ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        product.UserId = userId;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product updated)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (product == null) return NotFound();

        product.Name = updated.Name;
        product.Features = updated.Features;
        product.Description = updated.Description;

        await _db.SaveChangesAsync();

        return Ok(product);
    }
}

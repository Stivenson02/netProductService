using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using System.Security.Claims;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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

        // Disparar webhook de n8n
        var webhookUrl = "https://n8n.srv885850.hstgr.cloud/webhook/3af0813f-6be9-4886-a6eb-294ea614eaf5";

        var payload = new
        {
            productId = product.Id,
            name = product.Name,
            features = product.Features
        };

        using var http = new HttpClient();
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        string? generatedDescription = null;
        string? errorMessage = null;

        try
        {
            Console.WriteLine($"Enviando webhook a: {webhookUrl}");
            Console.WriteLine($"Payload: {json}");

            var response = await http.PostAsync(webhookUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Webhook respondió: {body}");

            // Intentar parsear el JSON para extraer la descripción generada
            var result = JsonSerializer.Deserialize<JsonElement>(body);
            if (result.ValueKind == JsonValueKind.Array && result.GetArrayLength() > 0)
            {
                var item = result[0];
                if (item.TryGetProperty("description", out var desc))
                {
                    generatedDescription = desc.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            Console.WriteLine($"Error al notificar a n8n: {ex.Message}");
        }

        // Si tenemos una descripción generada, actualizar el producto
        if (!string.IsNullOrWhiteSpace(generatedDescription))
        {
            product.Description = generatedDescription;
            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            product,
            updated = !string.IsNullOrEmpty(generatedDescription),
            error = errorMessage
        });
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

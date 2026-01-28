using Application.DTOs;
using Application.Interfaces;
using Audit.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IQueueService _queueService;
    private readonly ITraceContext _traceContext;

    public ProductsController(
        IProductService productService, 
        IQueueService queueService,
        ITraceContext traceContext)
    {
        _productService = productService;
        _queueService = queueService;
        _traceContext = traceContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByStatus(ProductStatus status)
    {
        var products = await _productService.GetByStatusAsync(status);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);
        
        // Enviar ID do produto para a fila SQS com o traceId do contexto
        await _queueService.SendMessageAsync(product.Id.ToString(), _traceContext.TraceId);
        
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

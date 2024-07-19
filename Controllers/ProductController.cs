using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly MySqlConnectionHandler _connectionHandler;
    private readonly Product _product;

    public ProductController(IConfiguration configuration)
    {
        _connectionHandler = new MySqlConnectionHandler(configuration);
        _product = new Product();
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductResponse>> Get()
    {
        try
        {
            var products = _product.Get(_connectionHandler);
            var productResponses = products.Select(p => ProductResponse.Get(p)).ToList();
            return Ok(new ProductListResponse { Status = 0, Products = productResponses });
        }
        catch (RecordNotFoundException ex)
        {
            return Ok(ErrorResponse.Get(501, ex.Message));
        }
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult<ProductResponse> Get(int id)
    {
        try
        {
            var product = _product.Get(_connectionHandler, id);
            return Ok(ProductResponse.Get(product));
        }
        catch (RecordNotFoundException ex)
        {
            return Ok(ErrorResponse.Get(501, ex.Message));
        }
    }

    [HttpPost]
    public ActionResult<ProductResponse> Add(Product product)
    {
        try
        {
            product.Add(_connectionHandler);
            return Ok(ProductResponse.Get(product));
        }
        catch (RecordNotFoundException ex)
        {
            return Ok(ErrorResponse.Get(501, ex.Message));
        }
    }

    [HttpPut]
    public ActionResult<ProductResponse> Update(int id, Product product)
    {
        try
        {
            product.Update(_connectionHandler, id);
            return Ok(ProductResponse.Get(product));
        }
        catch (RecordNotFoundException ex)
        {
            return Ok(ErrorResponse.Get(501, ex.Message));
        }
    }

    [HttpDelete]
    [Route("{id}")]
    public ActionResult<ProductResponse> Delete(int id)
    {
        try
        {
            var product = _product.Get(_connectionHandler, id);
            if (product == null)
            {
                return NotFound(ErrorResponse.Get(404, "Product not found"));
            }
            
            bool deleteSuccess = product.Delete(_connectionHandler);
            if (deleteSuccess)
            {
                return Ok(ProductResponse.Get(product));
            }
            else
            {
                return Ok(ErrorResponse.Get(1, "Error deleting product"));
            }
        }
        catch (Exception ex)
        {
            return Ok(ErrorResponse.Get(501, ex.Message));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class ProductListResponse : JsonResponse
{
    public List<ProductResponse> Products { get; set; }

    public static ProductListResponse Get(List<Product> products)
    {
        ProductListResponse response = new ProductListResponse();
        response.Status = 1;
        response.Products = products.Select(product => ProductResponse.Get(product)).ToList();
        return response;
    }
}
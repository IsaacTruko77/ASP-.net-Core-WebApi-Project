using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class ProductResponse : JsonResponse
{
    public Product Product { get; set; }

    public static ProductResponse Get(Product product)
    {
        ProductResponse response = new ProductResponse();
        response.Status = 0;
        response.Product = product;
        return response;
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;



public class Mapper
{

    #region ProducMapper
        
    public static Product ToProduct(DataRow row)
    {
        Product product = new Product
        {
            Product_Id = Convert.ToInt32(row["product_id"]),
            Product_Name = row["product_name"].ToString(),
            Product_Price = Convert.ToDouble(row["product_price"]),
            Product_Desc = row["product_desc"].ToString()
        };
        return product;
    }

    public static List<Product> ToProductList(DataTable table)
    {
        var products = new List<Product>();
        foreach (DataRow row in table.Rows)
        {
            products.Add(ToProduct(row));
        }
        return products;
    }
    
    #endregion
}

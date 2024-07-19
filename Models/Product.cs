using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1;
using MySql.Data.MySqlClient;


public class Product
{
    #region Statements

    private string select = "SELECT product_id, product_name, product_price, product_desc FROM Products;";
    private string selectOne = "SELECT product_id, product_name, product_price, product_desc FROM Products WHERE product_id = @ProductId;";
    private string insert = "INSERT INTO Products (product_name, product_price, product_desc) VALUES (@ProductName, @ProductPrice, @ProductDesc);";
    private string update = "UPDATE Products SET product_name = @ProductName, product_price = @ProductPrice, product_desc = @ProductDesc WHERE product_id = @ProductId;";
    private string delete = "DELETE FROM Products WHERE product_id = @ProductId;";

    #endregion

    #region Attributes

    private int _productId;
    private string _productName;
    private double _productPrice;
    private string _productDesc;
        
    #endregion

    #region Properties

    public int Product_Id { get => _productId; set => _productId = value; }

    public string Product_Name { get => _productName; set => _productName = value; }


    public double Product_Price { get => _productPrice; set => _productPrice = value; }

    public string Product_Desc { get => _productDesc; set => _productDesc = value; }


    #endregion

    #region Constructors

    public Product()
    {
        _productId = 0;
        _productName = "";
        _productPrice = 0;
        _productDesc = "";
    }

    public Product(int productId, string productName, double productPrice, string productDesc)
    {
        _productId = productId;
        _productName = productName;
        _productPrice = productPrice;
        _productDesc = productDesc;
    }
        
    #endregion

    #region Instance Methods

    public override string ToString()
    {
        return $"{_productId}, {_productName}, {_productPrice}, {_productDesc}";
    }


    public bool Add(MySqlConnectionHandler connectionHandler)
    {
        MySqlCommand cmd = new MySqlCommand(insert);
        cmd.Parameters.AddWithValue("@ProductName", _productName);
        cmd.Parameters.AddWithValue("@ProductPrice", _productPrice);
        cmd.Parameters.AddWithValue("@ProductDesc", _productDesc);
        
        return connectionHandler.ExecuteNonQuery(cmd);
    }

    public bool Delete(MySqlConnectionHandler connectionHandler)
    {
        MySqlCommand cmd = new MySqlCommand(delete);
        cmd.Parameters.AddWithValue("@ProductId", _productId);
        bool result = connectionHandler.ExecuteNonQuery(cmd);
        return result;
    }

    public bool Update(MySqlConnectionHandler connectionHandler, int id)
    {
        MySqlCommand cmd = new MySqlCommand(update);
        cmd.Parameters.AddWithValue("@ProductName", _productName);
        cmd.Parameters.AddWithValue("@ProductPrice", _productPrice);
        cmd.Parameters.AddWithValue("@ProductDesc", _productDesc);
        return connectionHandler.ExecuteNonQuery(cmd);
    }

    #endregion

    #region Class Methods

    public List<Product> Get(MySqlConnectionHandler connectionHandler)
    {
        MySqlCommand cmd = new MySqlCommand(select);
        return Mapper.ToProductList(connectionHandler.ExecuteQuery(cmd));

    }

    public Product Get(MySqlConnectionHandler connectionHandler, int id)
    {
        MySqlCommand cmd = new MySqlCommand(selectOne);
        cmd.Parameters.AddWithValue("@ProductId", id);
        return Mapper.ToProduct(connectionHandler.ExecuteQuery(cmd).Rows[0]);
    }




    #endregion
}
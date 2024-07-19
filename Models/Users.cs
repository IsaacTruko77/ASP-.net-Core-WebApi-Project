using System;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Protobuf.WellKnownTypes;


public class Users
{
    #region Properties
    
    public int User_Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string User_Role { get; set; } = string.Empty; 
    public Timestamp RegisteredAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
    public bool Active { get; set; } = true;
    public bool Suscribed { get; set; } = false;

    #endregion

}


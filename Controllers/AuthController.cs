using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SysCribBackend.Helpers;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MySqlConnectionHandler _dbHandler;

    public AuthController(MySqlConnectionHandler dbHandler)
    {
        _dbHandler = dbHandler;
    }

    private async Task<string> HashPasswordAsync(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = await Task.Run(() => sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public async Task<bool> VerifyPasswordAsync(int userId, string password)
    {
        string passwordHash = await HashPasswordAsync(password);
        var getPasswordCommand = new MySqlCommand(@"
            SELECT hash_password
            FROM UserPasswords
            WHERE user_id = @UserId;");
        getPasswordCommand.Parameters.Add("@UserId", MySqlDbType.Int32).Value = userId;
        
        try
        {
            var result = await _dbHandler.ExecuteScalarAsync(getPasswordCommand);
            if (result != null)
            {
                string storedPasswordHash = result.ToString();
                return storedPasswordHash == passwordHash;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar la contraseña: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        return false;
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdatePasswordAsync(int userId, string password)
    {
        string passwordHash = await HashPasswordAsync(password);
        var upsertPasswordCommand = new MySqlCommand(@"
            INSERT INTO UserPasswords (user_id, hash_password)
            VALUES (@UserId, @PasswordHash)
            ON DUPLICATE KEY UPDATE
            hash_password = VALUES(hash_password);");
        upsertPasswordCommand.Parameters.Add("@UserId", MySqlDbType.Int32).Value = userId;
        upsertPasswordCommand.Parameters.Add("@PasswordHash", MySqlDbType.VarChar).Value = passwordHash;
        
        try
        {
            await _dbHandler.ExecuteNonQueryAsync(upsertPasswordCommand);
            return Ok(new { message = "Credenciales de autenticación insertadas o actualizadas correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al insertar o actualizar las credenciales de autenticación: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return StatusCode(500, $"Error al insertar o actualizar las credenciales de autenticación: {ex.Message}");
        }
    }
}


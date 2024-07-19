using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SysCribBackend.Helpers;
using SysCribBackend.Models;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;


[ApiController]
[Route("api/[controller]")]
public class SignInController : ControllerBase
{
    private readonly MySqlConnectionHandler _dbHandler;
    private readonly AuthController _authController;
    private readonly IConfiguration _configuration;

    public SignInController(MySqlConnectionHandler dbHandler, IConfiguration configuration)
    {
        _dbHandler = dbHandler;
        _configuration = configuration;
        _authController = new AuthController(dbHandler);
    }

    #region Statements

    private static string login = @"
    SELECT u.user_id, u.username, u.first_name, u.last_name, email, u.suscribed, u.user_role, u.datetime, u.active 
    FROM Users u
    WHERE (((u.email = @EmailOrUsername COLLATE utf8mb4_general_ci
    OR u.username = @EmailOrUsername COLLATE utf8mb4_general_ci)
    AND u.suscribed = 1)AND u.active = 1);";

    #endregion

    #region Endpoints

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!ValidateKeys(request))
        {
            return BadRequest("Todos los campos son obligatorios");
        }

        try
        {
            var cmd = new MySqlCommand(login);
            cmd.Parameters.AddWithValue("@EmailOrUsername", request.UsernameorEmail);
            
            var dataTable = _dbHandler.ExecuteQuery(cmd);

            if (dataTable.Rows.Count == 0)
            {
                return Unauthorized("Usuario no encontrado");
            }

            var user = UsersMapper.ToUser(dataTable.Rows[0]);

            bool isPasswordValid = await _authController.VerifyPasswordAsync(user.User_Id, request.Password);

            if (!isPasswordValid)
            {
                return Unauthorized("Contraseña incorrecta");
            }

            var typeUser = new
            {
                user.User_Id,
                user.UserName,
                user.User_Role,
            };

            var token = GenerateJwtToken(user);

            return Ok(new { User = typeUser,Token = token, Status = 0 });
        }
        catch (Exception ex)
        {
            return Ok(new { Status = 1, Message = ex.Message });
        }
    }

    #endregion

    #region Helpers

    private bool ValidateKeys(LoginRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.UsernameorEmail) &&
                !string.IsNullOrWhiteSpace(request.Password);
    }

    #endregion

    #region Tokens

    private string GenerateJwtToken(Users user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.User_Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.User_Role),
            }),
            Expires = DateTime.UtcNow.AddHours(1), 
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JWT:Issuer"],
            Audience = _configuration["JWT:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion
}


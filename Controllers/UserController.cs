using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SysCribBackend.Models;
using SysCribBackend.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    #region Statements

    private readonly string get = "SELECT user_id, username, first_name, last_name, email, user_role, datetime, active, suscribed FROM Users WHERE active = 1;";
    private readonly string getOne = "SELECT user_id, username, first_name, last_name, email, user_role, datetime, active, suscribed FROM Users WHERE user_id = @UserId;";
    private readonly string disable = "UPDATE Users SET active = 0 WHERE user_id = @UserId;";
    private readonly string insertOrUpdateUser = @"
        INSERT INTO Users (user_id, username, first_name, last_name, email, user_role) 
        VALUES (@UserId, @UserName, @FirstName, @LastName, @Email, @Type)
        ON DUPLICATE KEY UPDATE
            username = VALUES(username),
            first_name = VALUES(first_name),
            last_name = VALUES(last_name),
            email = VALUES(email),
            user_role = VALUES(user_role);";


    #endregion

    #region Constructors
    
    private readonly MySqlConnectionHandler _dbHandler;
    private readonly AuthController _authController;

    public UsersController(MySqlConnectionHandler dbHandler)
    {
        _dbHandler = dbHandler;
        _authController = new AuthController(dbHandler);
    }

    #endregion

    #region Methods

    private bool ValidateKeys(CreateUserRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.UserName) &&
                !string.IsNullOrWhiteSpace(request.FirstName) &&
                !string.IsNullOrWhiteSpace(request.LastName) &&
                !string.IsNullOrWhiteSpace(request.Email) &&
                !string.IsNullOrWhiteSpace(request.Password);
    }

    #endregion

    [HttpGet]
    public ActionResult GetUsers()
    {
        MySqlCommand cmd = new MySqlCommand(get);
        var dataTable = _dbHandler.ExecuteQuery(cmd);
        var users = UsersMapper.ToUserList(dataTable);

        var formattedUsers = users.Select(user => new
        {
            user.User_Id,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Suscribed,
            user.User_Role,
            RegisteredAt = user.RegisteredAt.ToFormattedDate(),
            user.Active
        });

        return Ok(formattedUsers);
    }

    [HttpGet("{id}")]
    public ActionResult GetUser(int id)
    {
        var cmd = new MySqlCommand(getOne);
        cmd.Parameters.AddWithValue("@UserId", id);
        var dataTable = _dbHandler.ExecuteQuery(cmd);

        if (dataTable.Rows.Count == 0)
        {
            // User not found
            Console.WriteLine("User not found.");
            return NotFound();
        }
        var user = UsersMapper.ToUser(dataTable.Rows[0]);

        var formattedUser = new
        {
            user.User_Id,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Suscribed,
            user.User_Role,
            RegisteredAt = user.RegisteredAt.ToFormattedDate(),
            user.Active
        };

        return Ok(formattedUser);
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrUpdateUser([FromBody] CreateUserRequest request)
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
            bool success = await _dbHandler.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                
                var upsertUserCommand = new MySqlCommand(insertOrUpdateUser, connection, transaction);
                upsertUserCommand.Parameters.Add("@UserId", MySqlDbType.Int32).Value = request.Id;
                upsertUserCommand.Parameters.Add("@UserName", MySqlDbType.VarChar).Value = request.UserName;
                upsertUserCommand.Parameters.Add("@FirstName", MySqlDbType.VarChar).Value = request.FirstName;
                upsertUserCommand.Parameters.Add("@LastName", MySqlDbType.VarChar).Value = request.LastName;
                upsertUserCommand.Parameters.Add("@Email", MySqlDbType.VarChar).Value = request.Email;
                upsertUserCommand.Parameters.Add("@Type", MySqlDbType.VarChar).Value = request.Type;
                upsertUserCommand.Parameters.Add("@Active", MySqlDbType.Bit).Value = true;
                
                await upsertUserCommand.ExecuteNonQueryAsync();
                
                int userId = request.Id;
                if (request.Id == 0)
                {
                    var lastInsertedIdCommand = new MySqlCommand("SELECT LAST_INSERT_ID();", connection, transaction);
                    userId = Convert.ToInt32(await lastInsertedIdCommand.ExecuteScalarAsync());
                }
                
                if (!string.IsNullOrEmpty(request.Password))
                {
                    var hashedPassword = await HashPasswordAsync(request.Password);
                    var addPasswordCommand = new MySqlCommand(@"
                        INSERT INTO UserPasswords (user_id, hash_password)
                        VALUES (@UserId, @PasswordHash)
                        ON DUPLICATE KEY UPDATE
                        hash_password = VALUES(hash_password);", connection, transaction);
                    addPasswordCommand.Parameters.Add("@UserId", MySqlDbType.Int32).Value = userId;
                    addPasswordCommand.Parameters.Add("@PasswordHash", MySqlDbType.VarChar).Value = hashedPassword;
                    
                    await addPasswordCommand.ExecuteNonQueryAsync();
                }

                return true;
            });

            if (success)
            {
                var upsertedUser = new
                {
                    Id = request.Id == 0 ? (await GetLastInsertedId()) : request.Id,
                    request.UserName,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Type,
                    Active = true
                };
                
                return request.Id == 0
                    ? CreatedAtAction(nameof(GetUser), new { id = upsertedUser.Id }, upsertedUser)
                    : Ok(upsertedUser);
            }
            else
            {
                return StatusCode(500, "An error occurred while creating or updating the user.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while creating or updating the user. Error: {ex.Message}");
        }
    }


    private async Task<int> GetLastInsertedId()
    {
        var cmd = new MySqlCommand("SELECT LAST_INSERT_ID();");
        var result = await _dbHandler.ExecuteScalarAsync(cmd);
        return Convert.ToInt32(result);
    }

    private async Task<string> HashPasswordAsync(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = await Task.Run(() => sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    [HttpPut("{id}")]
    
    public ActionResult DisableUser(int id)
    {
        try
        {
            var cmd = new MySqlCommand(disable);
            cmd.Parameters.AddWithValue("@UserId", id);
            _dbHandler.ExecuteNonQuery(cmd);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while disabling the user. Error: {ex.Message}");
        }
    }
}


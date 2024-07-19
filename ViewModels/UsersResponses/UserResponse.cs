using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class UserResponse : JsonResponse
{

    public Users User { get; set; }


    public static UserResponse Get(Users user)
    {
        UserResponse response = new UserResponse();
        response.Status = 0;
        response.User = user;
        return response;
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class UserListResponse : JsonResponse
{

    public List<UserResponse> Users { get; set; }


    public static UserListResponse Get(List<Users> users)
    {
        UserListResponse response = new UserListResponse();
        response.Status = 1;
        response.Users = users.Select(user => UserResponse.Get(user)).ToList();
        return response;
    }
    
}

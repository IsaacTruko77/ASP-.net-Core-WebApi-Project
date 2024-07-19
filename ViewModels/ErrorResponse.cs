using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class ErrorResponse : JsonResponse
{
    public string ErrorMessage { get; set; }

    public static ErrorResponse Get(int status,string message)
    {
        ErrorResponse response = new ErrorResponse();
        response.Status = 0;
        response.ErrorMessage = message;
        return response;
    }
}

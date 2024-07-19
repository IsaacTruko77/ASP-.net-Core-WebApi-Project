using System;
using System;
using System.Collections.Generic;
using System.Text;


//RecordNotFoundException is an Exception (inherits)
public class RecordNotFoundException : Exception
{
    //Constructor
    private string _message;

    public override string Message => _message;
    public RecordNotFoundException(string table, string id)
    {
        _message = "Could not find record: " + table + " with id " + id;
    }
} 
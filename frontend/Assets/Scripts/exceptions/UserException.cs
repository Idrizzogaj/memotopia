using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserException : Exception
{
    public UserException(string message) : base(message){}
}

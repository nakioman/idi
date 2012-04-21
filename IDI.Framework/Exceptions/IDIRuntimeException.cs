using System;

namespace IDI.Framework.Exceptions
{
    public class IDIRuntimeException : Exception
    {
         public IDIRuntimeException(string message) : base(message) {}
    }
}
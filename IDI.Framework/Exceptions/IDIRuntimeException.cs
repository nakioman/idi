using System;
using System.ComponentModel.Composition;
using log4net;

namespace IDI.Framework.Exceptions
{
    public class IDIRuntimeException : Exception
    {
        [Import]
        private ILog log;

        public IDIRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
            log.Fatal(message, innerException);
        }


    }
}
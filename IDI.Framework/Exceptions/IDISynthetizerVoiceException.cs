using System;
using System.ComponentModel.Composition;
using log4net;

namespace IDI.Framework.Exceptions
{
    public class IDISynthetizerVoiceException : Exception
    {
        [Import] private ILog _log;

         public IDISynthetizerVoiceException(string message, Exception innerException) : base(message,innerException)
         {
             _log.Fatal(message, innerException);
         }
    }
}
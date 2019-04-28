using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf
{
    public enum ErrorCode
    {
        UNKNOWN = 0,
        BAD_FORMAT,
        WRONG_HMAC,
        UNSUPPORTED,
        OUT_OF_RANGE,
        BAD_ARGUMENT
    }

    public class KdfException : Exception
    {
        public ErrorCode Code { get; private set; } = ErrorCode.UNKNOWN;

        public string ErrorMessage { get { return $"{Code.ToString()}: {Message}"; } }

        public KdfException( string message ) : base(message)
        {
        }

        public KdfException(string message, Exception innerException) : base( message, innerException)
        {
        }

        public KdfException( ErrorCode code, string message ) : base( message )
        {
            Code = code;
        }

        public KdfException( ErrorCode code, string message, Exception innerException ) : base( message, innerException )
        {
            Code = code;
        }
    }
}

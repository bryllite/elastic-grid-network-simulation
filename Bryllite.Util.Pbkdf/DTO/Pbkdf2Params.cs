using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf.DTO
{
    public class Pbkdf2Params : KdfParams
    {
        public const string HMACSHA256 = "hmac-sha256";

        public int c = 262144;
        public string prf = HMACSHA256;

        public static Pbkdf2Params Default
        {
            get { return new Pbkdf2Params(); }
        }
    }
}

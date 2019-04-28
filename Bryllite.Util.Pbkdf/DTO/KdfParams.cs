using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf.DTO
{
    public class KdfParams
    {
        public string salt = RndProvider.GetNonZeroBytes(32).ToHex();
        public int dklen = 32;
    }
}

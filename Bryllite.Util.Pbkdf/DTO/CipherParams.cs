using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf.DTO
{
    public class CipherParams
    {
        public string iv = RndProvider.GetNonZeroBytes(16).ToHex();
    }
}

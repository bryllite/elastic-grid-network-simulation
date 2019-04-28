using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf.DTO
{
    public class ScryptParams : KdfParams
    {
        public int n = 262144;
        public int p = 1 ;
        public int r = 8;

        public static ScryptParams Default
        {
            get { return new ScryptParams(); }
        }
    }
}

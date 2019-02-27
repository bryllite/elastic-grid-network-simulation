using BrylliteLib.Utils;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Crypto.Secp256k1
{
    public abstract class ECKey
    {
        public static readonly string CURVE_NAME = "secp256k1";
        public static readonly string SECP256K1_PRIME = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F";
        public static readonly int PRIVATE_KEY_BYTES = 32;
        public static readonly int PRIVATE_KEY_LENGTH = PRIVATE_KEY_BYTES * 2;

        internal static readonly X9ECParameters secp256k1;
        internal static readonly BigInteger HALF_CURVE_ORDER;
        internal static readonly BigInteger CURVE_ORDER;
        internal static readonly ECDomainParameters mDomainParameter;

        protected readonly ECKeyParameters mKey;


        public ECDomainParameters DomainParameter
        {
            get
            {
                return mDomainParameter;
            }
        }

        static ECKey()
        {
            secp256k1 = SecNamedCurves.GetByName(CURVE_NAME);
            mDomainParameter = new ECDomainParameters(secp256k1.Curve, secp256k1.G, secp256k1.N, secp256k1.H);
            HALF_CURVE_ORDER = secp256k1.N.ShiftRight(1);
            CURVE_ORDER = secp256k1.N;
        }

        public ECKey(byte[] bytesKey, bool isPrivate)
        {
            if (isPrivate)
                mKey = new ECPrivateKeyParameters(new BigInteger(1, bytesKey), DomainParameter);
            else mKey = new ECPublicKeyParameters("EC", secp256k1.Curve.DecodePoint(bytesKey), DomainParameter);
        }

        public static byte[] NewPrivateKeyAsBytes()
        {
            byte[] bytesPrivateKey = RndGenerator.GetNonZeroBytes(PRIVATE_KEY_BYTES);
            return IsValidPrivateKey(bytesPrivateKey) ? bytesPrivateKey : NewPrivateKeyAsBytes();
        }

        public static bool IsValidPrivateKey(byte[] bytesPrivateKey)
        {
            if (bytesPrivateKey == null || bytesPrivateKey.Length != PRIVATE_KEY_BYTES) return false;

            BigInteger keyValue = new BigInteger(1, bytesPrivateKey);
            return keyValue.CompareTo(BigInteger.Zero) > 0 && keyValue.CompareTo(secp256k1.N) < 0;
        }

        public static bool IsValidPrivateKey(string privateKey)
        {
            return IsValidPrivateKey(privateKey.HexToByteArray());
        }
    }
}

using System;

namespace Bryllite.Core.Crypto
{
    public class Secp256k1
    {
        public static readonly int PRIVATE_KEY_BYTES = Secp256k1Net.Secp256k1.PRIVKEY_LENGTH;
        public static readonly int PUBLIC_KEY_BYTES = Secp256k1Net.Secp256k1.PUBKEY_LENGTH;
        public static readonly int SIGNATURE_BYTES = 1 + Secp256k1Net.Secp256k1.SIGNATURE_LENGTH;

        // SECP256k1.Net
        private static Secp256k1Net.Secp256k1 secp256k1 = new Secp256k1Net.Secp256k1();

        public static bool SecretKeyVerify(byte[] secretKey)
        {
            lock (secp256k1)
            {
                return secp256k1.SecretKeyVerify(secretKey);
            }
        }

        public static byte[] PublicKeyCreate(byte[] secretKey)
        {
            byte[] pubKey = new byte[PUBLIC_KEY_BYTES];
            lock (secp256k1)
            {
                return secp256k1.PublicKeyCreate(pubKey, secretKey) ? pubKey : null;
            }
        }

        public static byte[] PublicKeySerialize(byte[] pubKey, bool compressed )
        {
            int len = compressed ? Secp256k1Net.Secp256k1.SERIALIZED_COMPRESSED_PUBKEY_LENGTH : Secp256k1Net.Secp256k1.SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH;
            byte[] bytes = new byte[len];
            lock(secp256k1)
            {
                return secp256k1.PublicKeySerialize(bytes, pubKey, compressed ? Secp256k1Net.Flags.SECP256K1_EC_COMPRESSED : Secp256k1Net.Flags.SECP256K1_EC_UNCOMPRESSED ) ? bytes : null;
            }
        }

        public static byte[] PublicKeyParse( byte[] bytes )
        {
            byte[] pubKey = new byte[PUBLIC_KEY_BYTES];
            lock(secp256k1)
            {
                return secp256k1.PublicKeyParse(pubKey, bytes) ? pubKey : null;
            }
        }

        public static byte[] Sign(byte[] messageHash, byte[] secretKey)
        {
            byte[] signature = new byte[SIGNATURE_BYTES - 1];
            lock (secp256k1)
            {
                return secp256k1.Sign(signature, messageHash, secretKey) ? signature : null;
            }
        }

        public static byte[] SignRecoverable(byte[] messageHash, byte[] secretKey)
        {
            byte[] signature = new byte[SIGNATURE_BYTES];
            lock (secp256k1)
            {
                return secp256k1.SignRecoverable(signature, messageHash, secretKey) ? signature : null;
            }
        }

        public static bool Verify(byte[] signature, byte[] messageHash, byte[] pubKey)
        {
            lock (secp256k1)
            {
                return secp256k1.Verify(signature, messageHash, pubKey);
            }
        }

        public static byte[] Recover(byte[] signature, byte[] messageHash)
        {
            byte[] pubKey = new byte[PUBLIC_KEY_BYTES];
            lock (secp256k1)
            {
                return secp256k1.Recover(pubKey, signature, messageHash) ? pubKey : null;
            }
        }
    }
}

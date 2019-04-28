using Bryllite.Core.Hash;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf.Crypto
{
    public class PbkdfCrypt
    {
        internal static byte[] GenerateDerivedScryptKey(string password, byte[] salt, int n, int r, int p, int dkLen, bool checkRandN = false)
        {
            if (checkRandN)
            {
                //The test vectors of Ethereum provides a cost bigger than the Scrypt spec of "N = less than 2 ^ (128 * r / 8)"
                //so we allow to do validation in general for encryption but not for decryption.
                if (r == 1 && n >= 65536)
                    throw new KdfException(ErrorCode.OUT_OF_RANGE, "Cost parameter N must be > 1 and < 65536");
            }

            return Scrypt.CryptoScrypt(GetPasswordAsBytes(password), salt, n, r, p, dkLen);
        }


        internal static byte[] GenerateCipherKey(byte[] derivedKey)
        {
            var cipherKey = new byte[16];
            Array.Copy(derivedKey, cipherKey, 16);
            return cipherKey;
        }

        internal static byte[] GenerateMac(byte[] derivedKey, byte[] cipherText)
        {
            var result = new byte[16 + cipherText.Length];
            Array.Copy(derivedKey, 16, result, 0, 16);
            Array.Copy(cipherText, 0, result, 16, cipherText.Length);

            return SHA3.Hash256(result);
        }

        // http://stackoverflow.com/questions/34950611/how-to-create-a-pbkdf2-sha256-password-hash-in-c-sharp-bouncy-castle//
        internal static byte[] GeneratePbkdf2Sha256DerivedKey(string password, byte[] salt, int count, int dklen)
        {
            var pdb = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            pdb.Init(PbeParametersGenerator.Pkcs5PasswordToUtf8Bytes(password.ToCharArray()), salt, count);
            var key = (KeyParameter)pdb.GenerateDerivedMacParameters(8 * dklen);
            return key.GetKey();
        }


        internal static byte[] GenerateAesCtrCipher(byte[] iv, byte[] encryptKey, byte[] input)
        {
            // ctr https://gist.github.com/hanswolff/8809275
            var key = ParameterUtilities.CreateKeyParameter("AES", encryptKey);
            var parametersWithIV = new ParametersWithIV(key, iv);
            var cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

            cipher.Init(true, parametersWithIV);
            return cipher.DoFinal(input);
        }

        internal static byte[] GenerateAesCtrDeCipher(byte[] iv, byte[] encryptKey, byte[] input)
        {
            // ctr https://gist.github.com/hanswolff/8809275
            var key = ParameterUtilities.CreateKeyParameter("AES", encryptKey);
            var parametersWithIV = new ParametersWithIV(key, iv);
            var cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

            cipher.Init(false, parametersWithIV);
            return cipher.DoFinal(input);
        }

        internal static byte[] DecryptPbkdf2Sha256(string password, byte[] mac, byte[] iv, byte[] cipherText, int c, byte[] salt,
            int dklen)
        {
            var derivedKey = GeneratePbkdf2Sha256DerivedKey(password, salt, c, dklen);
            return Decrypt(mac, iv, cipherText, derivedKey);
        }

        internal static byte[] DecryptScrypt(string password, byte[] mac, byte[] iv, byte[] cipherText, int n, int p, int r,
            byte[] salt, int dklen)
        {
            var derivedKey = GenerateDerivedScryptKey(password, salt, n, r, p, dklen, false);
            return Decrypt(mac, iv, cipherText, derivedKey);
        }

        internal static byte[] Decrypt(byte[] mac, byte[] iv, byte[] cipherText, byte[] derivedKey)
        {
            ValidateMac(mac, cipherText, derivedKey);
            var encryptKey = new byte[16];
            Array.Copy(derivedKey, encryptKey, 16);
            var privateKey = GenerateAesCtrCipher(iv, encryptKey, cipherText);
            return privateKey;
        }

        internal static void ValidateMac(byte[] mac, byte[] cipherText, byte[] derivedKey)
        {
            var generatedMac = GenerateMac(derivedKey, cipherText);
            if (generatedMac.ToHex() != mac.ToHex())
                throw new KdfException(ErrorCode.WRONG_HMAC, "Cannot derive the same mac as the one provided from the cipher and derived key");
        }

        internal static byte[] GetPasswordAsBytes(string password)
        {
            return Encoding.UTF8.GetBytes(password);
        }
    }
}

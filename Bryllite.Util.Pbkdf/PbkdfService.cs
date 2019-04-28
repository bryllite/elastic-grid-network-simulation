using Bryllite.Core.Key;
using Bryllite.Util.Pbkdf.Crypto;
using Bryllite.Util.Pbkdf.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Pbkdf
{
    public class PbkdfService
    {
        public static readonly int Version = 3;
        public static readonly string CIPHER = "aes-128-ctr";

        public enum KdfType
        {
            pbkdf2,
            scrypt
        }

        public static KdfType GetKdfTypeFromJson( string json )
        {
            try
            {
                var doc = JObject.Parse(json);
                JObject crypto = (JObject)doc.GetValue("crypto", StringComparison.OrdinalIgnoreCase);
                string kdf = crypto.GetValue("kdf", StringComparison.OrdinalIgnoreCase).ToString();

                foreach ( KdfType type in Enum.GetValues(typeof(KdfType)))
                {
                    if (kdf.Equals(type.ToString(), StringComparison.OrdinalIgnoreCase))
                        return type;
                }
            }
            catch( Exception e )
            {
                throw new KdfException(ErrorCode.BAD_FORMAT, "json format exception", e);
            }

            throw new KdfException(ErrorCode.UNSUPPORTED, "unknown kdf");
        }

        public static Address GetAddressFromJson( string json )
        {
            try
            {
                var doc = JObject.Parse(json);
                return new Address(doc.GetValue("address", StringComparison.OrdinalIgnoreCase).ToString());
            }
            catch( Exception e )
            {
                throw new KdfException(ErrorCode.BAD_FORMAT, "address not parsable", e);
            }
        }

        internal static string EncryptKey( PrivateKey key, string password )
        {
            return EncryptKey(key, password, new ScryptParams()).ToJson();
        }

        internal static KeyStoreV3<ScryptParams> EncryptKey( PrivateKey key, string password, ScryptParams kdfParams )
        {
            if (ReferenceEquals(key, null)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "empty key");
            if (string.IsNullOrEmpty(password)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "empty password");

            // random values ( salt, iv )
            var salt = kdfParams.salt;
            var cipherParams = new CipherParams();

            // derivedKey -> cipherKey -> cipherText -> mac
            var derivedKey = PbkdfCrypt.GenerateDerivedScryptKey(password, salt.HexToBytes(), kdfParams.n, kdfParams.r, kdfParams.p, kdfParams.dklen);
            var cipherKey = PbkdfCrypt.GenerateCipherKey(derivedKey);
            var cipherText = PbkdfCrypt.GenerateAesCtrCipher(cipherParams.iv.HexToBytes(), cipherKey, key.Bytes);
            var mac = PbkdfCrypt.GenerateMac(derivedKey, cipherText);

            return new KeyStoreV3<ScryptParams>()
            {
                version = Version,
                id = Guid.NewGuid().ToString(),
                address = key.Address.HexAddress.ToLower(),
                crypto =
                {
                    ciphertext = cipherText.ToHex(),
                    cipherparams = cipherParams,
                    cipher = CIPHER,
                    kdf = KdfType.scrypt.ToString(),
                    kdfparams = kdfParams,
                    mac = mac.ToHex()
                }
            };
        }

        internal static KeyStoreV3<Pbkdf2Params> EncryptKey( PrivateKey key, string password, Pbkdf2Params kdfParams )
        {
            if (ReferenceEquals(key, null)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "empty key");
            if (string.IsNullOrEmpty(password)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "empty password");

            // unsupported prf
            if (kdfParams.prf != Pbkdf2Params.HMACSHA256) throw new KdfException(ErrorCode.UNSUPPORTED, $"unsupported kdfparams.prf:{kdfParams.prf}");

            // random values ( salt, iv )
            var salt = kdfParams.salt;
            var cipherParams = new CipherParams();

            // derivedKey -> cipherKey -> cipherText -> mac
            var derivedKey = PbkdfCrypt.GeneratePbkdf2Sha256DerivedKey(password, salt.HexToBytes(), kdfParams.c, kdfParams.dklen);
            var cipherKey = PbkdfCrypt.GenerateCipherKey(derivedKey);
            var cipherText = PbkdfCrypt.GenerateAesCtrCipher(cipherParams.iv.HexToBytes(), cipherKey, key.Bytes);
            var mac = PbkdfCrypt.GenerateMac(derivedKey, cipherText);

            return new KeyStoreV3<Pbkdf2Params>()
            {
                version = Version,
                id = Guid.NewGuid().ToString(),
                address = key.Address.HexAddress.ToLower(),
                crypto =
                {
                    ciphertext = cipherText.ToHex(),
                    cipherparams = cipherParams,
                    cipher = CIPHER,
                    kdf = KdfType.pbkdf2.ToString(),
                    kdfparams = kdfParams,
                    mac = mac.ToHex()
                }
            };
        }

        internal static PrivateKey DecryptKey( string json, string password )
        {
            KdfType kdf;
            try
            {
                kdf = GetKdfTypeFromJson(json);
            }
            catch( Exception e )
            {
                throw new KdfException(ErrorCode.BAD_FORMAT, "kdf not parsable", e);
            }

            if (kdf == KdfType.scrypt)
                return DecryptKey(JsonConvert.DeserializeObject<KeyStoreV3<ScryptParams>>(json), password);

            if (kdf == KdfType.pbkdf2)
                return DecryptKey(JsonConvert.DeserializeObject<KeyStoreV3<Pbkdf2Params>>(json), password);

            throw new KdfException($"unknown kdf:{kdf}");
        }

        internal static PrivateKey DecryptKey( KeyStoreV3<ScryptParams> keyStore, string password )
        {
            if (string.IsNullOrEmpty(password)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "empty password");
            if (ReferenceEquals(keyStore, null)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "keyStore is null");

            var crypto = keyStore.crypto;
            var kdfparams = keyStore.crypto.kdfparams;

            // unsupported cipher
            if (crypto.cipher != CIPHER) throw new KdfException(ErrorCode.UNSUPPORTED, $"unsupported cipher:{crypto.cipher}");

            var key = PbkdfCrypt.DecryptScrypt(password, crypto.mac.HexToBytes(),
                crypto.cipherparams.iv.HexToBytes(),
                crypto.ciphertext.HexToBytes(),
                kdfparams.n, kdfparams.p, kdfparams.r,
                kdfparams.salt.HexToBytes(), kdfparams.dklen);

            return new PrivateKey(key);
        }

        internal static PrivateKey DecryptKey( KeyStoreV3<Pbkdf2Params> keyStore, string password )
        {
            if (string.IsNullOrEmpty(password)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "empty password");
            if (ReferenceEquals(keyStore, null)) throw new KdfException(ErrorCode.BAD_ARGUMENT, "keyStore is null");

            var crypto = keyStore.crypto;
            var kdfparams = keyStore.crypto.kdfparams;

            // unsupported cipher
            if (crypto.cipher != CIPHER) throw new KdfException(ErrorCode.UNSUPPORTED, $"unsupported cipher:{crypto.cipher}");

            // unsupported prf
            if (kdfparams.prf != Pbkdf2Params.HMACSHA256) throw new KdfException(ErrorCode.UNSUPPORTED, $"unsupported kdfparams.prf:{kdfparams.prf}");

            var key = PbkdfCrypt.DecryptPbkdf2Sha256(password, crypto.mac.HexToBytes(),
                crypto.cipherparams.iv.HexToBytes(),
                crypto.ciphertext.HexToBytes(),
                kdfparams.c, kdfparams.salt.HexToBytes(), kdfparams.dklen);

            return new PrivateKey(key);
        }
    }
}

using Bryllite.Core.Key;
using Bryllite.Util.Log;
using Bryllite.Util.Pbkdf.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Bryllite.Util.Pbkdf
{
    public class KeyStoreServiceProvider
    {
        public static ILoggable blog = BLog.Global;

        public KeyStoreServiceProvider()
        {
        }

        public static string GetKeyStoreFileNameFor( string address )
        {
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException(nameof(address));
            return "UTC--" + DateTime.UtcNow.ToString("0").Replace(":", "-") + "--" + address.Replace("0x", "") + ".json";
        }

        /// <summary>
        /// Encrypt PrivateKey to KeyStoreV3 json string with password using Scrypt
        /// </summary>
        /// <exception cref="KdfException"></exception>
        public static string EncryptKeyStoreV3AsJson( PrivateKey key, string password, ScryptParams kdfParams )
        {
            try
            {
                return PbkdfService.EncryptKey(key, password, kdfParams).ToJson();
            }
            catch (KdfException e)
            {
                blog.error($"WrongHmacException! e={e.ErrorMessage}");
                throw;
            }
        }

        /// <summary>
        /// Encrypt PrivateKey to KeyStoreV3 json string with password using Pbkdf2
        /// </summary>
        /// <exception cref="KdfException"></exception>
        public static string EncryptKeyStoreV3AsJson( PrivateKey key, string password, Pbkdf2Params kdfParams )
        {
            try
            {
                return PbkdfService.EncryptKey(key, password, kdfParams).ToJson();
            }
            catch (KdfException e)
            {
                blog.error($"WrongHmacException! e={e.ErrorMessage}");
                throw;
            }
        }

        /// <summary>
        /// Encrypt PrivateKey to KeyStoreV3 json string with password
        /// </summary>
        /// <exception cref="KdfException"></exception>
        public static string EncryptKeyStoreV3AsJson( PrivateKey key, string password )
        {
            return EncryptKeyStoreV3AsJson(key, password, ScryptParams.Default);
        }

        /// <summary>
        /// Decrypt PrivateKey from KeyStoreV3 json string with password
        /// </summary>
        /// <exception cref="KdfException"></exception>
        public static PrivateKey DecryptKeyStoreV3( string json, string password )
        {
            try
            {
                return PbkdfService.DecryptKey(json, password);
            }
            catch( KdfException e )
            {
                blog.error($"Exception! e={e.ErrorMessage}");
                throw;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Bryllite.Util.Payloads
{
    public class BsonConverter
    {
        public static byte[] ToBytes<T>(T value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    new JsonSerializer().Serialize(writer, value);
                    return ms.ToArray();
                }
            }
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BsonDataReader reader = new BsonDataReader(ms))
                {
                    return new JsonSerializer().Deserialize<T>(reader);
                }
            }
        }
    }
}

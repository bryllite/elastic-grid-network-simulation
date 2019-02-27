using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrylliteLib.Utils
{
    public class BsonConverter
    {
        public static byte[] ToBytes<T>(T value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BsonDataWriter writer = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, value);

                return ms.ToArray();
            }
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}

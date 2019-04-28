using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bryllite.Core.Hash;
using Bryllite.Core.Key;
using Bryllite.Util;
using Bryllite.Util.Payloads;
using Newtonsoft.Json.Linq;

namespace Bryllite.Net.Messages
{
    public class Message : IMessage
    {
        public static readonly string MessageVersion = "1.0";

        public static readonly string HEADER = "header";
        public static readonly string VERSION = "_ver";
        public static readonly string SIGNATURE = "_signature";

        public static readonly string BODY = "body";
        public static readonly string SENDER = "_sender";
        public static readonly string TIMESTAMP = "_timestamp";

        protected Payload headers;
        protected Payload body;

        public Payload Headers => headers;
        public Payload Body => body;

        public Signature Signature => Signature.FromBytes(headers.Value<byte[]>(SIGNATURE));

        public Address Sender
        {
            get
            {
                return Signature.ToPublicKey(body.Hash).Address;
            }
        }

        public long TimeStamp => Value<long>(TIMESTAMP);

        public int Version => headers.Value<int>(VERSION);

        public byte[] Hash => HashProvider.Hash256(ToBytes());

        public int Length => ToBytes().Length;

        public string ID => body.Hash.ToHex();

        protected Message()
        {
            headers = new Payload.Builder().Build();
            body = new Payload.Builder().Build();
        }

        protected Message(Message message)
        {
            headers = Payload.FromObject(message.Headers);
            body = Payload.FromObject(message.Body);
        }

        protected Message(Payload header, Payload body)
        {
            this.headers = header;
            this.body = body;
        }

        public bool Verify(Address address)
        {
            return Sender == address;
        }

        public bool Verify(string address)
        {
            return Verify(new Address(address));
        }

        public bool Verify()
        {
            return Verify(Value<string>(SENDER));
        }

        public override string ToString()
        {
            return ToPayload().ToString();
            
        }

        protected Payload ToPayload()
        {
            return new Payload.Builder()
                .Value(HEADER, headers)
                .Value(BODY, body)
                .Build();
        }

        public byte[] ToBytes()
        {
            return ToPayload().ToBytes();
        }

        public static Message Parse(byte[] bytes)
        {
            Payload payload = Payload.Parse(bytes);
            return new Message(payload.Value<Payload>(HEADER), payload.Value<Payload>(BODY));
        }

        public static Message Parse(byte[] bytes, int offset)
        {
            return Parse(offset > 0 ? bytes.Skip(offset).ToArray() : bytes);
        }

        // json string 으로 Parse 한 메세지는 키 인증에 사용할 수 없다.
        // JObject가 unordered set 이므로, 내용의 hash값이 변한다.
        //public static Message Parse(string json)
        //{
        //    Payload payload = Payload.Parse(json);
        //    return new Message(payload.Value<Payload>(HEADER), payload.Value<Payload>(BODY));
        //}

        public T Value<T>(string name)
        {
            return body.Value<T>(name);
        }

        public T Value<T>(string name, T defaultValue)
        {
            return body.Value(name, defaultValue);
        }

        public T Header<T>(string name)
        {
            return headers.Value<T>(name);
        }

        public T Header<T>(string name, T defaultValue)
        {
            return headers.Value(name, defaultValue);
        }


        public class Builder
        {
            private Message message;

            public Builder()
            {
                message = new Message();
            }

            public Builder(Message other)
            {
                message = new Message(other);
            }

            public Builder WithHeader(Payload header)
            {
                message.headers = header;
                return this;
            }

            public Builder WithBody(Payload body)
            {
                message.body = body;
                return this;
            }

            public Builder Header(string name, JObject value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Header(string name, JValue value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Header(string name, JToken value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Header(string name, JArray value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Header(string name, object value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Header(string name, object[] value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Header(string name, string[] value)
            {
                message.Headers.Set(name, value);
                return this;
            }

            public Builder Body(string name, JObject value)
            {
                message.body.Set(name, value);
                return this;
            }

            public Builder Body(string name, JValue value)
            {
                message.Body.Set(name, value);
                return this;
            }

            public Builder Body(string name, JToken value)
            {
                message.Body.Set(name, value);
                return this;
            }

            public Builder Body(string name, object value)
            {
                message.Body.Set(name, value);
                return this;
            }

            public Builder Body(string name, object[] value)
            {
                message.Body.Set(name, value);
                return this;
            }

            public Builder Body(string name, string[] value)
            {
                message.Body.Set(name, value);
                return this;
            }

            public Message Build(PrivateKey key)
            {
                // add timestamp to body
                message.body.Set(TIMESTAMP, DateTime.UtcNow.Ticks);

                // sender address
                message.body.Set(SENDER, key.Address.HexAddress);

                // fill header with key sign for body
                message.headers.Set(VERSION, MessageVersion);
                message.headers.Set(SIGNATURE, key.Sign(message.body.Hash).ToBytes());

                return message;
            }
        }

    }
}

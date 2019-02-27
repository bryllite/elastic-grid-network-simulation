using BrylliteLib.Crypto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Net.Elastic
{
    // P2P Message for Elastic Networking
    public class ElasticMessage
    {
        public static readonly string TO = "To";
        public static readonly string BODY = "Body";
        public static readonly string FROMADDRESS = "FromAddress";
        public static readonly string FROMSIGN = "FromSign";

        // 엘라스틱 목적지 좌표
        public ElasticCoordinates To;

        // 전송자
        public CAddress FromAddress;

        // 전송사 서명
        public CSignature FromSign;

        // 전송 내용 ( 바디 )
        public Payload Body;


        public ElasticMessage()
        {
        }

        public ElasticMessage(Payload body, CPrivateKey key)
        {
            Body = body;

            Sign(key);
        }

        private ElasticMessage(byte[] bytes)
        {
            FromBytes(bytes);
        }

        private void FromBytes(byte[] bytes)
        {
            Payload p = Payload.Parse(bytes);

            if (p.ContainsKey(TO)) To = p.Get<byte[]>(TO);
            if (p.ContainsKey(FROMADDRESS)) FromAddress = p.Get<byte[]>(FROMADDRESS);
            if (p.ContainsKey(BODY)) Body = p.Get<Payload>(BODY);
            if (p.ContainsKey(FROMSIGN)) FromSign = p.Get<byte[]>(FROMSIGN);
        }

        public byte[] ToBytes()
        {
            Payload p = new Payload();

            if (To != null) p.Set(TO, To);
            if (FromAddress != null) p.Set(FROMADDRESS, FromAddress);
            if (Body != null) p.Set(BODY, Body);
            if (FromSign != null) p.Set(FROMSIGN, FromSign);

            return p.ToByteArray();
        }

        public static ElasticMessage Parse(byte[] bytes)
        {
            return new ElasticMessage(bytes);
        }

        public void Sign(CPrivateKey key)
        {
            FromAddress = key.Address;
            FromSign = key.Sign(Body.Hash);
        }

        public bool Verify()
        {
            CPublicKey key = FromSign.ToPublicKey(Body.Hash);
            return FromAddress.Equals(key.Address);
        }
    }
}

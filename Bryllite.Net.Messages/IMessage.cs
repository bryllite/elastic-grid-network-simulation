using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Core.Key;
using Bryllite.Util.Payloads;

namespace Bryllite.Net.Messages
{
    public interface IMessage
    {
        Payload Headers { get; }
        Payload Body { get; }

        Signature Signature { get; }
        Address Sender { get; }
        int Version { get; }

        byte[] Hash { get; }

        bool Verify(Address address);

        byte[] ToBytes();

        T Header<T>(string name);

        T Value<T>(string name);
    }
}

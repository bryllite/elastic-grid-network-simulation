using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bryllite.Net.Messages;

namespace Bryllite.Net.Elastic
{
    public interface IElasticNode
    {
        Action<ElasticAddress, Message> OnMessage { get; set; }

        void Start( CancellationTokenSource cts );
        void Stop();


        bool SendTo(Message message, ElasticAddress peer);

        int SendTo(Message message, ElasticAddress[] peers);

        void SendAll(Message message);

        void SendAll(Message message, byte n);
    }
}

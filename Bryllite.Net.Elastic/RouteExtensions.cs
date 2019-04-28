using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Core.Key;
using Bryllite.Net.Messages;
using Bryllite.Util;
using Bryllite.Util.Payloads;

namespace Bryllite.Net.Elastic
{
    public static class RouteExtensions
    {
        public static readonly string ROUTES = "routes";
        public static readonly string TTL = "ttl";
        public static readonly string TO = "to";
        public static readonly string LAYOUT = "layout";
        public static readonly string ROUTER = "router";
        public static readonly string ROUTERSIG = "routerSig";

        public static Payload Routes(this Message message)
        {
            try
            {
                return message.Header<Payload>(ROUTES);
            }
            catch (Exception)
            {
                return default(Payload);
            }
        }

        public static Message Routes(this Message message, Payload routes)
        {
            message.Headers.Set(ROUTES, routes);
            return message;
        }


        public static bool VerifyRouter(this Message message)
        {
            try
            {
                return Router(message) == RouterSignature(message).ToPublicKey(message.Body.Hash).Address;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void RouteTo(this Message message, byte ttl, Elastic3D to, ElasticLayout layout, PrivateKey routerKey)
        {
            Payload routes = new Payload.Builder()
                .Value(TTL, ttl)
                .Value(TO, to)
                .Value(LAYOUT, layout)
                .Value(ROUTER, routerKey.Address.HexAddress)
                .Value(ROUTERSIG, routerKey.Sign(message.Body.Hash).ToBytes())
                .Build();

            message.Headers.Set(ROUTES, routes);   
        }

        public static void RouteTo(this Message message, Elastic3D to, ElasticLayout layout, PrivateKey routerKey)
        {
            RouteTo(message, to.TimeToLive(), to, layout, routerKey);
        }

        public static byte TimeToLive(this Elastic3D coordinates)
        {
            return (byte)( coordinates.Z > 0 ? 3 : coordinates.Y > 0 ? 2 : coordinates.X > 0 ? 1 : 0 );
        }

        public static void TimeToLive(this Message message, byte ttl)
        {
            Payload routes = Routes(message);
            if (!ReferenceEquals(routes, null))
            {
                routes.Set(TTL, ttl);
                message.Headers.Set(ROUTES, routes);
            }
        }

        public static byte TimeToLive(this Message message)
        {
            try
            {
                return Routes(message).Value<byte>(TTL);
            }
            catch (Exception)
            {
                return byte.MaxValue;
            }
        }


        public static bool ShouldRoute(this Message message)
        {
            byte ttl = TimeToLive(message);
            return ttl != byte.MaxValue && ttl > 0;
        }


        public static Elastic3D To(this Message message)
        {
            try
            {
                return Routes(message).Value<Elastic3D>(TO);
            }
            catch (Exception)
            {
                return default(Elastic3D);
            }
        }

        public static ElasticLayout Layout(this Message message)
        {
            try
            {
                return Routes(message).Value<ElasticLayout>(LAYOUT);
            }
            catch (Exception)
            {
                return default(ElasticLayout);
            }
        }

        public static Address Router(this Message message)
        {
            try
            {
                return new Address(Routes(message).Value<string>(ROUTER));
            }
            catch (Exception)
            {
                return Address.Empty;
            }
        }

        public static Signature RouterSignature(this Message message)
        {
            try
            {
                return Signature.FromBytes(Routes(message).Value<byte[]>(ROUTERSIG));
            }
            catch (Exception)
            {
                return Signature.Empty;
            }
        }


        public static ElasticAddress Pick(this ElasticAddress[] peers, ElasticAddress me)
        {
            if (peers.Length == 0) return default(ElasticAddress);

            if (peers.Contains(me)) return me;

            return peers[RndProvider.Next(peers.Length)];
        }

        public static bool Contains(this ElasticAddress[] peers, ElasticAddress peer)
        {
            foreach (var p in peers)
                if (peer.Address == p.Address) return true;

            return false;
        }

        public static string Ellipsis(this ElasticAddress peer)
        {
            return Ellipsis(peer, 6, 6);
        }

        public static string Ellipsis(this ElasticAddress peer, int head)
        {
            return Ellipsis(peer, head, 0);
        }

        public static string Ellipsis(this ElasticAddress peer, int head, int tail)
        {
            return $"{ElasticAddress.PREFIX}{peer.HexAddress.Ellipsis(head, tail)}@{peer.Host}:{peer.Port}";
        }
    }
}

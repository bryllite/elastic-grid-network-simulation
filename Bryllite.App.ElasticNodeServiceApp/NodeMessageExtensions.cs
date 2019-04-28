using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Net.Messages;

namespace Bryllite.App.ElasticNodeServiceApp
{
    public static class NodeMessageExtensions
    {
        public const string ACTION = "action";


        public static string Action(this Message message)
        {
            try
            {
                return message.Value<string>(ACTION);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Message.Builder Action(this Message.Builder builder, string action)
        {
            builder.Body(ACTION, action);
            return builder;
        }
    }
}

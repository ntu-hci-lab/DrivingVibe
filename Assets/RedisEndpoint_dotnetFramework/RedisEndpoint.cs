using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisEndpoint
{

    public class RedisEndpoint
    {

        protected static ConfigurationOptions redisConfiguration = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Password = "",
            Ssl = false,
            ConnectTimeout = 6000,
            SyncTimeout = 6000
        };
        protected static ConnectionMultiplexer multiplexer;
        protected ISubscriber connection;

        public RedisEndpoint(string url, ushort port)
        {
            if (multiplexer == null)
            {
                /*
                try
                {
                    redisConfiguration.EndPoints.Add(url, port);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
                */
                redisConfiguration.EndPoints.Add(url, port);
                multiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
                if (!multiplexer.IsConnected)
                {
                    UnityEngine.Debug.Log("not connected");
                }
            }
        }

        static int Main()
        {
            return 0;
        }
    }

    public class Publisher : RedisEndpoint
    {
        public Publisher(string url, ushort port) : base(url, port)
        {
            connection = multiplexer.GetSubscriber();
        }
        public void Publish(string channelName, string msg)
        {
            connection.PublishAsync(channelName, msg, flags: CommandFlags.FireAndForget);
        }
    }

    public class Subscriber : RedisEndpoint
    {
        // public ChannelMessageQueue msgQueue;
        private int _intMsg;
        public int IntMsg => _intMsg;
        public Subscriber(string url, ushort port) : base(url, port)
        {
            _intMsg = 10;
            connection = multiplexer.GetSubscriber();
            if (!connection.IsConnected())
            {
                UnityEngine.Debug.Log("no connection!");
            }
        }
        public void SubscribeTo(string channelName)
        {
            
            // msgQueue = connection.Subscribe(channelName);
            connection.Subscribe(channelName, (channel, value) => 
            {
                _intMsg = int.Parse(value.ToString());
                UnityEngine.Debug.Log(value.ToString());
            });
        }
        /*
        public static void handler(RedisChannel channel, RedisValue value)
        {
            IntMsg = value.TryParse;
        }
        */
    }


}

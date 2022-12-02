using CSRedis;
using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Configuration;

namespace RedisHelp
{
    #region StackExchange connect redis
    public class RedisConnectionHelp
    {
        //系统自定义Key前缀
        public static readonly string SysCustomKey = ConfigurationManager.AppSettings["RedisCustomKey"] ?? "";

        //"127.0.0.1:6379,allowadmin=true
        private static readonly string RedisConnectionString = "p68pfi038u:6379,password=intel@123"; //ConfigurationManager.ConnectionStrings["RedisServer"].ConnectionString;

        private static readonly object Locker = new object();
        private static ConnectionMultiplexer _instance;
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private static ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 单例获取
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            _instance = GetManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 缓存获取
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnectionMultiplexer(string connectionString)
        {
            if (!ConnectionCache.ContainsKey(connectionString))
            {
                ConnectionCache[connectionString] = GetManager(connectionString);
            }
            return ConnectionCache[connectionString];
        }

        private static ConnectionMultiplexer GetManager(string connectionString = null)
        {
            connectionString = connectionString ?? RedisConnectionString;
            var connect = ConnectionMultiplexer.Connect(connectionString);

            //注册如下事件
            connect.ConnectionFailed += MuxerConnectionFailed;
            connect.ConnectionRestored += MuxerConnectionRestored;
            connect.ErrorMessage += MuxerErrorMessage;
            connect.ConfigurationChanged += MuxerConfigurationChanged;
            connect.HashSlotMoved += MuxerHashSlotMoved;
            connect.InternalError += MuxerInternalError;

            return connect;
        }

        #region 事件

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            Logger.Debug($"Configuration changed: {e.EndPoint}");
            Console.WriteLine("Configuration changed: " + e.EndPoint);
        }

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Logger.Error($"ErrorMessage: {e.Message}");
            Console.WriteLine("ErrorMessage: " + e.Message);
        }

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Logger.Debug($"ConnectionRestored: {e.EndPoint}");
            Console.WriteLine("ConnectionRestored: " + e.EndPoint);
        }

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Logger.Error($"reconnet：Endpoint failed: {e.EndPoint} FailureType: {e.FailureType}");
            Console.WriteLine("reconnet：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Logger.Debug($"HashSlotMoved:NewEndPoint - {e.NewEndPoint}, OldEndPoint -{e.OldEndPoint}");
            Console.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            Logger.Error($"InternalError:Message - {e.Exception.Message}");
            Console.WriteLine("InternalError:Message" + e.Exception.Message);
        }

        #endregion 事件
    }
    #endregion

    #region CSRedisCore connect redis
    public class RedisConnect
    {
        //系统自定义Key前缀
        public static readonly string SysCustomKey = ConfigurationManager.AppSettings["RedisCustomKey"] ?? "pre-";

        //redis sentinel
        private static readonly string RedisSentinels = ConfigurationManager.AppSettings["RedisSentinels"] ?? "127.0.0.1:11121;127.0.0.1:11131;127.0.0.1:11141";

        //redis connect string
        private static readonly string RedisConnectionString = ConfigurationManager.AppSettings["RedisConnectString"] ?? "mymaster,password=123456";
        private static readonly object Locker = new object();
        private static CSRedisClient _instance;

        public static CSRedisClient Connect()
        {
            if (_instance == null)
            {
                lock (Locker)
                {
                    if (_instance == null)
                    {
                        _instance = GetConnect();
                        RedisHelper.Initialization(_instance);
                    }
                }
            }
            return _instance;
        }

        private static CSRedisClient GetConnect()
        {
            //return new CSRedis.CSRedisClient("p68pfi038u:6379,password=intel@123,defaultDatabase=0");
            // use when deploy redis sentinels
            return new CSRedisClient(RedisConnectionString, RedisSentinels.Split(';'));
        }
       
    }
    #endregion
}

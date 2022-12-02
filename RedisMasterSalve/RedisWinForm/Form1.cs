using RedisHelp;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedisWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //准备过程
        //1 http://www.cnblogs.com/wudequn/p/8109798.html  页面中有下载主从哨兵配置文件（页面搜索文字   redis哨兵配置下载   ） 并搭redis主从 哨兵。
        //2 运行代码测试

        public static IConnectionMultiplexer RedisConnect;
        public static IDatabase DefaultDB;
        public static ISubscriber sentinelsub;
        private static ConnectionMultiplexer _sentinel;
        private static IRedisClient _redisClient;

        /// <summary>
        /// 创建redis连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //if (DefaultDB == null)
            //{
            //    ConnectRedis();
            //}
            //else
            //{
            //    Console.WriteLine("Have connected redis.");
            //}
            string key = "testkey";
            string value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");

            IRedisClient client = new RedisClient();
            client.Set(key, value);
            client.SAdd<string>(RedisClient.customKeyPre + key, value);


            Console.WriteLine($"write data to master -> {key}:{value}");
            var getValue = client.Get(key);
            
            Console.WriteLine($"get data from master -> {key}:{getValue}");
            if (_redisClient == null)
            {
                _redisClient = client;
                client.Subscribe("ch2", m => {
                    Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().Id.ToString()} subcribe receive message -> {m} {key}:{client.Get(key)} SAll:{client.SMembers(RedisClient.customKeyPre + key)}"); });
            }
        }

        /// <summary>
        /// 监控哨兵主从切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ConfigurationOptions sentineloption = new ConfigurationOptions();
            sentineloption.TieBreaker = "";//sentinel模式一定要写

            //三个哨兵
            sentineloption.EndPoints.Add("127.0.0.1:11121");
            sentineloption.EndPoints.Add("127.0.0.1:11131");
            sentineloption.EndPoints.Add("127.0.0.1:11141");

            //哨兵连接模式
            sentineloption.CommandMap = CommandMap.Sentinel;
            sentineloption.ServiceName = "mymaster";
            //我们可以成功的连接一个sentinel服务器，对这个连接的实际意义在于：当一个主从进行切换后，如果它外层有Twemproxy代理，我们可以在这个时机（+switch-master事件)通知你的Twemproxy代理服务器，并更新它的配置文件里的master服务器的地址，然后从起你的Twemproxy服务，这样你的主从切换才算真正完成。
            //一般没有代理服务器，直接更改从数据库配置文件，将其升级为主数据库。
            _sentinel = ConnectionMultiplexer.Connect(sentineloption);
            sentinelsub = _sentinel.GetSubscriber();

            sentinelsub.Subscribe("+switch-master", (ch, mg) =>
            {
                Console.WriteLine((string)mg);
            });
            Console.WriteLine("Start sentinel process and subscribe switch-master.");
        }

        /// <summary>
        /// 读写测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            //读写数据
            string key = "CustomKey";
            string value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");

            if (DefaultDB == null)
            {
                Console.WriteLine($"We have to connet redis before doing action with redis.");
                ConnectRedis();
            }
            
            //默认写入主库。
            DefaultDB.StringSet(key, value);
            Console.WriteLine($"write data to master -> {key}:{value}");
            Thread.Sleep(2000);
            //CommandFlags.PreferSlave参数表示读取从库数据   貌似是随机你从不同的slave中读取的。


            //关注问题：https://github.com/StackExchange/StackExchange.Redis/issues/593
            //关注问题：https://github.com/StackExchange/StackExchange.Redis/issues/547
            var getvalue = DefaultDB.StringGet(key,CommandFlags.PreferSlave);
            Console.WriteLine($"read data from Slave -> {key}:{getvalue}");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Console.WriteLine("read issue refer to https://github.com/StackExchange/StackExchange.Redis/issues/593");
            Console.WriteLine("read issue refer to https://github.com/StackExchange/StackExchange.Redis/issues/547");
        }

        private void ConnectRedis()
        {
            ConfigurationOptions config = new ConfigurationOptions()
            {
                //是一个列表，一个复杂的的场景中可能包含有主从复制 ， 对于这种情况，只需要指定所有地址在连接字符串中
                //（它将会自动识别出主服务器 set值得时候用的主服务器）假设这里找到了两台主服务器，将会对两台服务进行裁决选出一台作为主服务器
                //来解决这个问题 ， 这种情况是非常罕见的 ，我们也应该避免这种情况的发生。
                //
                EndPoints = { { "127.0.0.1", 11111 }, { "127.0.0.1", 12111 }, { "127.0.0.1", 13111 } }
            };
            //服务器秘密
            config.Password = "123456";
            //客户端名字
            config.ClientName = "127.0.0.1";
            //服务器名字
            config.ServiceName = "127.0.0.1";
            //true表示管理员身份，可以用一些危险的指令。
            config.AllowAdmin = true;
            RedisConnect = ConnectionMultiplexer.Connect(config);
            //_sentinel.GetSentinelMasterConnection(config);
            DefaultDB = RedisConnect.GetDatabase();
            Console.WriteLine("---------------------------Redis Master-Slave Standalone Create---------------------------");
            Console.WriteLine(RedisConnect.GetStatus());
            Console.WriteLine("---------------------------------------------------------------------------");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (_redisClient != null)
            {
                string key = "testkey";
                string value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
                _redisClient.Set(key, value);
                _redisClient.SAdd<string>(RedisClient.customKeyPre + key, value);
                _redisClient.Publish("ch2", $"{System.Diagnostics.Process.GetCurrentProcess().Id.ToString()} send message");
            }
        }
    }
}

using CSRedis;
using NLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RedisHelp
{
    public class RedisClient: IRedisClient
    {
        public static string customKeyPre = RedisConnect.SysCustomKey;
        public RedisClient()
        {
            RedisConnect.Connect();
        }

        public string HGet(string key, string field)
        {
            return RedisHelper.HGet(key, field);
        }
        public Dictionary<string, string> HGetAll(string key)
        {
            return RedisHelper.HGetAll(key);
        }
        public Dictionary<string, T> HGetAll<T>(string key)
        {
            return RedisHelper.HGetAll<T>(key);
        }
        public T HGet<T>(string key, string field)
        {
            return RedisHelper.HGet<T>(key, field);
        }
        public bool HMSet(string key, params object[] values)
        {
            return RedisHelper.HMSet(key, values);
        }
        public bool HSet(string key, string field, object value)
        {
            return RedisHelper.HSet(key, field, value);
        }

        public string Get(string key)
        {
            return RedisHelper.Get(key);
        }

        public T Get<T>(string key)
        {
            return RedisHelper.Get<T>(key);
        }

        public async Task<string> GetAsync(string key)
        {
            return await RedisHelper.GetAsync(key);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            return await RedisHelper.GetAsync<T>(key);
        }

        public long LPush<T>(string key, T value)
        {
            return RedisHelper.LPush(key, value);
        }

        public long Publish<T>(string channel, T message)
        {
            return RedisHelper.Publish(channel, JsonConvert.SerializeObject(message));
        }

        public long RPush<T>(string key, T value)
        {
            return RedisHelper.RPush(key, value);
        }

        public long SAdd<T>(string key, params T[] members)
        {
            return RedisHelper.SAdd(key, members);
        }

        public void Set(string key, object t, int expiresSec = -1)
        {
            RedisHelper.Set(key, t, expiresSec);
        }

        public async Task SetAsync(string key, object t, int expiresSec = -1)
        {
            await RedisHelper.SetAsync(key, t, expiresSec);
        }

        public bool SIsMember(string key, object member)
        {
            return RedisHelper.SIsMember(key, member);
        }

        public string[] SMembers(string key)
        {
            return RedisHelper.SMembers(key);
        }

        public T[] SMembers<T>(string key)
        {
            return RedisHelper.SMembers<T>(key);
        }

        public long SRem<T>(string key, params T[] members)
        {
            return RedisHelper.SRem(key, members);
        }

        public CSRedisClient.SubscribeObject Subscribe(string channel, Action<string> action)
        {
            return RedisHelper.Subscribe((channel, m => { action(m.Body); }));
        }

        public CSRedisClient.SubscribeListBroadcastObject SubscribeListBroadcase(string channel, Action<string> action)
        {
            return RedisHelper.SubscribeListBroadcast(channel, Process.GetCurrentProcess().Id.ToString(), m => action(m));
        }
    }
}

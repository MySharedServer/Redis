using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelp
{
    public interface IRedisClient
    {
        string HGet(string key, string field);
        Dictionary<string, string> HGetAll(string key);
        Dictionary<string, T> HGetAll<T>(string key);
        T HGet<T>(string key, string field);
        bool HMSet(string key, params object[] keyValues);
        bool HSet(string key, string field, object value);
        string Get(string key);
        Task<string> GetAsync(string key);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        long LPush<T>(string key, T value);
        long Publish<T>(string channel, T message);
        long RPush<T>(string key, T value);
        long SAdd<T>(string key, params T[] members);
        void Set(string key, object t, int expiresSec = 0);
        Task SetAsync(string key, object t, int expiresSec = 0);
        bool SIsMember(string key, object member);
        string[] SMembers(string key);
        T[] SMembers<T>(string key);
        long SRem<T>(string key, params T[] members);
        CSRedisClient.SubscribeObject Subscribe(string channel, Action<string> action);
        CSRedisClient.SubscribeListBroadcastObject SubscribeListBroadcase(string channel, Action<string> action);
    }
}

using Himall.Core;
using Himall.Core.Strategies;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Yaouplat.Strategy.Redis
{
	public class Redis : ICache, IStrategy
	{
		private class CacheObject<T>
		{
			public int ExpireTime
			{
				get;
				set;
			}

			public bool ForceOutofDate
			{
				get;
				set;
			}

			public T Value
			{
				get;
				set;
			}
		}

		private int DEFAULT_TMEOUT = 600;

		private string address;

		private JsonSerializerSettings jsonConfig;

		private ConnectionMultiplexer connectionMultiplexer;

		private IDatabase database;

		private ISubscriber sub;

		private Dictionary<string, ISubscriber> subs;

		public int TimeOut
		{
			get
			{
				return this.DEFAULT_TMEOUT;
			}
			set
			{
				this.DEFAULT_TMEOUT = value;
			}
		}

		public Redis()
		{
			JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jss.NullValueHandling = NullValueHandling.Ignore;
			this.jsonConfig = jss;
			this.subs = new Dictionary<string, ISubscriber>();
			this.address = ConfigurationManager.AppSettings["RedisServer"];
			bool flag = this.address == null || string.IsNullOrWhiteSpace(this.address.ToString());
			if (flag)
			{
				throw new ApplicationException("配置文件中未找到RedisServer的有效配置");
			}
			this.connectionMultiplexer = ConnectionMultiplexer.Connect(this.address, null);
			this.database = this.connectionMultiplexer.GetDatabase(-1, null);
			this.sub = this.connectionMultiplexer.GetSubscriber(null);
		}

		public object Get(string key)
		{
			return this.Get<object>(key);
		}

		public T Get<T>(string key)
		{
			DateTime now = DateTime.Now;
			RedisValue value = this.database.StringGet(key, CommandFlags.None);
			DateTime now2 = DateTime.Now;
			T result = default(T);
			bool flag = !value.IsNull;
			if (flag)
			{
				Redis.CacheObject<T> cacheObject = JsonConvert.DeserializeObject<Redis.CacheObject<T>>(value, this.jsonConfig);
				bool flag2 = !cacheObject.ForceOutofDate;
				if (flag2)
				{
					this.database.KeyExpire(key, new TimeSpan?(new TimeSpan(0, 0, cacheObject.ExpireTime)), CommandFlags.None);
				}
				result = cacheObject.Value;
			}
			DateTime now3 = DateTime.Now;
			Log.Debug(string.Concat(new object[]
			{
				"取数据时间:",
				now2.Subtract(now).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
			return result;
		}

		public void Insert(string key, object data)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now.AddSeconds((double)this.TimeOut) - now;
			DateTime now2 = DateTime.Now;
			string jsonData = this.GetJsonData(data, this.TimeOut, false);
			DateTime now3 = DateTime.Now;
			this.database.StringSet(key, jsonData, null, When.Always, CommandFlags.None);
			Log.Debug(string.Concat(new object[]
			{
				"插入数据时间:",
				DateTime.Now.Subtract(now3).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
		}

		public void Insert(string key, object data, int cacheTime)
		{
			DateTime now = DateTime.Now;
			TimeSpan value = TimeSpan.FromSeconds((double)cacheTime);
			DateTime now2 = DateTime.Now;
			string jsonData = this.GetJsonData(data, this.TimeOut, true);
			DateTime now3 = DateTime.Now;
			this.database.StringSet(key, jsonData, new TimeSpan?(value), When.Always, CommandFlags.None);
			Log.Debug(string.Concat(new object[]
			{
				"插入数据时间:",
				DateTime.Now.Subtract(now3).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
		}

		public void Insert(string key, object data, DateTime cacheTime)
		{
			DateTime now = DateTime.Now;
			TimeSpan value = cacheTime - DateTime.Now;
			DateTime now2 = DateTime.Now;
			string jsonData = this.GetJsonData(data, this.TimeOut, true);
			DateTime now3 = DateTime.Now;
			this.database.StringSet(key, jsonData, new TimeSpan?(value), When.Always, CommandFlags.None);
			Log.Debug(string.Concat(new object[]
			{
				"插入数据时间:",
				DateTime.Now.Subtract(now3).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
		}

		public void Insert<T>(string key, T data)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now.AddSeconds((double)this.TimeOut) - now;
			DateTime now2 = DateTime.Now;
			string jsonData = this.GetJsonData<T>(data, this.TimeOut, false);
			DateTime now3 = DateTime.Now;
			this.database.StringSet(key, jsonData, null, When.Always, CommandFlags.None);
			Log.Debug(string.Concat(new object[]
			{
				"插入数据时间:",
				DateTime.Now.Subtract(now3).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
		}

		public void Insert<T>(string key, T data, int cacheTime)
		{
			DateTime now = DateTime.Now;
			TimeSpan value = TimeSpan.FromSeconds((double)cacheTime);
			DateTime now2 = DateTime.Now;
			string jsonData = this.GetJsonData<T>(data, this.TimeOut, true);
			DateTime now3 = DateTime.Now;
			this.database.StringSet(key, jsonData, new TimeSpan?(value), When.Always, CommandFlags.None);
			Log.Debug(string.Concat(new object[]
			{
				"插入数据时间:",
				DateTime.Now.Subtract(now3).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
		}

		public void Insert<T>(string key, T data, DateTime cacheTime)
		{
			DateTime now = DateTime.Now;
			TimeSpan value = cacheTime - DateTime.Now;
			DateTime now2 = DateTime.Now;
			string jsonData = this.GetJsonData<T>(data, this.TimeOut, true);
			DateTime now3 = DateTime.Now;
			this.database.StringSet(key, jsonData, new TimeSpan?(value), When.Always, CommandFlags.None);
			Log.Debug(string.Concat(new object[]
			{
				"插入数据时间:",
				DateTime.Now.Subtract(now3).TotalMilliseconds,
				"毫秒,转JSON时间:",
				now3.Subtract(now2).TotalMilliseconds,
				"毫秒"
			}));
		}

		private string GetJsonData(object data, int cacheTime, bool forceOutOfDate)
		{
			Redis.CacheObject<object> cacheObject = new Redis.CacheObject<object>
			{
				Value = data,
				ExpireTime = cacheTime,
				ForceOutofDate = forceOutOfDate
			};
			return JsonConvert.SerializeObject(cacheObject, this.jsonConfig);
		}

		private string GetJsonData<T>(T data, int cacheTime, bool forceOutOfDate)
		{
			Redis.CacheObject<T> cacheObject = new Redis.CacheObject<T>
			{
				Value = data,
				ExpireTime = cacheTime,
				ForceOutofDate = forceOutOfDate
			};
			return JsonConvert.SerializeObject(cacheObject, this.jsonConfig);
		}

		public void Remove(string key)
		{
			this.database.KeyDelete(key, CommandFlags.HighPriority);
		}

		public bool Exists(string key)
		{
			return this.database.KeyExists(key, CommandFlags.None);
		}

		public void Send(string key, object data)
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now.AddSeconds((double)this.TimeOut) - now;
			string jsonData = this.GetJsonData(data, this.TimeOut, false);
			this.sub.Publish(key, jsonData, CommandFlags.None);
		}

		public void RegisterSubscribe<T>(string key, Cache.DoSub dosub)
		{
			ISubscriber subscriber = this.connectionMultiplexer.GetSubscriber(null);
			subscriber.Subscribe(key, delegate(RedisChannel channel, RedisValue message)
			{
				T t = this.Recieve<T>(message);
				dosub.Invoke(t);
			}, CommandFlags.None);
			foreach (string current in this.subs.Keys)
			{
				bool flag = current == key;
				if (flag)
				{
					return;
				}
			}
			this.subs.Add(key, subscriber);
		}

		public void UnRegisterSubscrib(string key)
		{
			this.sub.Unsubscribe(key, null, CommandFlags.None);
			this.subs.Remove(key);
		}

		private T Recieve<T>(string cachevalue)
		{
			T result = default(T);
			bool flag = !string.IsNullOrEmpty(cachevalue);
			if (flag)
			{
				Redis.CacheObject<T> cacheObject = JsonConvert.DeserializeObject<Redis.CacheObject<T>>(cachevalue, this.jsonConfig);
				result = cacheObject.Value;
			}
			return result;
		}
	}
}

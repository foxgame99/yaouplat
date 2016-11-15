using Enyim.Caching;
using Enyim.Caching.Memcached;
using Himall.Core;
using System;

namespace Yaouplat.Strategy.Memecached
{
	public class Memecached : ICache
	{
		private const int DEFAULT_TMEOUT = 600;

		private int timeout = 600;

		private MemcachedClient client = MemcachedClientService.Instance.Client;

		public int TimeOut
		{
			get
			{
				return this.timeout;
			}
			set
			{
				this.timeout = ((value > 0) ? value : 600);
			}
		}

		public object Get(string key)
		{
			return this.client.Get(key);
		}

		public void Remove(string key)
		{
			this.client.Remove(key);
		}

		public void Clear()
		{
			this.client.FlushAll();
		}

		public void Insert(string key, object data)
		{
			this.client.Store(StoreMode.Add, key, data);
		}

		public void Insert(string key, object data, int cacheTime)
		{
			this.client.Store(StoreMode.Set, key, data, DateTime.Now.AddMinutes((double)this.timeout));
		}

		public void Insert(string key, object data, DateTime cacheTime)
		{
			this.client.Store(StoreMode.Set, key, data, cacheTime);
		}

        public bool Exists(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string key, T data)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string key, T data, DateTime cacheTime)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string key, T data, int cacheTime)
        {
            throw new NotImplementedException();
        }

        public void RegisterSubscribe<T>(string key, Cache.DoSub dosub)
        {
            throw new NotImplementedException();
        }

        public void Send(string key, object data)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterSubscrib(string key)
        {
            throw new NotImplementedException();
        }
    }
}

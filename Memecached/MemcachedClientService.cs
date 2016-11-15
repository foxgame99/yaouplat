using Enyim.Caching;
using System;

namespace Yaouplat.Strategy.Memecached
{
	internal class MemcachedClientService
	{
		private static readonly MemcachedClientService _instance = new MemcachedClientService();

		private readonly MemcachedClient _client;

		public MemcachedClient Client
		{
			get
			{
				return this._client;
			}
		}

		public static MemcachedClientService Instance
		{
			get
			{
				return MemcachedClientService._instance;
			}
		}

		private MemcachedClientService()
		{
			this._client = new MemcachedClient("memcached");
		}
	}
}

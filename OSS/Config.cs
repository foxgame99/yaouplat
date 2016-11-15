using Himall.Core;
using System;
using System.Configuration;

namespace Yaouplat.Strategy.OSS
{
	public static class Config
	{
		public static readonly string PrivateEndpoint;

		public static readonly string FileServerDomain;

		public static readonly string AccessKeyId;

		public static readonly string AccessKeySecret;

		public static readonly string BucketName;

		public static readonly string ImageServerDomain;

		static Config()
		{
			Config.PrivateEndpoint = ConfigurationManager.AppSettings["PrivateEndpoint"];
			Config.FileServerDomain = ConfigurationManager.AppSettings["FileServerDomain"];
			Config.AccessKeyId = ConfigurationManager.AppSettings["AccessKeyId"];
			Config.AccessKeySecret = ConfigurationManager.AppSettings["AccessKeySecret"];
			Config.BucketName = ConfigurationManager.AppSettings["BucketName"];
			Config.ImageServerDomain = ConfigurationManager.AppSettings["ImageServerDomain"];
			if (string.IsNullOrWhiteSpace(Config.PrivateEndpoint))
			{
				throw new HimallIOException("未配置PrivateEndpoint节点");
			}
			if (string.IsNullOrWhiteSpace(Config.FileServerDomain))
			{
				throw new HimallIOException("未配置FileServerDomain节点");
			}
			if (string.IsNullOrWhiteSpace(Config.AccessKeyId))
			{
				throw new HimallIOException("未配置AccessKeyId节点");
			}
			if (string.IsNullOrWhiteSpace(Config.AccessKeySecret))
			{
				throw new HimallIOException("未配置AccessKeySecret节点");
			}
			if (string.IsNullOrWhiteSpace(Config.BucketName))
			{
				throw new HimallIOException("未配置BucketName节点");
			}
		}
	}
}

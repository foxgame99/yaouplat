using Aliyun.OSS;
using System;

namespace Yaouplat.Strategy.OSS
{
	internal static class OssClientFactory
	{
		public static IOss CreateOssClient()
		{
			return OssClientFactory.CreateOssClient(AccountSettings.Load());
		}

		public static IOss CreateOssClient(AccountSettings settings)
		{
			return new OssClient(settings.OssEndpoint, settings.OssAccessKeyId, settings.OssAccessKeySecret);
		}
	}
}

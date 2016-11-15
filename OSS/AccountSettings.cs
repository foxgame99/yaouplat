using System;

namespace Yaouplat.Strategy.OSS
{
	internal class AccountSettings
	{
		public string OssEndpoint
		{
			get;
			set;
		}

		public string OssAccessKeyId
		{
			get;
			set;
		}

		public string OssAccessKeySecret
		{
			get;
			set;
		}

		private AccountSettings()
		{
		}

		public static AccountSettings Load()
		{
			return new AccountSettings
			{
				OssAccessKeyId = Config.AccessKeyId,
				OssAccessKeySecret = Config.AccessKeySecret,
				OssEndpoint = Config.PrivateEndpoint
			};
		}
	}
}

using Aliyun.OSS;
using Himall.Core;
using Himall.Core.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yaouplat.Strategy.OSS
{
	public class OSS : IHimallIO, IStrategy
	{
		private static IOss ossClient;

		public OSS()
		{
			OSS.ossClient = OssClientFactory.CreateOssClient();
		}

		public string GetFilePath(string fileName)
		{
			fileName = this.GetFileName(fileName);
			if (!string.IsNullOrEmpty(fileName))
			{
				return string.Format("http://{0}/{1}", Config.FileServerDomain, fileName);
			}
			return string.Empty;
		}

		public string GetProductSizeImage(string productPath, int index, int width = 0)
		{
			if (string.IsNullOrWhiteSpace(productPath))
			{
				return string.Empty;
			}
			string text = productPath;
			if (string.IsNullOrEmpty(System.IO.Path.GetExtension(productPath)) && !productPath.EndsWith("/"))
			{
				text = productPath + "/";
			}
			text = System.IO.Path.GetDirectoryName(text).Replace("\\", "/");
			if (string.IsNullOrWhiteSpace(Config.ImageServerDomain))
			{
				throw new HimallIOException("调用获取图片路径接口必须配置ImageServerDomain节点");
			}
			string text2 = string.Format("{0}/{1}.png", text, index);
			if (!string.IsNullOrEmpty(text2))
			{
				string result = string.Empty;
				if (width == 0)
				{
					if (text2.StartsWith("http"))
					{
						result = text2.Replace("http:/", "http://");
					}
					else
					{
						result = string.Format("http://{0}/{1}", Config.ImageServerDomain, text2);
					}
				}
				if (width != 0)
				{
					string text3 = "@";
					if (text2.StartsWith("http"))
					{
						text2 = text2.Replace("http:/", "http://");
						result = string.Format("{0}{1}{2}w", text2, text3, width);
					}
					else
					{
						result = string.Format("http://{0}/{1}{2}{3}w", new object[]
						{
							Config.ImageServerDomain,
							text2,
							text3,
							width
						});
					}
				}
				return result;
			}
			return text2;
		}

		public string GetImagePath(string imageName, string styleName = null)
		{
			if (string.IsNullOrWhiteSpace(Config.ImageServerDomain))
			{
				throw new HimallIOException("调用获取图片路径接口必须配置ImageServerDomain节点");
			}
			imageName = this.GetFileName(imageName);
			if (string.IsNullOrEmpty(imageName))
			{
				return imageName;
			}
			if (imageName.StartsWith("http"))
			{
				return imageName;
			}
			string result = string.Empty;
			if (string.IsNullOrWhiteSpace(styleName))
			{
				result = string.Format("http://{0}/{1}", Config.ImageServerDomain, imageName);
			}
			else
			{
				string text = "@!";
				result = string.Format("http://{0}/{1}{2}{3}", new object[]
				{
					Config.ImageServerDomain,
					imageName,
					text,
					styleName
				});
			}
			return result;
		}

		public byte[] GetFileContent(string fileName)
		{
			if (this.ExistFile(fileName))
			{
				fileName = this.GetFileName(fileName);
				OssObject @object = OSS.ossClient.GetObject(Config.BucketName, fileName);
				System.Collections.Generic.List<byte> list = new System.Collections.Generic.List<byte>();
				if (@object.Content != null)
				{
					for (int num = @object.Content.ReadByte(); num != -1; num = @object.Content.ReadByte())
					{
						list.Add((byte)num);
					}
				}
				return list.ToArray();
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
		}

		public void CreateFile(string fileName, System.IO.Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew)
		{
			fileName = this.GetFileName(fileName);
			if (fileCreateType != FileCreateType.CreateNew)
			{
				this.RecurseCreateFileDir(fileName);
				OSS.ossClient.PutObject(Config.BucketName, fileName, stream);
				return;
			}
			if (!this.ExistFile(fileName))
			{
				this.RecurseCreateFileDir(fileName);
				OSS.ossClient.PutObject(Config.BucketName, fileName, stream);
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
		}

		public void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew)
		{
			fileName = this.GetFileName(fileName);
			if (fileCreateType == FileCreateType.CreateNew)
			{
				if (!this.ExistFile(fileName))
				{
					this.RecurseCreateFileDir(fileName);
					byte[] bytes = System.Text.Encoding.ASCII.GetBytes(content);
					using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
					{
						OSS.ossClient.PutObject(Config.BucketName, fileName, memoryStream);
						return;
					}
				}
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
			}
			this.RecurseCreateFileDir(fileName);
			byte[] bytes2 = System.Text.Encoding.ASCII.GetBytes(content);
			System.IO.MemoryStream content2 = new System.IO.MemoryStream(bytes2);
			OSS.ossClient.PutObject(Config.BucketName, fileName, content2);
		}

		public void CreateDir(string dirName)
		{
			dirName = this.GetDirName(dirName);
			if (!this.ExistDir(dirName))
			{
				System.Collections.Generic.List<string> dirs = dirName.Remove(dirName.Length - 1).Split(new char[]
				{
					'/'
				}).ToList<string>();
				this.RecurseCreateDir(dirs);
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.DirExist));
		}

		public bool ExistFile(string fileName)
		{
			fileName = this.GetFileName(fileName);
			return OSS.ossClient.DoesObjectExist(Config.BucketName, fileName);
		}

		public bool ExistDir(string dirName)
		{
			dirName = this.GetDirName(dirName);
			return OSS.ossClient.DoesObjectExist(Config.BucketName, dirName);
		}

		public void DeleteDir(string dirName, bool recursive = false)
		{
			dirName = this.GetDirName(dirName);
			if (!this.ExistDir(dirName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.DirNotExist));
			}
			if (recursive)
			{
				System.Collections.Generic.List<string> files = this.GetFiles(dirName, true);
				DeleteObjectsRequest deleteObjectsRequest = new DeleteObjectsRequest(Config.BucketName, files);
				OSS.ossClient.DeleteObjects(deleteObjectsRequest);
				return;
			}
			System.Collections.Generic.List<string> dirAndFiles = this.GetDirAndFiles(dirName, false);
			if (dirAndFiles == null || dirAndFiles.Count == 0)
			{
				OSS.ossClient.DeleteObject(Config.BucketName, dirName);
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.DirDeleleError));
		}

		public void DeleteFile(string fileName)
		{
			fileName = this.GetFileName(fileName);
			if (this.ExistFile(fileName))
			{
				OSS.ossClient.DeleteObject(Config.BucketName, fileName);
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
		}

		public void DeleteFiles(System.Collections.Generic.List<string> fileNames)
		{
			DeleteObjectsRequest deleteObjectsRequest = new DeleteObjectsRequest(Config.BucketName, fileNames, false);
			OSS.ossClient.DeleteObjects(deleteObjectsRequest);
		}

		public void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			sourceFileName = this.GetFileName(sourceFileName);
			if (!this.ExistFile(sourceFileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
			}
			ObjectMetadata objectMetadata = OSS.ossClient.GetObjectMetadata(Config.BucketName, sourceFileName);
			if (objectMetadata.ObjectType == "Appendable")
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.AppendFileNotOperate));
			}
			destFileName = this.GetFileName(destFileName);
			if (!overwrite && this.ExistFile(destFileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
			}
			this.RecurseCreateFileDir(destFileName);
			CopyObjectRequest copyObjectRequst = new CopyObjectRequest(Config.BucketName, sourceFileName, Config.BucketName, destFileName);
			OSS.ossClient.CopyObject(copyObjectRequst);
		}

		public void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			sourceFileName = this.GetFileName(sourceFileName);
			if (!this.ExistFile(sourceFileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
			}
			ObjectMetadata objectMetadata = OSS.ossClient.GetObjectMetadata(Config.BucketName, sourceFileName);
			if (objectMetadata.ObjectType == "Appendable")
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.AppendFileNotOperate));
			}
			destFileName = this.GetFileName(destFileName);
			if (!overwrite && this.ExistFile(destFileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
			}
			this.RecurseCreateFileDir(destFileName);
			CopyObjectRequest copyObjectRequst = new CopyObjectRequest(Config.BucketName, sourceFileName, Config.BucketName, destFileName);
			OSS.ossClient.CopyObject(copyObjectRequst);
			OSS.ossClient.DeleteObject(Config.BucketName, sourceFileName);
		}

		public System.Collections.Generic.List<string> GetDirAndFiles(string dirName, bool self = false)
		{
			dirName = this.GetDirName(dirName);
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
			ListObjectsRequest listObjectsRequest = new ListObjectsRequest(Config.BucketName)
			{
				Prefix = dirName,
				Delimiter = "/"
			};
			ObjectListing objectListing = OSS.ossClient.ListObjects(listObjectsRequest);
			foreach (string current in objectListing.CommonPrefixes)
			{
				list.Add(current);
			}
			foreach (OssObjectSummary current2 in objectListing.ObjectSummaries)
			{
				list.Add(current2.Key);
			}
			if (!self)
			{
				list.Remove(dirName);
			}
			return list;
		}

		public System.Collections.Generic.List<string> GetFiles(string dirName, bool self = false)
		{
			dirName = this.GetDirName(dirName);
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
			ListObjectsRequest listObjectsRequest = new ListObjectsRequest(Config.BucketName)
			{
				Prefix = dirName
			};
			ObjectListing objectListing = OSS.ossClient.ListObjects(listObjectsRequest);
			foreach (string current in objectListing.CommonPrefixes)
			{
				list.Add(current);
			}
			foreach (OssObjectSummary current2 in objectListing.ObjectSummaries)
			{
				list.Add(current2.Key);
			}
			if (!self)
			{
				list.Remove(dirName);
			}
			return list;
		}

		public void AppendFile(string fileName, System.IO.Stream stream)
		{
			fileName = this.GetFileName(fileName);
			long position = 0L;
			if (this.ExistFile(fileName))
			{
				ObjectMetadata objectMetadata = OSS.ossClient.GetObjectMetadata(Config.BucketName, fileName);
				if (!(objectMetadata.ObjectType == "Appendable"))
				{
					throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.NoramlFileNotOperate));
				}
				position = objectMetadata.ContentLength;
			}
			else
			{
				this.RecurseCreateFileDir(fileName);
			}
			AppendObjectRequest request = new AppendObjectRequest(Config.BucketName, fileName)
			{
				ObjectMetadata = new ObjectMetadata(),
				Content = stream,
				Position = position
			};
			OSS.ossClient.AppendObject(request);
		}

		public void AppendFile(string fileName, string content)
		{
			fileName = this.GetFileName(fileName);
			long position = 0L;
			if (this.ExistFile(fileName))
			{
				ObjectMetadata objectMetadata = OSS.ossClient.GetObjectMetadata(Config.BucketName, fileName);
				if (!(objectMetadata.ObjectType == "Appendable"))
				{
					throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.NoramlFileNotOperate));
				}
				position = objectMetadata.ContentLength;
			}
			else
			{
				this.RecurseCreateFileDir(fileName);
			}
			byte[] bytes = System.Text.Encoding.Default.GetBytes(content);
			using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes))
			{
				AppendObjectRequest request = new AppendObjectRequest(Config.BucketName, fileName)
				{
					ObjectMetadata = new ObjectMetadata(),
					Content = memoryStream,
					Position = position
				};
				OSS.ossClient.AppendObject(request);
			}
		}

		public MetaInfo GetDirMetaInfo(string dirName)
		{
			dirName = this.GetDirName(dirName);
			if (this.ExistDir(dirName))
			{
				ObjectMetadata objectMetadata = OSS.ossClient.GetObjectMetadata(Config.BucketName, dirName);
				MetaInfo metaInfo = new MetaInfo();
                metaInfo.LastModifiedTime = objectMetadata.LastModified.AddHours(8.0);
                metaInfo.ContentLength = objectMetadata.ContentLength;
                metaInfo.ContentType = objectMetadata.ContentType;
                metaInfo.ObjectType = objectMetadata.ObjectType;
				return metaInfo;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.DirNotExist));
		}

		public MetaInfo GetFileMetaInfo(string fileName)
		{
			fileName = this.GetFileName(fileName);
			if (this.ExistFile(fileName))
			{
				ObjectMetadata objectMetadata = OSS.ossClient.GetObjectMetadata(Config.BucketName, fileName);
				MetaInfo metaInfo = new MetaInfo();
                metaInfo.LastModifiedTime = objectMetadata.LastModified.AddHours(8.0);
                metaInfo.ContentLength = objectMetadata.ContentLength;
                metaInfo.ContentType = objectMetadata.ContentType;
                metaInfo.ObjectType = objectMetadata.ObjectType;
				return metaInfo;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
		}

		private void RecurseCreateFileDir(string fileName)
		{
			if (fileName.Contains("/"))
			{
				System.Collections.Generic.List<string> list = fileName.Split(new char[]
				{
					'/'
				}).ToList<string>();
				list.RemoveAt(list.Count - 1);
				this.RecurseCreateDir(list);
			}
		}

		private void RecurseCreateDir(System.Collections.Generic.List<string> dirs)
		{
			string text = string.Empty;
			using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
			{
				foreach (string current in dirs)
				{
					text += this.GetDirName(current);
					if (!this.ExistDir(text))
					{
						OSS.ossClient.PutObject(Config.BucketName, text, memoryStream);
					}
				}
			}
		}

		private string GetFileName(string fileName)
		{
			if (!string.IsNullOrWhiteSpace(fileName) && fileName.StartsWith("/"))
			{
				fileName = fileName.Substring(1);
			}
			return fileName;
		}

		private string GetDirName(string dirName)
		{
			if (string.IsNullOrWhiteSpace(dirName))
			{
				return string.Empty;
			}
			if (dirName.StartsWith("/"))
			{
				dirName = dirName.Substring(1);
			}
			if (!dirName.EndsWith("/"))
			{
				dirName += "/";
			}
			return dirName;
		}

		public void CreateThumbnail(string sourceFilename, string destFilename, int width, int height)
		{
		}
	}
}

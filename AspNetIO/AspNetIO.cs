using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Yaouplat.Strategy.AspNetIO
{
	public class AspNetIO : IHimallIO, IStrategy
	{
		public void AppendFile(string fileName, string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			string physicalPath = this.GetPhysicalPath(fileName);
			FileStream fileStream = new FileStream(physicalPath, FileMode.Append, FileAccess.Write);
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Close();
		}

		private byte[] StreamToBytes(Stream stream)
		{
			byte[] array = new byte[stream.Length];
			stream.Read(array, 0, array.Length);
			stream.Seek(0L, SeekOrigin.Begin);
			return array;
		}

		public void AppendFile(string fileName, Stream stream)
		{
			string physicalPath = this.GetPhysicalPath(fileName);
			byte[] array = this.StreamToBytes(stream);
			FileStream fileStream = new FileStream(physicalPath, FileMode.Append, FileAccess.Write);
			fileStream.Write(array, 0, array.Length);
			fileStream.Close();
		}

		public void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			if (string.IsNullOrWhiteSpace(sourceFileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
			}
			string physicalPath = this.GetPhysicalPath(sourceFileName);
			string physicalPath2 = this.GetPhysicalPath(destFileName);
			string path = physicalPath2.Remove(physicalPath2.LastIndexOf("\\"));
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (!overwrite && this.ExistFile(physicalPath2))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
			}
			File.Copy(physicalPath, physicalPath2, overwrite);
		}

		public void CreateDir(string dirName)
		{
			string physicalPath = this.GetPhysicalPath(dirName);
			Directory.CreateDirectory(physicalPath);
		}

		public void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			string physicalPath = this.GetPhysicalPath(fileName);
			string path = physicalPath.Remove(physicalPath.LastIndexOf("\\"));
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (fileCreateType != FileCreateType.CreateNew)
			{
				FileStream fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write);
				fileStream.Write(bytes, 0, bytes.Length);
				fileStream.Close();
				return;
			}
			if (!File.Exists(physicalPath))
			{
				FileStream fileStream2 = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write);
				fileStream2.Write(bytes, 0, bytes.Length);
				fileStream2.Close();
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
		}

		public void CreateFile(string fileName, Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew)
		{
			string physicalPath = this.GetPhysicalPath(fileName);
			string path = physicalPath.Remove(physicalPath.LastIndexOf("\\"));
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (fileCreateType != FileCreateType.CreateNew)
			{
				FileStream fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write);
				byte[] array = this.StreamToBytes(stream);
				fileStream.Write(array, 0, array.Length);
				fileStream.Close();
				return;
			}
			if (!File.Exists(physicalPath))
			{
				FileStream fileStream2 = new FileStream(physicalPath, FileMode.Create, FileAccess.Write);
				byte[] array2 = this.StreamToBytes(stream);
				fileStream2.Write(array2, 0, array2.Length);
				fileStream2.Close();
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
		}

		public void DeleteDir(string dirName, bool recursive = false)
		{
			string physicalPath = this.GetPhysicalPath(dirName);
			if (Directory.Exists(physicalPath))
			{
				Directory.Delete(physicalPath, recursive);
				return;
			}
			throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.DirNotExist));
		}

		public void DeleteFile(string fileName)
		{
			string physicalPath = this.GetPhysicalPath(fileName);
			if (this.ExistFile(fileName))
			{
				File.Delete(physicalPath);
			}
		}

		public void DeleteFiles(List<string> fileNames)
		{
			foreach (string current in fileNames)
			{
				string physicalPath = this.GetPhysicalPath(current);
				if (this.ExistFile(physicalPath))
				{
					File.Delete(physicalPath);
				}
			}
		}

		public bool ExistDir(string dirName)
		{
			string physicalPath = this.GetPhysicalPath(dirName);
			return Directory.Exists(physicalPath);
		}

		public bool ExistFile(string fileName)
		{
			string physicalPath = this.GetPhysicalPath(fileName);
			return File.Exists(physicalPath);
		}

		public List<string> GetDirAndFiles(string dirName, bool self = false)
		{
			List<string> list = new List<string>();
			string physicalPath = this.GetPhysicalPath(dirName);
			if (self)
			{
				list.Add(physicalPath);
			}
			list.AddRange(this.GetDirAndFiles(physicalPath));
			return list;
		}

		public MetaInfo GetDirMetaInfo(string dirName)
		{
			string physicalPath = this.GetPhysicalPath(dirName);
			MetaInfo metaInfo = new MetaInfo();
            metaInfo.LastModifiedTime = Directory.GetLastWriteTime(physicalPath);
            metaInfo.ContentLength = IOHelper.GetDirectoryLength(physicalPath);
			return metaInfo;
		}

		public byte[] GetFileContent(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
			}
			string physicalPath = this.GetPhysicalPath(fileName);
			FileStream fileStream = new FileStream(physicalPath, FileMode.Open);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			fileStream.Close();
			return array;
		}

		public MetaInfo GetFileMetaInfo(string fileName)
		{
			MetaInfo metaInfo = new MetaInfo();
			string physicalPath = this.GetPhysicalPath(fileName);
			FileInfo fileInfo = new FileInfo(physicalPath);
            metaInfo.ContentLength = fileInfo.Length;
			string mimeMapping = MimeMapping.GetMimeMapping(physicalPath);
            metaInfo.ContentType = mimeMapping;
            metaInfo.LastModifiedTime = fileInfo.LastWriteTime;
			return metaInfo;
		}

		public string GetFilePath(string fileName)
		{
			return string.Format("{0}/{1}", this.GetHttpUrl(), fileName);
		}

		public List<string> GetFiles(string dirName, bool self = false)
		{
			List<string> list = new List<string>();
			string physicalPath = this.GetPhysicalPath(dirName);
			if (self)
			{
				list.Add(physicalPath);
			}
			list.AddRange(this.GetAllFiles(physicalPath));
			return list;
		}

		private List<string> GetAllFiles(string path)
		{
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(path);
			list.AddRange(files);
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			for (int i = 0; i < array.Length; i++)
			{
				string path2 = array[i];
				list.AddRange(this.GetAllFiles(path2));
			}
			return list;
		}

		private List<string> GetDirAndFiles(string path)
		{
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(path);
			list.AddRange(files);
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				list.Add(text);
				list.AddRange(this.GetAllFiles(text));
			}
			return list;
		}

		public string GetImagePath(string imageName, string styleName = null)
		{
			string arg_05_0 = string.Empty;
			if (!string.IsNullOrWhiteSpace(styleName))
			{
				return imageName;
			}
			return imageName;
		}

		public void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			if (string.IsNullOrWhiteSpace(sourceFileName))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileNotExist));
			}
			string physicalPath = this.GetPhysicalPath(sourceFileName);
			string physicalPath2 = this.GetPhysicalPath(destFileName);
			string path = physicalPath2.Remove(physicalPath2.LastIndexOf("\\"));
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (!overwrite && this.ExistFile(physicalPath2))
			{
				throw new HimallIOException(EnumHelper.ToDescription(IOErrorMsg.FileExist));
			}
			File.Move(physicalPath, physicalPath2);
		}

		private string GetHttpUrl()
		{
			string host = WebHelper.GetHost();
			string port = WebHelper.GetPort();
			string str = (port == "80") ? "" : (":" + port);
			return "http://" + host + str + "/";
		}

		private string GetFileName(string fileName)
		{
			if (!string.IsNullOrWhiteSpace(fileName) && fileName.StartsWith("/"))
			{
				fileName = fileName.Substring(1);
			}
			return fileName;
		}

		private string GetPhysicalPath(string fileName)
		{
			return IOHelper.GetMapPath(fileName);
		}

		private string GetDirName(string dirName)
		{
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
			string physicalPath = this.GetPhysicalPath(sourceFilename);
			string physicalPath2 = this.GetPhysicalPath(destFilename);
			ImageHelper.CreateThumbnail(physicalPath, physicalPath2, width, height);
		}

		public string GetProductSizeImage(string productPath, int index, int width = 0)
		{
			if (!string.IsNullOrEmpty(productPath))
			{
				if (string.IsNullOrEmpty(Path.GetExtension(productPath)) && !productPath.EndsWith("/"))
				{
					productPath += "/";
				}
				productPath = Path.GetDirectoryName(productPath).Replace("\\", "/");
				string result = string.Empty;
				if (width == 0)
				{
					result = string.Format("{0}/{1}.png", productPath, index);
				}
				if (width != 0)
				{
					result = string.Format(productPath + "/{0}_{1}.png", index, width);
				}
				return result;
			}
			return productPath;
		}
	}
}

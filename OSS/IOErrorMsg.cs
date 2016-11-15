using System;
using System.ComponentModel;

namespace Yaouplat.Strategy.OSS
{
	public enum IOErrorMsg
	{
		[Description("文件名称不能为空")]
		FileEmpty = 1,
		[Description("文件已存在")]
		FileExist,
		[Description("目录已存在")]
		DirExist = 4,
		[Description("文件不存在")]
		FileNotExist = 8,
		[Description("目录不存在")]
		DirNotExist = 16,
		[Description("目录下包含子目录或文件，无法删除")]
		DirDeleleError = 32,
		[Description("文件为普通文件，不允许操作")]
		NoramlFileNotOperate = 64,
		[Description("文件为可追加文件，不允许操作")]
		AppendFileNotOperate = 128,
		[Description("目标文件为可追加文件，不允许操作")]
		TargetAppendFileNotOperate = 256
	}
}

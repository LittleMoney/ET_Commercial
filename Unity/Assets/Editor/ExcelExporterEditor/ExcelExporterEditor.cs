using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ETModel;
using MongoDB.Bson;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

public struct CellInfo
{
	public string Type;
	public string Name;
	public string Desc;
}

public class ExcelMD5Info
{
	public Dictionary<string, string> fileMD5 = new Dictionary<string, string>();

	public string Get(string fileName)
	{
		string md5 = "";
		this.fileMD5.TryGetValue(fileName, out md5);
		return md5;
	}

	public void Add(string fileName, string md5)
	{
		this.fileMD5[fileName] = md5;
	}
}

public class ExcelExporterEditor : EditorWindow
{
	[MenuItem("Tools/导出配置")]
	private static void ShowWindow()
	{
		GetWindow(typeof(ExcelExporterEditor));
	}

	//private const string ExcelPath = "../Excel";
	//private const string ServerConfigPath = "../Config/";

	//private bool isClient;

	//private ExcelMD5Info md5Info;



	private const string ExcelPath = "../Excel";

	private const string ClientBundlesDir = "Assets/Bundles";
	private const string ClientConfigPath = "Assets/Bundles/{0}/Config";
	private const string ExportClientModel = @"./Assets/Model/Generated/Config";
	private const string ExportClientHotfix = @"./Assets/Hotfix/Generated/Config";

	private const string ServerConfigPath = "../ServerConfig/";
	private const string ExportServerModel = @"../Server/Model/Generated/Config";

	private bool isClient;


	// Update is called once per frame
	private void OnGUI()
	{
		try
		{
			if (GUILayout.Button("导出客户端配置"))
			{
				this.isClient = true;

				ExportAll(ClientConfigPath);
				AssetDatabase.Refresh();
				ExportAllClass(ExportClientModel, "namespace ETModel\n{\n");
				ExportAllClass(ExportClientHotfix, "using ETModel;\n\nnamespace ETHotfix\n{\n");

				Log.Info($"导出客户端配置完成!");
			}


			if (GUILayout.Button("清理客户端配置"))
			{
				DirectoryInfo _dirInfo = new DirectoryInfo(ClientBundlesDir);
				foreach (DirectoryInfo _subDirInfo in _dirInfo.GetDirectories())
				{
					DirectoryInfo _toyConfigDir = new DirectoryInfo(string.Format(ClientConfigPath, _subDirInfo.Name));
					if (_toyConfigDir.Exists)
					{
						_toyConfigDir.Delete(true);
					}
				}

				if (Directory.Exists(ExportClientModel))
				{
					Directory.Delete(ExportClientModel, true);
				}
				if (Directory.Exists(ExportClientHotfix))
				{
					Directory.Delete(ExportClientHotfix, true);
				}

				Log.Info($"清理客户端配置完成!");
			}

			if (GUILayout.Button("导出服务端配置"))
			{
				this.isClient = false;

				ExportAll(ServerConfigPath);

				ExportAllClass(ExportServerModel, "namespace ETModel\n{\n");

				Log.Info($"导出服务端配置完成!");
			}

			if (GUILayout.Button("清理服务端配置"))
			{
				if (Directory.Exists(ServerConfigPath))
				{
					Directory.Delete(ServerConfigPath, true);
				}

				if (Directory.Exists(ExportServerModel))
				{
					Directory.Delete(ExportServerModel, true);
				}
				Log.Info($"清理服务端配置完成!");
			}
		}
		catch (Exception e)
		{
			Log.Error(e);
		}
	}

	private void ExportAllClass(string exportDir, string csHead, HashSet<string> classMap=null,string relativePath = "")
	{
		DirectoryInfo _directoryInfo = new DirectoryInfo(Path.Combine(ExcelPath, relativePath));

		if (relativePath == "")
		{
			HashSet<string>  _classMap = new HashSet<string>();
			//经过实际开发后发现命名空间多了容易混乱，放弃每个模块对应命名空间的做法
			string _csHead = csHead.Substring(0, csHead.Length - 3) + "\n{\n";

			foreach (FileInfo fileInfo in _directoryInfo.GetFiles())
			{
				if (fileInfo.Name == "md5.txt") continue;
				if (fileInfo.Extension != ".xlsx") continue;
				if (Path.GetFileName(fileInfo.Name).StartsWith("~")) continue;
				throw new Exception("不要将配置文件放在Excel根目录下,请放在对应Toy模块名的子目录中");
			}

			foreach (DirectoryInfo _subDirInfo in _directoryInfo.GetDirectories())
			{
				if (_subDirInfo.Name.StartsWith("~")) continue;

				Log.Debug(Path.Combine(relativePath, _subDirInfo.Name));
				ExportAllClass(exportDir, csHead, _classMap, Path.Combine(relativePath, _subDirInfo.Name));
				Log.Info($"进入目录 {Path.Combine(relativePath, _subDirInfo.Name)}");
			}
			AssetDatabase.Refresh();
		}
		else
        {
			//经过实际开发后发现命名空间多了容易混乱，放弃每个模块对应命名空间的做法
			string _csHead = csHead.Substring(0, csHead.Length - 3) + "\n{\n";

			foreach (FileInfo fileInfo in _directoryInfo.GetFiles())
			{
				if (fileInfo.Name == "md5.txt") continue;
				if (fileInfo.Extension != ".xlsx") continue;
				if (Path.GetFileName(fileInfo.Name).StartsWith("~")) continue;

				ExportClass(fileInfo.FullName, exportDir, _csHead, classMap,relativePath);
				Log.Info($"生成{fileInfo.FullName}类");
			}

			foreach (DirectoryInfo _subDirInfo in _directoryInfo.GetDirectories())
			{
				if (_subDirInfo.Name.StartsWith("~")) continue;

				ExportAllClass(exportDir, csHead, classMap, Path.Combine(relativePath, _subDirInfo.Name));
				Log.Info($"进入目录 {Path.Combine(relativePath, _subDirInfo.Name)}");
			}
		}
	}

	private void ExportClass(string fileName, string exportDir, string csHead, HashSet<string> classMap = null,string relativePath="")
	{
		XSSFWorkbook xssfWorkbook;
		using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			xssfWorkbook = new XSSFWorkbook(file);
		}

		//检查是否组要导出类文件到目录
		string _scope = GetCellString(xssfWorkbook.GetSheetAt(0), 0, 0);

		//客户端
		if (exportDir == ExportClientModel && !_scope.Contains("AppType." + AppType.ClientM.ToString()))
		{
			return;
		}

		//客户端热更
		if (exportDir == ExportClientHotfix && !_scope.Contains("AppType." + AppType.ClientH.ToString()))
		{
			return;
		}

		//服务端
		if (exportDir == ExportServerModel && _scope.Replace("AppType." + AppType.ClientM.ToString(), "").Replace("AppType." + AppType.ClientH.ToString(), "").Replace("|", "").Replace("\t", "").Trim() == "")
		{
			return;
		}

		string protoName = Path.GetFileNameWithoutExtension(fileName);
		string exportPath = Path.Combine(exportDir, relativePath, $"{protoName}.cs");
		string exportDirPath = Path.Combine(exportDir, relativePath);

		//重建目录
		if (!Directory.Exists(exportDirPath)) Directory.CreateDirectory(exportDirPath);
		//删除文件重建
		if (!File.Exists(exportPath)) File.Delete(exportPath);


		if (classMap.Contains(protoName))
		{
			using (FileStream txt = new FileStream(exportPath, FileMode.Create))
			using (StreamWriter sw = new StreamWriter(txt))
			{
				sw.Write("//配置类已经在其他文件中生成");
			}
			return;
		}
      
		using (FileStream txt = new FileStream(exportPath, FileMode.Create))
		using (StreamWriter sw = new StreamWriter(txt))
		{
			StringBuilder sb = new StringBuilder();
			ISheet sheet = xssfWorkbook.GetSheetAt(0);
			sb.Append(csHead);

			int cellCount = sheet.GetRow(3).LastCellNum;
			string idFieldType = null;

			//获取id的类型
			for (int i = 2; i < cellCount; i++)
			{
				string fieldDesc = GetCellString(sheet, 2, i);

				if (fieldDesc.StartsWith("#")) continue; //列被注释
				if (fieldDesc.StartsWith("s") && this.isClient) continue; // s开头表示这个列是服务端专用
				if (fieldDesc.StartsWith("c") && !this.isClient) continue; // c开头表示这个字段是客户端专用

				string fieldName = GetCellString(sheet, 3, i);

				if (fieldName == "Id" || fieldName == "_id")
				{
					idFieldType = GetCellString(sheet, 4, i);
					break;
				}
			}

			if (idFieldType != null && idFieldType != "string" && idFieldType != "long")
			{
				throw new Exception("Id 的数据类型必须是 string,long 其中之一");
			}

			sb.Append($"\t[Config((int)({GetCellString(sheet, 0, 0)}))]\n");
			sb.Append($"\tpublic partial class {protoName}Category : ACategory<{protoName}>\n");
			sb.Append("\t{\n");
			sb.Append("\t}\n\n");

			if (idFieldType == "string")
			{
				sb.Append($"\tpublic class {protoName}: IConfigString\n");
				sb.Append("\t{\n");
				sb.Append("\t\tpublic string Id{ get; set; }\n");
			}
			else
			{
				sb.Append($"\tpublic class {protoName}: IConfigLong\n");
				sb.Append("\t{\n");
				sb.Append("\t\tpublic long Id{ get; set; }\n");
			}

			for (int i = 2; i < cellCount; i++)
			{
				string fieldDesc = GetCellString(sheet, 2, i);

				if (fieldDesc.StartsWith("#")) continue; //列被注释
				if (fieldDesc.StartsWith("s") && this.isClient) continue; // s开头表示这个列是服务端专用
				if (fieldDesc.StartsWith("c") && !this.isClient) continue; // c开头表示这个字段是客户端专用

				string fieldName = GetCellString(sheet, 3, i);

				if (fieldName == "Id" || fieldName == "_id")
				{
					continue;
				}

				string fieldType = GetCellString(sheet, 4, i);
				if (fieldType == "" || fieldName == "")
				{
					continue;
				}

				sb.Append($"\t\tpublic {fieldType} {fieldName};\n");
			}

			sb.Append("\t}\n");
			sb.Append("}\n");

			sw.Write(sb.ToString());
		}

		classMap.Add(protoName);
	}

	private void ExportAll(string exportDir)
	{
		ExportDir(ExcelPath, exportDir, "");
		Log.Info("所有表导表完成");
		AssetDatabase.Refresh();
	}

	private void ExportDir(string ExcelPath, string exportDir, string toyDirName = "", string relativePath = "")
	{
		DirectoryInfo _directoryInfo = new DirectoryInfo(Path.Combine(ExcelPath, relativePath));

		if (relativePath == "")
		{
			//导出当前目录下的表格
			foreach (FileInfo _fileInfo in _directoryInfo.GetFiles())
			{
				if (_fileInfo.Name == "md5.txt") continue;
				if (_fileInfo.Extension != ".xlsx") continue;
				if (_fileInfo.Name.StartsWith("~")) continue;

				throw new Exception("不要将配置文件放在Excel根目录下,请放在对应toy模块名的子目录中");
			}

			//导出子目录下的表格
			foreach (DirectoryInfo _subDirInfo in _directoryInfo.GetDirectories())
			{
				if (_subDirInfo.Name.StartsWith("~")) continue;
				ExportDir(ExcelPath, exportDir, _subDirInfo.Name, _subDirInfo.Name);
			}

		}
		else
		{
			//导出当前目录下的表格
			foreach (FileInfo _fileInfo in _directoryInfo.GetFiles())
			{
				if (_fileInfo.Name == "md5.txt") continue;
				if (_fileInfo.Extension != ".xlsx") continue;
				if (_fileInfo.Name.StartsWith("~")) continue;
				Export(_fileInfo.FullName, exportDir, toyDirName, relativePath);
			}

			//导出子目录下的表格
			foreach (DirectoryInfo _subDirInfo in _directoryInfo.GetDirectories())
			{
				if (_subDirInfo.Name.StartsWith("~")) continue;
				ExportDir(ExcelPath, exportDir, toyDirName, Path.Combine(relativePath, _subDirInfo.Name));
			}
		}
	}

	private void Export(string fileName, string exportDir,string toyDirName, string relativePath)
	{
		XSSFWorkbook xssfWorkbook;
		using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			xssfWorkbook = new XSSFWorkbook(file);
		}

		//检查是否组要导出配置文件到目录
		string _scope = GetCellString(xssfWorkbook.GetSheetAt(0), 0, 0);

		if (exportDir == ClientConfigPath && !_scope.Contains("AppType." + AppType.ClientM.ToString()) && !_scope.Contains("AppType." + AppType.ClientH.ToString()))
		{
			Log.Debug($"{fileName} {_scope} 配置不属于 Client");
			return;
		}

		if (exportDir == ServerConfigPath && _scope.Replace("AppType." + AppType.ClientM.ToString(), "").Replace("AppType." + AppType.ClientH.ToString(), "").Replace("|", "").Replace("\t", "").Trim() == "")
		{

			Log.Debug($"{fileName} {_scope} 配置不属于 Server");
			return;
		}

		string _exportDir = null;
		string _exportPath = null;
		string _protoName = Path.GetFileNameWithoutExtension(fileName);


		if (exportDir == ClientConfigPath)
        {
			if (relativePath == toyDirName)
			{
				_exportDir = string.Format(ClientConfigPath, toyDirName);
				_exportPath = Path.Combine(_exportDir, $"{_protoName}.txt");
			}
			else
			{
				_exportDir = Path.Combine(string.Format(ClientConfigPath, toyDirName),relativePath.Substring(toyDirName.Length+1));
				_exportPath = Path.Combine(_exportDir, $"{_protoName}.txt");
			}

			//先创建目录
			if (!Directory.Exists(_exportDir))
			{
				Directory.CreateDirectory(_exportDir);
			}

			//删除文件重建
			if (File.Exists(_exportPath))
			{
				File.Delete(_exportPath);
			}
		}
		else if (exportDir == ServerConfigPath)
		{
			_exportDir = Path.Combine(ServerConfigPath, relativePath);
			_exportPath = Path.Combine(_exportDir, $"{_protoName}.txt");

			//先创建目录
			if (!Directory.Exists(_exportDir))
			{
				Directory.CreateDirectory(_exportDir);
			}

			//删除文件重建
			if (File.Exists(_exportPath))
			{
				File.Delete(_exportPath);
			}
		}
        else
        {
			throw new Exception("导出目录有误");
        }

		
		Log.Info($"{_protoName}导表开始");

		using (FileStream txt = new FileStream(_exportPath, FileMode.Create))
		using (StreamWriter sw = new StreamWriter(txt))
		{
			for (int i = 0; i < xssfWorkbook.NumberOfSheets; ++i)
			{
				ISheet sheet = xssfWorkbook.GetSheetAt(i);
				ExportSheet(sheet, sw, fileName);
			}
		}
		Log.Info($"{_protoName}导表完成");
	}

	private void ExportSheet(ISheet sheet, StreamWriter sw, string fileName)
	{
		int cellCount = sheet.GetRow(3).LastCellNum;

		CellInfo[] cellInfos = new CellInfo[cellCount];

		for (int i = 2; i < cellCount; i++) //第二列开始
		{
			string fieldDesc = GetCellString(sheet, 2, i);  //列描述
			string fieldName = GetCellString(sheet, 3, i);  //列名
			string fieldType = GetCellString(sheet, 4, i);  //列类型
			cellInfos[i] = new CellInfo() { Name = fieldName, Type = fieldType, Desc = fieldDesc };
		}

		for (int i = 5; i <= sheet.LastRowNum; ++i) //第5行开始为实际数据
		{
			if (GetCellString(sheet, i, 2) == "") //首列值为空认为是空行
			{
				continue;
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			IRow row = sheet.GetRow(i);
			for (int j = 2; j < cellCount; ++j)
			{
				string desc = cellInfos[j].Desc.ToLower();

				if (desc.StartsWith("#")) continue; //列被注释
				if (desc.StartsWith("s") && this.isClient) continue; // s开头表示这个列是服务端专用
				if (desc.StartsWith("c") && !this.isClient) continue; // c开头表示这个字段是客户端专用

				string fieldValue = GetCellString(row, j);
				if (fieldValue == "" && cellInfos[j].Type != "string")
				{
					throw new Exception($"{fileName} sheet: {sheet.SheetName} 中有空白字段 {i},{j}");
				}

				if (j > 2)
				{
					sb.Append(",");
				}

				string fieldName = cellInfos[j].Name;

				if (fieldName == "Id" || fieldName == "_id")
				{
					if (this.isClient)
					{
						fieldName = "Id";
					}
					else
					{
						fieldName = "_id";
					}
				}

				string fieldType = cellInfos[j].Type;
				sb.Append($"\"{fieldName}\":{Convert(fieldType, fieldValue)}");
			}
			sb.Append("}");
			sw.WriteLine(sb.ToString());
		}
	}

	private static string Convert(string type, string value)
	{
		switch (type)
		{
			case "int[]":
			case "int32[]":
			case "long[]":
				return $"[{value}]";
			case "string[]":
				return $"[{value}]";
			case "int":
			case "int32":
			case "int64":
			case "long":
			case "float":
			case "double":
				return value;
			case "string":
				return $"\"{value}\"";
			default:
				throw new Exception($"不支持此类型: {type}");
		}
	}

	private static string GetCellString(ISheet sheet, int i, int j)
	{
		return sheet.GetRow(i)?.GetCell(j)?.ToString() ?? "";
	}

	private static string GetCellString(IRow row, int i)
	{
		return row?.GetCell(i)?.ToString() ?? "";
	}

	private static string GetCellString(ICell cell)
	{
		return cell?.ToString() ?? "";
	}
}

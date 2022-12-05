using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ETModel;

namespace ETTools
{
    internal class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    public static class Program
    {
        public const string HotfixMessageDirPath = "./HotfixMessage";
        public const string InnerMessageDirPath = "./InnerMessage";
        public const string OuterMessageDirPath = "./OuterMessage";

        public const string UnityOuterMessageDirPath = "../Unity/Assets/Model/Generated/OuterMessage";
        public const string UnityHotfixMessageDirPath = "../Unity/Assets/Hotfix/Generated/HotfixMessage";

        public const string ServerInnerMessageDirPath = "../Server/Model/Generated/InnerMessage";
        public const string ServerOutMessageDirPath = "../Server/Model/Generated/OuterMessage";
        public const string ServerHotfixMessageDirPath = "../Server/Model/Generated/HotfixMessage";

        public static void Main()
        {
            string protoc = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                protoc = "protoc.exe";
            }
            else
            {
                protoc = "protoc";
            }

            #region outer message
            
            if (Directory.Exists(UnityOuterMessageDirPath))
            {
                Directory.Delete(UnityOuterMessageDirPath, true);
            }
            Directory.CreateDirectory(UnityOuterMessageDirPath);


            string[] _outerProtoFiles = Directory.GetFiles(OuterMessageDirPath, "*.proto");
            int _outerProtoStartCode = 100;
            for (int i = 0; i < _outerProtoFiles.Length; i++)
            {
                string _outerProtoFileName = System.IO.Path.GetFileName(_outerProtoFiles[i]);
                ProcessHelper.Run(protoc, "--csharp_out=\"" + UnityOuterMessageDirPath + "\" --proto_path=\"" + OuterMessageDirPath + "\" " + _outerProtoFiles[i], waitExit: true);
                Proto2CS("ETModel", OuterMessageDirPath, _outerProtoFileName, UnityOuterMessageDirPath, "OuterOpcode", _outerProtoFileName.Substring(0, _outerProtoFileName.LastIndexOf(".proto")) + "Opcode",ref _outerProtoStartCode);
            }

            //拷贝到服务端
            if (Directory.Exists(ServerOutMessageDirPath))
            {
                Directory.Delete(ServerOutMessageDirPath, true);
            }
            Directory.CreateDirectory(ServerOutMessageDirPath);

            DirectoryInfo _outerMessageDirInfo = new DirectoryInfo(UnityOuterMessageDirPath);
            {
                foreach (FileInfo _fileInfo in _outerMessageDirInfo.GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    string _filePath = _fileInfo.FullName.Replace(_outerMessageDirInfo.FullName, "");
                    if (_filePath[0] == '\\') _filePath = _filePath.Substring(1);
                    _filePath.Replace('\\', '/');
                    _fileInfo.CopyTo(ServerOutMessageDirPath + "/" + _filePath);
                }
            }


            #endregion

            #region hotfix message
            if (Directory.Exists(UnityHotfixMessageDirPath))
            {
                Directory.Delete(UnityHotfixMessageDirPath, true);
            }
            Directory.CreateDirectory(UnityHotfixMessageDirPath);

            string[] _hotfixProtoFiles = Directory.GetFiles(HotfixMessageDirPath, "*.proto");
            int _hotfixProtoStartCode = 10000;
            for (int i = 0; i < _hotfixProtoFiles.Length; i++)
            {
                string _hotfixProtoFileName = System.IO.Path.GetFileName(_hotfixProtoFiles[i]);
                ProcessHelper.Run(protoc, "--csharp_out=\"" + UnityHotfixMessageDirPath + "\" --proto_path=\"" + HotfixMessageDirPath + "\" " + _hotfixProtoFiles[i], waitExit: true);
                Proto2CS("ETHotfix", HotfixMessageDirPath, _hotfixProtoFileName, UnityHotfixMessageDirPath, "HotfixOpcode", _hotfixProtoFileName.Substring(0, _hotfixProtoFileName.LastIndexOf(".proto")) + "Opcode",ref _hotfixProtoStartCode);
            }

            //拷贝到服务端
            if (Directory.Exists(ServerHotfixMessageDirPath))
            {
                Directory.Delete(ServerHotfixMessageDirPath, true);
            }
            Directory.CreateDirectory(ServerHotfixMessageDirPath);
            DirectoryInfo _hotfixMessageDirInfo = new DirectoryInfo(UnityHotfixMessageDirPath);
            {
                foreach (FileInfo _fileInfo in _hotfixMessageDirInfo.GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    string _filePath = _fileInfo.FullName.Replace(_hotfixMessageDirInfo.FullName, "");
                    if (_filePath[0] == '\\') _filePath = _filePath.Substring(1);
                    _filePath.Replace('\\', '/');
                    _fileInfo.CopyTo(ServerHotfixMessageDirPath + "/" + _filePath);
                }
            }

            #endregion

            #region inner message
            if (Directory.Exists(ServerInnerMessageDirPath))
            {
                Directory.Delete(ServerInnerMessageDirPath, true);
            }
            Directory.CreateDirectory(ServerInnerMessageDirPath);

            string[] _innerProtoFiles = Directory.GetFiles(InnerMessageDirPath, "*.proto");
            int _innerProtoStartCode = 1000;
            for (int i = 0; i < _innerProtoFiles.Length; i++)
            {
                string _innerProtoFileName = System.IO.Path.GetFileName(_innerProtoFiles[i]);
                InnerProto2CS.Proto2CS("ETModel", _innerProtoFiles[i], ServerInnerMessageDirPath, "InnerOpcode",ref _innerProtoStartCode);
                InnerProto2CS.GenerateOpcode("ETModel", "InnerOpcode", System.IO.Path.GetFileNameWithoutExtension(_innerProtoFileName) + "Opcode", ServerInnerMessageDirPath);
            }

            #endregion


            Console.WriteLine("proto2cs succeed!");
        }

        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2CS(string ns, string protoPath, string protoName, string outputPath, string opcodeClassName, string opcodeOutputFileName,ref int startOpcode, bool isClient = true)
        {
            msgOpcode.Clear();
            string proto = Path.Combine(protoPath, protoName);

            string s = File.ReadAllText(proto);

            StringBuilder sb = new StringBuilder();
            sb.Append("using ETModel;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            bool isMsgStart = false;

            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"{newline}\n");
                }

                if (newline.StartsWith("message"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }
                    else
                    {
                        parentClass = "";
                    }

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                    sb.Append($"\t[Message({opcodeClassName}.{msgName})]\n");
                    sb.Append($"\tpublic partial class {msgName} ");
                    if (parentClass != "")
                    {
                        sb.Append($": {parentClass} ");
                    }

                    sb.Append("{}\n\n");
                }

                if (isMsgStart && newline == "}")
                {
                    isMsgStart = false;
                }
            }

            sb.Append("}\n");

            GenerateOpcode(ns, opcodeClassName, opcodeOutputFileName, outputPath, sb);
        }

        private static void GenerateOpcode(string ns, string opcodeClassName, string outputFileName, string outputPath, StringBuilder sb)
        {
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static partial class {opcodeClassName}");
            sb.AppendLine("\t{");
            foreach (OpcodeInfo info in msgOpcode)
            {
                sb.AppendLine($"\t\t public const ushort {info.Name} = {info.Opcode};");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, outputFileName + ".cs");
            File.WriteAllText(csPath, sb.ToString());
        }
    }

    public static class InnerProto2CS
    {
        //private const string protoPath = ".";
        //private const string serverMessagePath = "../Server/Model/Module/Message/";
        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2CS()
        {
            //msgOpcode.Clear();
            //Proto2CS("ETModel", "InnerMessage.proto", serverMessagePath, "InnerOpcode", 1000);
            //GenerateOpcode("ETModel", "InnerOpcode", serverMessagePath);
        }

        public static void Proto2CS(string ns, string protoFilePath, string outputPath, string opcodeClassName,ref int startOpcode)
        {
            msgOpcode.Clear();
            //string proto = Path.Combine(protoPath, protoName);
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(protoFilePath) + ".cs");

            string s = File.ReadAllText(protoFilePath);

            StringBuilder sb = new StringBuilder();
            sb.Append("using ETModel;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            bool isMsgStart = false;
            string parentClass = "";
            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"{newline}\n");
                }

                if (newline.StartsWith("message"))
                {
                    parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                    sb.Append($"\t[Message({opcodeClassName}.{msgName})]\n");
                    sb.Append($"\tpublic partial class {msgName}");
                    if (parentClass == "IActorMessage" || parentClass == "IActorRequest" || parentClass == "IActorResponse" ||
                        parentClass == "IFrameMessage")
                    {
                        sb.Append($": {parentClass}\n");
                    }
                    else if (parentClass != "")
                    {
                        sb.Append($": {parentClass}\n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    continue;
                }

                if (isMsgStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");
                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.AppendLine(newline);
                        continue;
                    }

                    if (newline.Trim() != "" && newline != "}")
                    {
                        if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, ns, newline);
                        }
                        else
                        {
                            Members(sb, newline, true);
                        }
                    }
                }
            }

            sb.Append("}\n");

            File.WriteAllText(csPath, sb.ToString());
        }

        public static void GenerateOpcode(string ns, string opcodeClassName, string outputFileName, string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static partial class {opcodeClassName}");
            sb.AppendLine("\t{");
            foreach (OpcodeInfo info in msgOpcode)
            {
                sb.AppendLine($"\t\t public const ushort {info.Name} = {info.Opcode};");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, outputFileName + ".cs");
            File.WriteAllText(csPath, sb.ToString());
        }

        private static void Repeated(StringBuilder sb, string ns, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                if (ss[2].Contains('=')) ss[2] = ss[2].Substring(0, ss[2].IndexOf('='));
                string name = ss[2];

                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }

        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[0];
                if (ss[1].Contains('=')) ss[1] = ss[1].Substring(0, ss[1].IndexOf('='));
                string name = ss[1];

                string typeCs = ConvertType(type);

                sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }
    }
}

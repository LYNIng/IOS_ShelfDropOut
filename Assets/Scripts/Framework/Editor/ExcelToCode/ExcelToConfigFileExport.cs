using FileExport;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using static FileExport.FileExportBase;
using static ReadExcel.ExcelSourceData;
using Debug = UnityEngine.Debug;
using NUnit.Framework.Constraints;
using Codice.Client.BaseCommands;

namespace ReadExcel
{

    /// <summary>
    /// 读取器
    /// </summary>
    public class ExcelFileReader : SourceFileReader
    {
        public override bool IsIgnoreFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return IsTemporaryFile(fileName);
        }

        private static bool IsTemporaryFile(string fileName)
        {
            return Regex.Match(fileName, @"^\~\$").Success;
        }

        public override bool ReadFile(string path, out object result)
        {
            result = null;
            var fileName = Path.GetFileName(path);
            //var exaName = Path.GetExtension(path);
            //if(IsTemporaryFile(fileName))
            //{
            //    return false;
            //}
            SheetSource sheet = null;
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                //可以同时读流
                FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //读取Excel
                ExcelPackage package = new ExcelPackage(fileStream);
                ExcelWorksheet excelData = package.Workbook.Worksheets[0];
                fileStream.Close();

                sheet = SheetParser.Parse(excelData, fileInfo.Name);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                sheet = null;
            }

            if (sheet != null)
            {
                //传递给解析器解析
                result = sheet;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
    public class ExcelSourceData
    {
        public string[,] dataArr;
        public int columns;
        public int rows;

        public int keyCol;
        public int valueNameCol;
        /// <summary>
        /// 配置的设定
        /// </summary>
        public class ExcelSettingItem
        {
            public string _des;
            public string _port;
            public string _typeName;
            public string _name;
            public bool isKey;
            public bool isValueName;
            public bool isIgnore;
            public int index;
        }

        public Dictionary<int, ExcelSettingItem> colDefineDict = new Dictionary<int, ExcelSettingItem>();//列的设定参数
    }


    /// <summary>
    /// 解析器
    /// </summary>
    public class ExcelSourceFileResolver : SourceFileResolver
    {
        //关键字检测
        private readonly static Dictionary<string, int> keyworldDict = new Dictionary<string, int>
        {
            { "key",1},
            { "valuename",1<<1},
        };

        //特殊行处理逻辑
        private readonly Dictionary<int, Func<string, int, ExcelSettingItem, ExcelSourceData, bool>> rowLogic = new Dictionary<int, Func<string, int, ExcelSettingItem, ExcelSourceData, bool>>
        {
            { 2, ReadDes },
            { 3, ReadPort },
            { 4, ReadName },
            { 5, ReadTypeName },
        };
        private static bool ReadTypeName(string str, int col, ExcelSettingItem item, ExcelSourceData data)
        {
            //第六行需要判定 一些使用 | 字符隔开的 关键字 例如 key 和 valueName
            var arr = str.Split('|');
            if (arr.Length > 0)
            {
                item._typeName = arr[0];
            }
            var mask = GetKeywordMask(arr);
            if (IsKey(mask))
            {
                item.isKey = true;
            }
            else if (IsValueName(mask))
            {
                data.valueNameCol = col;
            }
            return true;
        }
        private static bool ReadName(string str, int col, ExcelSettingItem item, ExcelSourceData data)
        {
            item._name = str;
            if (str[0] == '*')
                item.isIgnore = true;
            return true;
        }
        private static bool ReadPort(string str, int col, ExcelSettingItem item, ExcelSourceData data)
        {
            item._port = str;
            return true;
        }
        private static bool ReadDes(string str, int col, ExcelSettingItem item, ExcelSourceData data)
        {
            item._des = str;
            return true;
        }
        public override bool Resolve(object readData, out object result)
        {
            result = null;
            var st = readData as SheetSource;
            if (st == null) return false;

            ExcelSourceData exportData = new ExcelSourceData();
            int row = exportData.rows = st.row;
            int columns = exportData.columns = st.column;
            exportData.dataArr = new string[columns, row];
            int count = 0;
            exportData.keyCol = -1;
            exportData.valueNameCol = -1;
            for (int j = 0; j < columns; ++j)
            {
                var item = new ExcelSettingItem();
                for (int i = 0; i < row; ++i)
                {
                    string readStr = null;
                    if (!st.matrix[i, j].IsNullOrEmpty())
                    {
                        readStr = st.matrix[i, j].ToString().Trim();
                    }
                    else
                    {
                        readStr = "";
                    }
                    exportData.dataArr[j, i] = readStr;

                    if (!string.IsNullOrEmpty(readStr) && rowLogic.TryGetValue(i, out var func))
                    {
                        func?.Invoke(readStr, j, item, exportData);
                    }
                }
                if (!item.isIgnore)
                    item.index = count++;
                if (item.isKey)
                {
                    exportData.keyCol = j;
                }
                if (item.isValueName)
                {
                    exportData.valueNameCol = j;
                }
                exportData.colDefineDict.Add(j, item);
            }

            //传递给生成器去生成文件
            result = exportData;
            return true;
        }


        private static int GetKeywordMask(string[] arr)
        {
            if (arr == null || arr.Length <= 1) return 0;
            int resultMask = 0;
            for (int i = 0; i < arr.Length; ++i)
            {
                var tempStr = arr[i];
                if (string.IsNullOrEmpty(tempStr)) continue;

                if (keyworldDict.TryGetValue(tempStr.ToLower(), out int value))
                {
                    resultMask = resultMask | value;
                }
            }
            return resultMask;
        }
        private static bool IsKey(int mask)
        {
            return CheckKeyWord("key", mask);
        }
        private static bool IsValueName(int mask)
        {
            return CheckKeyWord("valuename", mask);
        }
        private static bool CheckKeyWord(string keyWord, int mask)
        {
            if (keyworldDict.TryGetValue(keyWord, out var value))
            {
                return (mask & value) != 0;
            }
            return false;
        }

    }

    /// <summary>
    /// 配置数据生成器
    /// </summary>
    public class ExcelConfigDataMaker : SourceFileDataMaker
    {
        private string _exportPath = EditorPathHelper.Export_ExcelConfigsDataFolderPath;
        private bool isDispersived { get; set; } = true;

        /// <summary>
        /// Excel数据生成器
        /// </summary>
        /// <param name="isDispersived">true 分开文件,false 一个文件</param>
        public ExcelConfigDataMaker(bool isDispersived)
        {
            this.isDispersived = isDispersived;
        }

        public override bool GenerateFile(string exportDir, Dictionary<string, FileExportData> exportData)
        {
            if (isDispersived)
            {
                Dispersive(exportDir, exportData);
            }
            else
            {
                Agminated(exportDir, exportData);
            }
            return false;
        }

        /// <summary>
        /// 每个文件分开生成
        /// </summary>
        private void Dispersive(string exportDir, Dictionary<string, FileExportData> exportData)
        {
            foreach (var kvp in exportData)
            {
                var fex = kvp.Value;
                var excelData = fex.sourceResolveData as ExcelSourceData;
                var fileName = fex.fileInfo.Name.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];

                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                }

                int row = excelData.rows;
                int col = excelData.columns;
                var dataArr = excelData.dataArr;

                StringBuilder sbr = new StringBuilder();
                int endCol = col - 1;
                for (int i = 6; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        var tmp = dataArr[j, 4];
                        //第六行带*号将会跳过
                        if (tmp[0] == '*')
                        {
                            if (j == endCol)
                            {
                                sbr.Append("\n");
                            }
                            continue;
                        }
                        if (j == 0)
                        {
                            var st = dataArr[j, i];

                            if (string.IsNullOrEmpty(st))
                            {
                                //Debug.Log($"Ignore Excel Data :{j} - {i}");
                                break;
                            }
                        }


                        var s = dataArr[j, i].Trim().Replace("\n", "\\n").Replace("/r/n", "\\n");
                        sbr.Append(s);

                        if (j == endCol)
                        {
                            sbr.Append("\n");
                        }
                        else
                        {
                            sbr.Append("__");
                        }
                    }
                }

                string str = null;
                str = string.Format("{0}/{1}.txt", _exportPath, fileName);
                using (FileStream fs = new FileStream(str, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(sbr.ToString().TrimEnd());
                    }
                }


            }


        }

        /// <summary>
        /// 只存储在一个文件里面
        /// </summary>
        private void Agminated(string exportDir, Dictionary<string, FileExportData> exportData)
        {

        }

    }

    /// <summary>
    /// 配置数据代码生成器
    /// </summary>
    public class ExcelConfigCodeMaker : SourceFileDataMaker
    {
        public string exportPath { get; set; } = $"{EditorPathHelper.Export_ExcelConfigsScriptsFolderPath}";

        private bool IsIgnoreName(string name)
        {
            return name[0] == '*';
        }

        public override bool GenerateFile(string exportDir, Dictionary<string, FileExportData> exportData)
        {
            foreach (var kvp in exportData)
            {
                var fex = kvp.Value;
                var fileName = fex.fileInfo.Name.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];
                string codeExportPath = string.Format(exportPath, fileName);

                var excelData = fex.sourceResolveData as ExcelSourceData;

                var dataArr = excelData.dataArr;
                var hasKey = excelData.keyCol != -1;
                int keyCol = excelData.keyCol;
                int valueCol = excelData.valueNameCol; //  如果为-1 则是没有定义valueName
                string finalExportDir = exportPath;

                var t_key = "";
                string keyName = "";
                if (excelData.colDefineDict.TryGetValue(keyCol, out var keyItem))
                {
                    t_key = keyItem._typeName;
                    keyName = keyItem._name;
                }
                else
                {
                    t_key = "int";
                }


                StringBuilder sbr = new StringBuilder();

                CodeHelper.GenerateTitle(sbr);
                sbr.Append("using System.Collections.Generic;\r\n");
                sbr.Append("using System.Threading.Tasks;\r\n");
                sbr.Append("using UnityEngine;\r\n");
                CodeHelper.GenerateClassCodeLine(sbr, fileName, alignCount: 0);
                CodeHelper.GenerateActionScopeBegin(sbr, 0);

                if (hasKey)
                {
                    CodeHelper.AlignSpace(sbr, 1);
                    sbr.Append($"protected bool {fileName}_HasKey => true;\r\n");
                }
                else
                {
                    CodeHelper.AlignSpace(sbr, 1);
                    sbr.Append($"protected bool {fileName}_HasKey => false;\r\n");
                }

                var m_dictData = $"m_{fileName}dict";
                var m_IDX_KEY = $"m_{fileName}IDX_KEY";
                var T_Data = $"{fileName}Data";

                var createDataFuncName = $"Create{fileName}Data";


                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"protected Dictionary<{t_key}, {T_Data}> {m_dictData};\r\n");
                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"private Dictionary<int, {t_key}> {m_IDX_KEY};\r\n");

                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"public int DataCount => {m_dictData}.Count;\r\n");

                CodeHelper.Property.GenerateProperty_GetSet(sbr, 1, "bool", "IsLoaded",
                    setPropertyModifier: CodeHelper.Modifier.Protected);

                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"private AssetLoader<TextAsset> textAssetLoader;\r\n");

                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"public async Task AsyncInit{fileName}()\r\n");
                CodeHelper.GenerateActionScope(sbr, 1, (aCount) =>
                {
                    CodeHelper.AlignSpace(sbr, aCount);
                    sbr.Append($"textAssetLoader = new AssetLoader<TextAsset>($\"Configs/{{GetType().Name}}.txt\");\r\n");
                    CodeHelper.AlignSpace(sbr, aCount);
                    sbr.Append($"await textAssetLoader.AsyncLoad();\r\n");
                    CodeHelper.AlignSpace(sbr, aCount);
                    sbr.Append($"LangCfgDataLoaded(textAssetLoader.Asset.text);\r\n");

                });

                sbr.Append($"    private void {fileName}DataLoaded(string d)\r\n" +
                    $"    {{\r\n" +
                    $"        {m_dictData} = new Dictionary<{t_key}, {T_Data}>();\r\n" +
                    $"        {m_IDX_KEY} = new Dictionary<int, {t_key}>();\r\n" +
                    $"        string[] allLines = d.Split('\\n');\r\n" +
                    $"        for (int index = 0; index < allLines.Length; ++index)\r\n" +
                    $"        {{\r\n" +
                    $"            string content = allLines[index].Trim();\r\n" +
                    $"\r\n" +
                    $"            var strArr = content.Split(\"__\", System.StringSplitOptions.RemoveEmptyEntries);\r\n" +
                    $"\r\n" +
                    $"            if ({createDataFuncName}(strArr, out var mKey, out var mData))\r\n" +
                    $"            {{\r\n" +
                    $"                if ({fileName}_HasKey)\r\n" +
                    $"                {{\r\n" +
                    $"                    if ({m_dictData}.TryAdd(mKey, mData))\r\n" +
                    $"                    {{\r\n" +
                    $"                        {m_IDX_KEY}.Add(index, mKey);\r\n" +
                    $"                        //读取成功\r\n" +
                    $"                        //Debug.Log($\"读取成功 {{mKey}} {{mData}}\");\r\n" +
                    $"                    }}\r\n" +
                    $"                    else\r\n" +
                    $"                    {{\r\n" +
                    $"                        Debug.Log($\"添加到容器失败 {{mKey}}\");\r\n" +
                    $"                    }}\r\n" +
                    $"                }}\r\n" +
                    $"                else\r\n" +
                    $"                {{\r\n" +
                    $"                    Debug.Log($\"没有 Key 检查Excel 并定义Key\");\r\n" +
                    $"                }}\r\n" +
                    $"\r\n" +
                    $"            }}\r\n" +
                    $"            else\r\n" +
                    $"            {{\r\n" +
                    $"                Debug.Log($\"创建Data失败 {{string.Concat(strArr)}}\");\r\n" +
                    $"            }}\r\n" +
                    $"        }}\r\n" +
                    $"    }}\r\n");

                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"public {fileName}.{T_Data} Get{fileName}Data({t_key} id)\r\n");
                CodeHelper.GenerateActionScope(sbr, 1, (cCount) =>
                {
                    CodeHelper.AlignSpace(sbr, cCount);
                    sbr.Append($"return {m_dictData}[id];\r\n");
                });


                CodeHelper.GenerateConstructor(sbr, 1, CodeHelper.Modifier.Public, fileName, (s, agcnt) =>
                {
                    CodeHelper.AlignSpace(sbr, agcnt);
                    sbr.Append($"if (callInit) _ = AsyncInit{fileName}();\r\n");
                }, insertParamStr: "bool callInit = true");

                string enumValueName = null;
                string enumValueType = null;
                //int enumValueCol = 0;
                //创建数值Struct
                CodeHelper.Struct.GenrateStructCode(sbr, $"{fileName}Data", 1, (sbr2, count2) =>
                {

                    foreach (var tempkvp in excelData.colDefineDict)
                    {
                        var typeName = tempkvp.Value._typeName;
                        var valueName = tempkvp.Value._name;
                        if (!IsIgnoreName(valueName))
                        {
                            CodeHelper.GenerateMemberValue(sbr, count2, CodeHelper.Modifier.Public, typeName, valueName);

                            if (string.IsNullOrEmpty(enumValueName) && tempkvp.Value.isValueName)
                            {
                                enumValueName = valueName;
                                enumValueType = typeName;
                            }
                        }

                    }
                });

                if (valueCol > -1 && excelData.colDefineDict.TryGetValue(valueCol, out var valueNameItem))
                {
                    CodeHelper.GenerateClassCode(sbr, "ValueName", contentAction: (alignCount) =>
                    {
                        for (int row = 6; row < excelData.rows; ++row)
                        {
                            var enumName = excelData.dataArr[valueCol, row];
                            var enumValue = excelData.dataArr[keyCol, row];
                            if (!string.IsNullOrEmpty(enumName))
                                CodeHelper.GenerateConstValue(sbr, alignCount, CodeHelper.Modifier.Public, t_key, enumName, enumValue);
                        }
                    });

                }

                CodeHelper.AlignSpace(sbr, 1);
                sbr.Append($"protected bool {createDataFuncName}(string[] strArr, out {t_key} resultKey, out {fileName}Data resultData)\r\n");
                CodeHelper.GenerateActionScopeBegin(sbr, 1);
                CodeHelper.AlignSpace(sbr, 1 + 1);
                sbr.Append($"resultData = new {T_Data}();\r\n");
                foreach (var tempkvp in excelData.colDefineDict)
                {
                    var typeName = tempkvp.Value._typeName;
                    var valueName = tempkvp.Value._name;
                    var index = tempkvp.Value.index;
                    if (!IsIgnoreName(valueName))
                    {
                        CodeHelper.ExcelBaseData.GenerateReadData(sbr, 1 + 1, typeName, $"resultData.{valueName}", index);
                    }
                }
                CodeHelper.AlignSpace(sbr, 1 + 1);
                sbr.Append($"resultKey = resultData.{keyName};\r\n");
                CodeHelper.AlignSpace(sbr, 1 + 1);
                sbr.Append($"return true;\r\n");
                CodeHelper.GenerateActionScopeEnd(sbr, 1);

                foreach (var tempKvp in excelData.colDefineDict)
                {
                    var typeName = tempKvp.Value._typeName;
                    var valueName = tempKvp.Value._name;
                    var index = tempKvp.Value.index;
                    var isKey = tempKvp.Value.isKey;
                    var des = tempKvp.Value._des;

                    if (IsIgnoreName(valueName))
                    {
                        continue;
                    }
                    if (isKey)
                    {
                        //创建GetKey函数
                        CodeHelper.GenerateAnnotation(sbr, 1, des);
                        //CodeHelper.ExcelBaseData.GenerateGetKeyByIndexFunc(sbr, 1, t_key, keyName);
                        CodeHelper.AlignSpace(sbr, 1);
                        sbr.Append($"public {t_key} Get{keyName}ByIndex(int idx) => {m_IDX_KEY}[idx];\r\n");
                    }
                    else
                    {
                        //创建GetValue函数
                        CodeHelper.GenerateAnnotation(sbr, 1, des);
                        CodeHelper.AlignSpace(sbr, 1);
                        sbr.Append($"public {typeName} Get{valueName}By{keyName}({t_key} {keyName}) => {m_dictData}[{keyName}].{valueName};\r\n");
                        //CodeHelper.ExcelBaseData.GenerateGetValueByKeyNameFunc(sbr, 1, typeName, valueName, t_key, keyName);

                    }
                }

                CodeHelper.GenerateActionScopeEnd(sbr, 0);

                if (!Directory.Exists(finalExportDir))
                {
                    Directory.CreateDirectory(finalExportDir);
                }
                string str = null;
                str = $"{finalExportDir}/{fileName}.cs";

                using (FileStream fs = new FileStream(str, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(sbr.ToString().TrimEnd());
                    }
                }

            }

            return true;
        }
    }

    public class ExcelLanguageCharacterFileMaker : SourceFileDataMaker
    {
        public string exportPath { get; set; } = $"{EditorPathHelper.Editor_ExcelCharacterFileFolderPath}";

        List<string> readFile = new List<string>
        {
            "LanguageConfigs",
            "LanguageConfigsB",
        };

        public override bool GenerateFile(string exportDir, Dictionary<string, FileExportData> exportData)
        {
            foreach (var kvp in exportData)
            {
                var fex = kvp.Value;
                var fileName = fex.fileInfo.Name.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];

                if (!readFile.Contains(fileName)) continue;

                var excelData = kvp.Value.sourceResolveData as ExcelSourceData;

                int row = excelData.rows;
                int col = excelData.columns;
                var dataArr = excelData.dataArr;

                StringBuilder sbr = new StringBuilder();

                string res = "";

                int endCol = col - 1;

                for (int j = 1; j < col; j++)
                {
                    var tmp = dataArr[j, 4];
                    if (tmp[0] == '*')
                        continue;
                    var languageType = tmp;
                    for (int i = 6; i < row; i++)
                    {
                        var s = dataArr[j, i];

                        foreach (var c in s)
                        {
                            if (!res.Contains(c))
                            {
                                res += c;
                            }
                        }
                    }

                    if (!Directory.Exists(exportPath))
                    {
                        Directory.CreateDirectory(exportPath);
                    }

                    sbr.Append(res);
                    var str = $"{exportPath}/{fileName}_{languageType}.txt";
                    using (FileStream fs = new FileStream(str, FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(sbr.ToString().TrimEnd());
                        }
                    }
                    res = "";
                    sbr.Clear();
                }

            }

            return true;
        }
    }

    /// <summary>
    /// Excel装配器
    /// 读取器 ExcelFileReader
    /// 解析器 ExcelSourceFilerrResolver
    /// 生成器 
    ///  -- 数据生成 ExcelConfigDataMaker,
    ///  -- 代码生成 ExcelConfigCodeMaker
    /// </summary>
    public class ExcelToConfigFileExport : FileExportBase
    {
        public const string C_LINKNAME = "Configs";
        public override HashSet<string> fileTypeFilterater => new HashSet<string> { ".xlsx" };

        public override string LinkName => C_LINKNAME;

        public ExcelToConfigFileExport() :
            base(new ExcelFileReader(),//excel读取器
                new ExcelSourceFileResolver(),//excel 解析器
                new SourceFileDataMaker[] {
                    new ExcelConfigDataMaker(true),
                    new ExcelConfigCodeMaker()
                    //,new ExcelLanguageCharacterFileMaker() 
                }//excel 配置文件生成器 和 代码生成器
                )
        {

        }
    }


}


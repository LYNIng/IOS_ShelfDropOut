#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System;
using OfficeOpenXml;

namespace ReadExcel
{
    public class ExportExcelData
    {

        static StringBuilder configList;

        static StringBuilder dataList;

        //[MenuItem("ExcelTool/导出表格数据", false, 10)]
        //public static void ExportData()
        //{
        //    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        //    string[] dirs = new string[]{
        //        EditorPathHelper.Editor_ExcelConfigsFolderPath,
        //    };

        //    ExportConfigs(dirs);

        //    var dataDirs = new string[]
        //    {
        //        EditorPathHelper.Editor_ExcelDataDefineFolderPath,
        //    };

        //    ExportDefineData(dataDirs);
        //}

        static void ExportConfigs(string[] dirs)
        {
            configList = new StringBuilder();
            configCodeHelperDict = new Dictionary<string, GenerateConfigCodeHelper>();
            List<string> arrlist = new List<string>();

            for (int i = 0; i < dirs.Length; i++)
            {
                string temp;
                GetAllFile(dirs[i], out temp);
                string[] arr1 = temp.Trim().Split('\r', '\n');
                for (int j = 0; j < arr1.Length; j++)
                {
                    if (!arrlist.Contains(arr1[j]))
                        arrlist.Add(arr1[j]);
                }
            }

            string[] arr = arrlist.ToArray();
            if (arr.Length > 0)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    var excelName = System.IO.Path.GetFileName(arr[i]);
                    if (IsTemporaryFile(excelName))
                    {
                        Debug.Log("无效的excel配置文件已自动过滤" + excelName);
                        continue;
                    }
                    try
                    {
                        ReadConfigData(arr[i]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message + arr[i]);
                    }
                }

                if (!Directory.Exists(EditorPathHelper.Editor_ResDev_DataPath))
                {
                    Directory.CreateDirectory(EditorPathHelper.Editor_ResDev_DataPath);
                }
                string str = EditorPathHelper.Editor_ResDev_DataPath + "/ABeforeLoadData.txt";
                using (FileStream fs = new FileStream(str, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string tempStr = configList.ToString().TrimEnd();
                        sw.Write(tempStr);
                    }
                }


            }

            var configs = new GenerateConfigurationsCode(configList, EditorPathHelper.Editor_HotUpdate_AutoCode_GenerateCodePath);
            configs.Generate();


            AssetDatabase.Refresh();
            Debug.Log("导表完成");

        }

        static void ExportDefineData(string[] dirs)
        {
            dataList = new StringBuilder();
            var arrList = new List<string>();

            for (int i = 0; i < dirs.Length; ++i)
            {
                string temp;
                GetAllFile(dirs[i], out temp);
                string[] arr1 = temp.Trim().Split('\r', '\n');
                for (int j = 0; j < arr1.Length; j++)
                {
                    if (!arrList.Contains(arr1[j]))
                        arrList.Add(arr1[j]);
                }
            }

            if (arrList.Count > 0)
            {
                for (int i = 0; i < arrList.Count; i++)
                {
                    var path = arrList[i];
                    var excelName = System.IO.Path.GetFileName(path);
                    if (IsTemporaryFile(excelName))
                    {
                        Debug.Log("无效的excel配置文件已自动过滤" + excelName);
                        continue;
                    }
                    try
                    {
                        ReadDefineData(path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message + path);
                    }
                }

                AnalysisManagerDataCode();

                CreateDefineDataCode();

            }

            AssetDatabase.Refresh();
            Debug.Log("生成对象完成");
        }

        //public static void CommonExportData()
        //{
        //    string DirPath = Application.dataPath + "/Excel";
        //    string temp;
        //    GetAllFile(DirPath, out temp);
        //    string[] arr = temp.Trim().Split('\r', '\n');


        //    if (arr.Length > 0)
        //    {
        //        for (int i = 0; i < arr.Length; i++)
        //        {
        //            try
        //            {
        //                ReadConfigData(arr[i]);
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.LogError(e.Message);
        //                return;
        //            }
        //        }

        //        Debug.Log("导表完成");
        //    }

        //}


        /// <summary>
        /// like ~$Equip
        /// </summary>
        /// <returns></returns>
        private static bool IsTemporaryFile(string fileName)
        {
            return Regex.Match(fileName, @"^\~\$").Success;
        }


        /// <summary>
        /// 获取源
        /// </summary>
        /// <returns></returns>
        private static void GetSources(string filePath, out SheetSource sheet)
        {
            try
            {

                FileInfo file = new FileInfo(filePath);
                if (IsTemporaryFile(file.Name))//临时文件
                {
                    sheet = null;
                    return;
                }

                //可以同时读流
                FileStream fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //读取Excel
                ExcelPackage package = new ExcelPackage(fileStream);
                ExcelWorksheet excelData = package.Workbook.Worksheets[0];
                fileStream.Close();


                sheet = SheetParser.Parse(excelData, file.Name);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                sheet = null;
            }
        }

        private static void ReadConfigData(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            string[] strs = path.Split('.');
            string fileType = strs[strs.Length - 1];

            if (fileType == "xlsx")
            {
                SheetSource structs;
                GetSources(path, out structs);
                CreateConfigCode(path, structs);
            }
        }

        private static void ReadDefineData(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            string[] strs = path.Split('.');
            string fileType = strs[strs.Length - 1];

            if (fileType == "xlsx")
            {
                SheetSource structs;
                GetSources(path, out structs);
                AnalysisDefineDataCode(path, structs);
            }
        }

        private static void CreateConfigCode(string path, SheetSource st)
        {
            string filePath = path.Substring(0, path.LastIndexOf('\\') + 1);
            string fileFullName = path.Substring(path.LastIndexOf('\\') + 1);
            var spArr = filePath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            string folderName = spArr[spArr.Length - 1];
            string fileName = fileFullName.Substring(0, fileFullName.LastIndexOf('.'));

            string[,] dataArr = null;

            int row, columns;
            row = st.row;
            columns = st.column;

            dataArr = new string[columns, row];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (!st.matrix[i, j].IsNullOrEmpty())
                    {
                        dataArr[j, i] = st.matrix[i, j].ToString().Trim();
                        //ms.WriteUTF8String(st.matrix[i, j].ToString().Trim());
                    }
                    else
                    {
                        dataArr[j, i] = "";
                        //ms.WriteUTF8String("");
                    }
                }
            }
            //using (MemoryStreamHelper ms = new MemoryStreamHelper())
            //{
            //    //ms.WriteInt(row);
            //    //ms.WriteInt(columns);
            //}

            var gData = new GenerateDataHelper(Application.dataPath + "/Res_Dev", fileName, dataArr, row, columns);
            gData.Generate();
            var cData = new GenerateConfigCodeHelper(Application.dataPath + "/HotUpdate/", fileName, dataArr, row, columns);
            cData.Generate();
            configCodeHelperDict.Add(cData.fileName, cData);
            configList.AppendLine(fileName);


        }

        public static Dictionary<string, GenerateConfigCodeHelper> configCodeHelperDict;
        public static Dictionary<string, GenerateDefineDataCodeHelper> generateDefineDataCodeHelperDict;
        public static GenerateDefineDataFactoryCodeHelper factoryCodeHelper;

        public static Dictionary<string, GenerateManagerFileCodeHelper> managerCodeHelperDict;
        private static void AnalysisDefineDataCode(string path, SheetSource st)
        {

            string filePath = path.Substring(0, path.LastIndexOf('\\') + 1);
            string fileFullName = path.Substring(path.LastIndexOf('\\') + 1);
            var spArr = filePath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            string folderName = spArr[spArr.Length - 1];
            string fileName = fileFullName.Substring(0, fileFullName.LastIndexOf('.'));

            string[,] dataArr = null;

            int row, columns;
            row = st.row;
            columns = st.column;

            dataArr = new string[columns, row];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (!st.matrix[i, j].IsNullOrEmpty())
                    {
                        dataArr[j, i] = st.matrix[i, j].ToString().Trim();
                    }
                    else
                    {
                        dataArr[j, i] = "";
                    }
                }
            }

            if (generateDefineDataCodeHelperDict == null)
            {
                generateDefineDataCodeHelperDict = new Dictionary<string, GenerateDefineDataCodeHelper>();
            }

            var dCode = new GenerateDefineDataCodeHelper(Application.dataPath + "/HotUpdate/", fileName, dataArr, row, columns);
            dCode.Analysis();
            generateDefineDataCodeHelperDict.Add(fileName, dCode);
            factoryCodeHelper = new GenerateDefineDataFactoryCodeHelper(EditorPathHelper.Editor_HotUpdate_AutoCode_GenerateCodePath, "DefineObjectFactory");

        }

        private static void AnalysisManagerDataCode()
        {
            #region Code manager
            if (managerCodeHelperDict == null)
            {
                managerCodeHelperDict = new Dictionary<string, GenerateManagerFileCodeHelper>();
            }
            Dictionary<string, List<GenerateDefineDataCodeHelper>> dict = new Dictionary<string, List<GenerateDefineDataCodeHelper>>();
            foreach (var kvp in generateDefineDataCodeHelperDict)
            {
                //遍历获取DefineCode 中的manager名称
                var item = kvp.Value;
                if (!string.IsNullOrEmpty(item.managerName))
                {
                    List<GenerateDefineDataCodeHelper> list = null;
                    if (!dict.TryGetValue(item.managerName, out list))
                    {
                        list = new List<GenerateDefineDataCodeHelper>();
                        dict.Add(item.managerName, list);
                    }
                    list.Add(item);
                }
            }

            foreach (var kvp in dict)
            {
                var manager = new GenerateManagerFileCodeHelper(EditorPathHelper.Editor_HotUpdate_AutoCode_GenerateCodePath, kvp.Key, kvp.Value);
                managerCodeHelperDict.Add(kvp.Key, manager);
                manager.Analysis();
            }



            #endregion

        }

        private static void CreateDefineDataCode()
        {
            foreach (var kvp in generateDefineDataCodeHelperDict)
            {
                var code = kvp.Value;
                code.Generate();
            }

            foreach (var kvp in managerCodeHelperDict)
            {
                var code = kvp.Value;
                code.Generate();
            }

            factoryCodeHelper.Generate();
            //generateDefineDataCodeHelperDict.Clear();
            //managerCodeHelperDict.Clear();
        }

        /// <summary>
        /// 将所有文件路径整合在一起
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="allfile"></param>
        private static void GetAllFile(string dirPath, out string allfile)
        {
            allfile = "";
            DirectoryInfo dirs = new DirectoryInfo(dirPath);
            FileInfo[] file = dirs.GetFiles();//获得目录下文件对          
                                              //循环文件
            for (int j = 0; j < file.Length; j++)
            {
                if (file[j].Extension == ".xlsx")
                {
                    allfile += dirPath + "/" + file[j].Name + "\r\n";
                }
            }
            allfile = allfile.Replace(@"/", "\\");

        }


    }

    public class GenerateDataHelper
    {
        private string filePath;
        private string fileName;
        private string[,] dataArr;
        private int row;
        private int column;
        static StringBuilder versionSub = new StringBuilder();
        static Dictionary<string, string> m_DicVersion = new Dictionary<string, string>();
        public GenerateDataHelper(string filePath, string fileName, string[,] dataArr, int row, int column)
        {
            this.filePath = filePath;
            this.fileName = fileName;
            this.dataArr = dataArr;
            this.row = row;
            this.column = column;
        }

        public void Generate()
        {
            CreateData(filePath, fileName, dataArr, row, column);
        }

        private static void CreateData(string filePath, string fileName, string[,] dataArr, int row, int columns)
        {
            if (dataArr == null) return;

            if (!Directory.Exists(string.Format("{0}/Datas", filePath)))
            {
                Directory.CreateDirectory(string.Format("{0}/Datas", filePath));
            }

            StringBuilder sbr = new StringBuilder();

            //row 行
            //columns 列
            //dataArr[columns,row]
            string str = null;

            int tempcolumns = columns - 1;
            for (int i = 6; i < row; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var tmp = dataArr[j, 4];
                    if (tmp[0] == '*')
                    {
                        if (j == tempcolumns)
                        {
                            sbr.Append("\n");
                        }
                        continue;
                    }

                    sbr.Append(dataArr[j, i]);

                    if (j == tempcolumns)
                    {
                        sbr.Append("\n");
                    }
                    else
                    {
                        sbr.Append(" ");
                    }
                }
            }

            str = string.Format("{0}/Datas/{1}.txt", filePath, fileName);
            using (FileStream fs = new FileStream(str, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString().TrimEnd());
                }
            }

            if (!str.IsNullOrEmpty())
            {
                AddVersionFile(fileName, str);
            }

        }

        private static void AddVersionFile(string fileName, string filePath)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);

            versionSub.Append("Datas/");
            versionSub.Append(fileName);

            versionSub.Append(".txt ");
            versionSub.Append(GetMD5HashFromFile(filePath));
            versionSub.Append(" ");
            float size = fileInfo.Length / 1024.0f;
            versionSub.Append((int)size);
            //初始资源 为1
            versionSub.Append(" 1");
            versionSub.Append("\n");

            m_DicVersion[fileName] = versionSub.ToString();
            versionSub.Clear();
        }
        private static string GetMD5HashFromFile(string filename)
        {
            try
            {
                FileStream file = new FileStream(filename, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));

                }
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }
    }

    public class GenerateConfigCodeHelper
    {
        public string structTypeStr;
        public string keyStr;
        private string filePath;
        public string fileName;
        private string[,] dataArr;
        private int row;
        private int column;

        public List<int> defineNameList;
        public List<_dataCodeStruct> codeList;

        public GenerateConfigCodeHelper(string filePath, string fileName, string[,] dataArr, int row, int columus)
        {
            this.filePath = filePath;
            this.fileName = fileName;
            this.dataArr = dataArr;
            this.row = row;
            this.column = columus;
        }

        public struct _dataCodeStruct
        {
            public string nameStr;
            public string typeStr;
            public string annotationStr;
            public string portTypeStr;
        }

        public void Generate()
        {
            CreateCode(filePath, fileName, dataArr, row, column);
        }
        public bool HasNameStr(string nameStr)
        {
            for (int i = 0; i < codeList.Count; ++i)
            {
                if (codeList[i].nameStr == nameStr)
                    return true;
            }

            return false;
        }
        private void CreateCode(string filePath, string fileName, string[,] dataArr, int row, int columus)
        {
            if (dataArr == null) return;
            var folderPath = string.Format("{0}AutoCode/GenerateCode/ConfigDatas", filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            int keyIndex = -1;
            codeList = new List<_dataCodeStruct>();
            defineNameList = new List<int>();
            string keyTypeStr = string.Empty;
            for (int j = 0; j < columus; j++)
            {
                var typeArr = dataArr[j, 5].Split('|', StringSplitOptions.RemoveEmptyEntries);
                var typeStr = typeArr[0];

                bool isValueName = IsValueName(typeArr);

                if (isValueName)
                {
                    if (defineNameList == null)
                    {
                        defineNameList = new List<int>();
                    }
                    defineNameList.Add(j);
                }

                var str = dataArr[j, 4];
                if (str[0] == '*')
                {
                    Debug.Log($"跳过生成属性{str}");
                    continue;
                }

                if (keyIndex < 0)
                {
                    var iskey = TypeIsKey(typeArr);
                    if (iskey)
                    {
                        keyIndex = codeList.Count;
                        keyTypeStr = typeStr;
                    }
                }
                var dataCode = new _dataCodeStruct();
                dataCode.annotationStr = dataArr[j, 2];
                dataCode.typeStr = typeStr;
                dataCode.nameStr = dataArr[j, 4];
                dataCode.portTypeStr = dataArr[j, 3];

                codeList.Add(dataCode);
            }
            bool generateByNameFunc = defineNameList.Count > 0;
            StringBuilder sbr = new StringBuilder();
            sbr.Append("\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.AppendFormat("//更新时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//=============================================\r\n");
            var className = $"{fileName}";
            var t_structDataName = $"_{fileName}Struct";
            var t_key = string.IsNullOrEmpty(keyTypeStr) ? "int" : keyTypeStr;
            keyStr = t_key;
            structTypeStr = t_structDataName;
            var hasKey = keyIndex >= 0;
            sbr.Append("\r\n");
            sbr.AppendFormat("public class {0} : ExcelBaseData<{1}, {2}>\r\n", className, t_key, $"{className}.{t_structDataName}");

            sbr.Append("{\r\n");

            GenerateValueName(sbr, defineNameList, dataArr, row);

            GenerateStructData(sbr, className, t_structDataName, codeList);

            GenerateInitFunc(sbr, fileName);

            if (hasKey)
            {
                sbr.Append("    protected override bool HasKey => true; \r\n");
            }
            else
            {
                GenerateTransKeyFunc(sbr, t_key);
            }

            GenerateValue(sbr, keyIndex, t_key, codeList, generateByNameFunc);

            GenerateCreateFunc(sbr, t_structDataName, hasKey, t_key, keyIndex, codeList);

            if (generateByNameFunc)
                GenerateGetDataFunc(sbr, t_structDataName);
            sbr.Append("}\r\n");

            string fPath = $"{folderPath}/_{fileName}.cs";
            using (FileStream fs = new FileStream(fPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            //codeList.Clear();
            //defineNameList.Clear();
        }

        private static void GenerateValue(StringBuilder sbr, int keyIndex, string keyTypeStr, List<_dataCodeStruct> codeList, bool generateByNameFunc)
        {
            for (int i = 0; i < codeList.Count; i++)
            {
                var code = codeList[i];
                sbr.Append("    /// <summary>\r\n");
                sbr.AppendFormat("    /// {0}\r\n", code.annotationStr);
                sbr.Append("    /// </summary>\r\n");

                //有键值 通过键值获取配表值
                sbr.AppendFormat("    public {0} Get{1}({2} key)\r\n", code.typeStr, code.nameStr, keyTypeStr);
                sbr.Append("    {\r\n");
                sbr.Append($"       return GetDataByKey(key).{code.nameStr};\r\n");
                sbr.Append("    }\r\n");
                if (generateByNameFunc)
                {
                    GenerateCodeUtil.GenerateAnnotationCode(sbr, code.annotationStr, 1);
                    sbr.AppendFormat("    public {0} Get{1}(E_ValueName valueName)\r\n", code.typeStr, code.nameStr, keyTypeStr);
                    sbr.Append("    {\r\n");
                    sbr.Append($"       return GetDataByValueName(valueName).{code.nameStr};\r\n");
                    sbr.Append("    }\r\n");
                }

                sbr.Append("\r\n");
            }
        }

        private static void GenerateDataRefIndex(StringBuilder sbr, List<_dataCodeStruct> codeList)
        {
            sbr.Append("public class ValueToIndex\r\n");
            sbr.Append("{\r\n");
            for (int i = 0; i < codeList.Count; i++)
            {
                var code = codeList[i];
                sbr.Append($"    public const int {code.nameStr.ToUpper()} = {i};");
            }
            sbr.Append("}\r\n");
        }

        /// <summary>
        /// 生成初始化函数
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="fileName"></param>
        private static void GenerateInitFunc(StringBuilder sbr, string fileName)
        {
            sbr.AppendFormat("    public {0}()\r\n", fileName);
            sbr.Append("    {\r\n");
            sbr.AppendFormat("         InitData(\"{0}.txt\");\r\n", fileName);
            sbr.Append("    }\r\n");
            sbr.Append("\r\n");
        }
        /// <summary>
        /// 生成Struct
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="className"></param>
        /// <param name="structName"></param>
        /// <param name="codeList"></param>
        private static void GenerateStructData(StringBuilder sbr, string className, string structName, List<_dataCodeStruct> codeList)
        {
            sbr.AppendFormat("    public struct {0}\r\n", structName);
            sbr.Append("    {\r\n");
            for (int i = 0; i < codeList.Count; ++i)
            {
                var code = codeList[i];
                sbr.AppendFormat("        public {0} {1};\r\n", code.typeStr, code.nameStr);
            }
            sbr.Append("    }\r\n");
            sbr.Append("\r\n");
        }
        /// <summary>
        /// 生成ValueName枚举
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="defineNameList"></param>
        /// <param name="dataArr"></param>
        /// <param name="rowLength"></param>
        private static void GenerateValueName(StringBuilder sbr, List<int> defineNameList, string[,] dataArr, int rowLength)
        {
            if (defineNameList.Count == 0) return;

            sbr.Append($"    public enum E_ValueName\r\n");
            sbr.Append("    {\r\n");
            for (int i = 0; i < defineNameList.Count; i++)
            {
                var colums = defineNameList[i];
                for (int row = 6; row < rowLength; ++row)
                {
                    var dataName = dataArr[colums, row];
                    if (string.IsNullOrEmpty(dataName)) continue;
                    sbr.Append($"        {dataName} = {row - 6},\r\n");

                }
            }
            sbr.Append("    }\r\n");
            sbr.Append("\r\n");
        }

        private static bool IsValueName(string[] strArr)
        {
            if (strArr.Length > 1)
            {
                for (int i = 0; i < strArr.Length; ++i)
                {
                    var tmp = strArr[i].ToLower();
                    if (tmp.Equals("valuename"))
                        return true;
                }
            }
            return false;
        }

        private static bool TypeIsKey(string[] typeStr)
        {
            if (typeStr.Length > 1)
            {
                for (int i = 1; i < typeStr.Length; i++)
                {
                    var tmp = typeStr[i].ToLower();
                    if (tmp.Equals("key"))
                        return true;
                }
            }
            return false;
        }

        private static void GenerateCreateFunc(StringBuilder sbr, string structName, bool hasKey, string keyTypeName, int keyIndex, List<_dataCodeStruct> codeList)
        {
            sbr.AppendFormat($"    protected override bool CreateData(string[] strArr,out {keyTypeName} resultKey, out {structName} resultData)\r\n");
            sbr.Append("{\r\n");
            sbr.Append($"        resultData = new {structName}();\r\n");
            if (!hasKey)
            {
                sbr.Append($"        resultKey = default;\r\n");
            }
            for (int i = 0; i < codeList.Count; ++i)
            {
                var code = codeList[i];
                sbr.Append($"        resultData.{code.nameStr} = " + GetArgCode(code.typeStr, i) + "\r\n");
                if (hasKey)
                {
                    if (keyIndex == i)
                        sbr.Append($"        resultKey = resultData.{code.nameStr};\r\n");
                }
                else
                {

                }
            }
            sbr.Append($"        return true;\r\n");
            sbr.Append("}\r\n");
        }
        private static void GenerateGetDataFunc(StringBuilder sbr, string structName)
        {
            sbr.Append($"    public {structName} GetDataByValueName(E_ValueName valueName)\r\n");
            sbr.Append("    {\r\n");
            sbr.Append("        return GetDataByKey(GetKeyByIndex((int)valueName));\r\n");
            sbr.Append("    }\r\n");
            sbr.Append("\r\n");
        }
        private static void GenerateTransKeyFunc(StringBuilder sbr, string keyTypeName)
        {
            sbr.Append($"    protected override {keyTypeName} TransKey(int index)\r\n");
            sbr.Append("    {\r\n");
            sbr.Append("        return index;\r\n");
            sbr.Append("    }\r\n");
        }
        private static string GetArgCode(string argType, int index)
        {
            string result = "";
            switch (argType)
            {
                case "double":
                    result = $"GetDouble(strArr, {index});";
                    break;
                case "int":
                    result = $"GetInt(strArr, {index});";
                    break;
                case "float":
                    result = $"GetFloat(strArr, {index});";
                    break;
                case "string":
                    result = $"GetString(strArr, {index});";
                    break;
                case "long":
                    result = $"GetLong(strArr, {index});";
                    break;
                case "bool":
                    result = $"GetBool(strArr, {index});";
                    break;
                case "int[]":
                    result = $"GetIntArray(strArr, {index});";
                    break;
                case "float[]":
                    result = $"GetFloatArray(strArr, {index});";
                    break;
                case "long[]":
                    result = $"GetLongArray(strArr, {index});";
                    break;
                case "string[]":
                    result = $"GetStringArray(strArr, {index});";
                    break;
                case "Vector3":
                    result = $"GetVector3(strArr, {index});";
                    break;
            }

            if (string.IsNullOrEmpty(result))
            {
                result = $"new {argType}(strArr, {index});";
                //Debug.Log($"发现不支持的配置类型 {argType} ");
            }
            return result;
        }
    }

    public class GenerateConfigurationsCode
    {
        private StringBuilder configSbr;
        private string filePath;
        public GenerateConfigurationsCode(StringBuilder configSbr, string filePath)
        {
            this.configSbr = configSbr;
            this.filePath = filePath;
        }

        public void Generate()
        {
            CreateConfigsode(filePath, configSbr);
        }

        private static void CreateConfigsode(string filePath, StringBuilder configSbr)
        {
            StringBuilder sbr = new StringBuilder();
            var folderPath = filePath;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var configArr = configSbr.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            sbr.Append("\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.AppendFormat("//更新时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//=============================================\r\n");

            sbr.Append("\r\n");
            sbr.Append("public class Configurations \r\n");
            sbr.Append("{\r\n");

            GenerateConfig(sbr, configArr);

            GenerateClearFunc(sbr, configArr);

            sbr.Append("}\r\n");
            sbr.Append("\r\n");


            string fPath = $"{folderPath}/_Configurations.cs";
            using (FileStream fs = new FileStream(fPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }
        }

        private static void GenerateClearFunc(StringBuilder sbr, string[] configArr)
        {
            sbr.Append("    public void Clear()\r\n");
            sbr.Append("    {\r\n");
            for (int i = 0; i < configArr.Length; i++)
            {
                var configName = configArr[i];
                sbr.Append($"        if (_{configName} != null)\r\n");
                sbr.Append("        {\r\n");
                sbr.Append($"            _{configName}.Clear();\r\n");
                sbr.Append($"            _{configName} = null;\r\n");
                sbr.Append("        }\r\n");
            }
            sbr.Append("    }\r\n");
            sbr.Append("\r\n");
        }
        private static void GenerateConfig(StringBuilder sbr, string[] configArr)
        {
            for (int i = 0; i < configArr.Length; ++i)
            {
                var configName = configArr[i];
                var configDataName = $"{configName}";
                sbr.Append($"    private static {configDataName} _{configDataName};\r\n");
                sbr.Append($"    public static {configDataName} {configName}\r\n");
                sbr.Append("    {\r\n");
                sbr.Append("        get\r\n");
                sbr.Append("        {\r\n");
                sbr.Append($"            if(_{configDataName} == null)\r\n");
                sbr.Append("            {\r\n");
                sbr.Append($"                _{configDataName} = new {configDataName}();\r\n");
                sbr.Append("            }\r\n");
                sbr.Append($"            return _{configDataName};\r\n");
                sbr.Append("        }\r\n");
                sbr.Append("    }\r\n");
            }
        }

    }

    public class GenerateDefineDataCodeHelper
    {
        public struct _dataCodeStruct
        {
            public string nameStr;
            public string typeStr;
            public string annotationStr;
            public string portTypeStr;
            public string defaultValueStr;
            public string modifierStr;
            public bool isAbstract;
            public bool isCreateParam;
            public bool isConfig;
            public bool isNoMember;
            public List<string[]> linkConfigs;
            public List<string> mutualList;
        }

        private string filePath;
        private string fileName;
        private string[,] dataArr;
        private int row;
        private int column;

        public string managerName;
        private string inheritName;
        public string className;

        public bool isAbstract;

        public Dictionary<string, _dataCodeStruct> codes;

        public List<string> inheritRootLs = new List<string>();

        public Dictionary<string, _dataCodeStruct> createParamDict = new Dictionary<string, _dataCodeStruct>();



        public bool hasNoMember;
        public GenerateDefineDataCodeHelper(string filePath, string fileName, string[,] strArr, int row, int column)
        {
            this.filePath = filePath;
            this.fileName = fileName;
            this.dataArr = strArr;
            this.row = row;
            this.column = column;
        }

        public void Analysis()
        {
            if (codes == null)
                codes = new Dictionary<string, _dataCodeStruct>();
            else
                codes.Clear();

            inheritName = dataArr[1, 2];
            className = fileName;
            managerName = dataArr[3, 2];
            AnalysisData(filePath, fileName, dataArr, row, column, codes);
        }

        public void Generate()
        {
            CreateData(filePath, codes, className, inheritName, isAbstract);

        }
        private void AnalysisData(string filePath, string fileName, string[,] dataArr, int row, int columus, Dictionary<string, _dataCodeStruct> codeDict)
        {
            isAbstract = false;
            if (dataArr == null) return;
            string inheritName = dataArr[1, 2];
            var className = fileName;
            for (int r = 4; r < row; r++)
            {
                var dataCode = new _dataCodeStruct();
                dataCode.nameStr = dataArr[0, r];
                if (string.IsNullOrEmpty(dataCode.nameStr))
                {
                    continue;
                }
                dataCode.annotationStr = dataArr[1, r];
                var typeStr = dataArr[2, r];
                dataCode.typeStr = TransValueType(typeStr, out string _);
                dataCode.modifierStr = dataArr[3, r];
                dataCode.defaultValueStr = dataArr[4, r];
                dataCode.portTypeStr = dataArr[5, r];
                dataCode.isCreateParam = IsCreateParam(dataArr[6, r]);
                dataCode.isConfig = IsConfig(dataArr[6, r]);
                dataCode.isNoMember = IsNomember(dataArr[6, r]);
                dataCode.mutualList = AnalysisMutualList(dataArr[6, r]);
                if (dataCode.isNoMember)
                    hasNoMember = true;
                dataCode.linkConfigs = AnalysisLinkConfig(dataArr[7, r]);
                if (dataCode.modifierStr.Equals("abstract"))
                {
                    isAbstract = true;
                }
                dataCode.isAbstract = isAbstract;
                if (dataCode.isCreateParam)
                {
                    createParamDict.Add(dataCode.nameStr, dataCode);
                }
                codeDict.Add(dataCode.nameStr, dataCode);

            }
        }
        private List<string[]> AnalysisLinkConfig(string linkConfig)
        {
            if (string.IsNullOrEmpty(linkConfig)) return null;
            var linkConfigs = new List<string[]>();
            var strArr = linkConfig.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (strArr.Length > 0)
            {
                foreach (var str in strArr)
                {
                    var tempArr = str.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    linkConfigs.Add(tempArr);
                }
            }
            return linkConfigs;
        }

        private void CreateData(string filePath, Dictionary<string, _dataCodeStruct> codes, string className, string inheritName, bool isAbstract)
        {

            var folderPath = string.Format("{0}AutoCode/GenerateCode/DefineDatas", filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);


            StringBuilder sbr = new StringBuilder();
            sbr.Append("\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.AppendFormat("//更新时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.Append("\r\n");
            if (string.IsNullOrEmpty(inheritName))
            {
                if (isAbstract)
                    sbr.Append($"public abstract partial class {className} : ExcelBaseObject\r\n");
                else
                {
                    GenerateCodeUtil.GenerateAnnotationCode(sbr, $"使用DefineObjectFactory.Create{className}创建对象", 0);
                    sbr.Append($"public partial class {className} : ExcelBaseObject\r\n");
                }
            }
            else
            {
                if (isAbstract)
                    sbr.Append($"public abstract partial class {className} : {inheritName}\r\n");
                else
                    sbr.Append($"public partial class {className} : {inheritName}\r\n");
            }
            sbr.Append("{\r\n");
            bool hasSetByConfig = false;
            AnalysisInheritData(codes, inheritName, out var ineritData);
            GenerateCreateParamCode(sbr, createParamDict, ineritData);
            GenerateMemberValueCode(sbr, codes, inheritName);
            if (!isAbstract)
            {
                GenerateSetByFunc(sbr, ineritData, out hasSetByConfig);
            }
            GenerateCreateFuncCode(sbr, codes, className, inheritName, hasSetByConfig, ineritData);
            GenerateStatusCode(sbr, inheritName);
            sbr.Append("}\r\n");
            string fPath = $"{folderPath}/_{className}.cs";
            using (FileStream fs = new FileStream(fPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            //codes.Clear();
        }

        private struct _setByStru
        {
            public string[] linkParam;
            public string valueName;
        }
        private void GenerateSetByFunc(StringBuilder sbr, Dictionary<string, _dataCodeStruct> ineritData, out bool hassetByConfig)
        {
            //if (!string.IsNullOrEmpty(inheritName)) return;
            hassetByConfig = false;
            var tempDict = new Dictionary<string, List<_setByStru>>();

            if (ineritData != null && ineritData.Count > 0)
            {
                foreach (var item in ineritData)
                {
                    if (item.Value.linkConfigs != null && item.Value.linkConfigs.Count > 0)
                    {
                        foreach (var item2 in item.Value.linkConfigs)
                        {
                            var fkey = item2[0];
                            if (!tempDict.TryGetValue(fkey, out var resultList))
                            {
                                resultList = new List<_setByStru>();
                                tempDict.Add(fkey, resultList);
                            }
                            resultList.Add(new _setByStru { valueName = item.Key, linkParam = item2 });
                        }
                    }
                }
            }

            foreach (var item in codes)
            {
                if (item.Value.linkConfigs != null && item.Value.linkConfigs.Count > 0)
                {
                    foreach (var item2 in item.Value.linkConfigs)
                    {
                        var fkey = item2[0];
                        if (!tempDict.TryGetValue(fkey, out var resultList))
                        {
                            resultList = new List<_setByStru>();
                            tempDict.Add(fkey, resultList);
                        }
                        resultList.Add(new _setByStru { valueName = item.Key, linkParam = item2 });
                    }
                }
            }

            foreach (var item in tempDict)
            {
                hassetByConfig = true;
                GenerateCodeUtil.GenerateAnnotationCode(sbr, $"使用{item.Key}执行配置链接", 1);
                sbr.Append($"    private void SetBy{item.Key}()\r\n");
                sbr.Append("    {\r\n");
                sbr.Append($"        var data = {item.Key};\r\n");
                foreach (var setItem in item.Value)
                {
                    if (setItem.linkParam.Length <= 1) continue;

                    string tmp = "data";
                    //string lastDataType = null;
                    for (int i = 1; i < setItem.linkParam.Length; i++)
                    {
                        if (i == setItem.linkParam.Length - 1)
                        {
                            if (codes.TryGetValue(item.Key, out var codeStruct)
                                && ExportExcelData.configCodeHelperDict.TryGetValue(codeStruct.typeStr, out var resultConfig)
                                && resultConfig.HasNameStr(setItem.linkParam[i]))
                            {

                                tmp += $".{setItem.linkParam[i]}";
                            }
                            else
                            {
                                tmp += $".{setItem.linkParam[i]}()";
                            }
                        }
                        else
                            tmp += $".{setItem.linkParam[i]}";
                    }

                    if (codes.TryGetValue(setItem.valueName, out var code))
                    {
                        if (code.isConfig && ExportExcelData.configCodeHelperDict.TryGetValue(code.typeStr, out var resultConfig))
                        {

                            sbr.Append($"        {setItem.valueName}ID = Configurations.{code.typeStr}.GetID(({code.typeStr}.E_ValueName)System.Enum.Parse(typeof({code.typeStr}.E_ValueName), {tmp}));\r\n");
                            //链接表格配置ID

                        }
                        else
                        {
                            //普通链接
                            sbr.Append($"        {setItem.valueName}.SetByConfig({tmp});\r\n");
                        }

                    }
                    else if (ineritData != null && ineritData.TryGetValue(setItem.valueName, out code))
                    {
                        if (code.isConfig)
                        {
                            //链接表格配置ID
                            sbr.Append($"        {setItem.valueName}ID =  {tmp});\r\n");

                        }
                        else
                        {
                            //普通链接
                            sbr.Append($"        {setItem.valueName}.SetByConfig({tmp});\r\n");
                        }
                    }
                }
                sbr.Append("    }\r\n");
            }

            if (hassetByConfig)
            {
                GenerateCodeUtil.GenerateAnnotationCode(sbr, "通过链接配置初始化对象");
                sbr.Append($"    private void InitByConfig()\r\n");
                sbr.Append("    {\r\n");
                foreach (var item in tempDict)
                {
                    if (codes.TryGetValue(item.Key, out var code))
                    {
                        if (code.isConfig)
                        {
                            sbr.Append($"        if ({item.Key}ID != 0)\r\n");
                            sbr.Append("        {\r\n");
                            sbr.Append($"            SetBy{item.Key}();\r\n");
                            sbr.Append("        }\r\n");

                        }
                    }
                }
                sbr.Append("    }\r\n");
            }
        }


        private const string C_GETSETSTR = @"{ get; protected set; }";

        private void AnalysisInheritData(Dictionary<string, _dataCodeStruct> dataDict, string inheritName, out Dictionary<string, _dataCodeStruct> ineritData)
        {
            ineritData = null;
            HashSet<string> hashSet = null;//防止死循环

            void CheckIneritData(string inheritName, Dictionary<string, _dataCodeStruct> ineritData)
            {
                if (!hashSet.Contains(inheritName) &&
                    ExportExcelData.generateDefineDataCodeHelperDict.TryGetValue(inheritName, out var codeHelper))
                {
                    hashSet.Add(inheritName);

                    foreach (var kvp in codeHelper.codes)
                    {
                        var code = kvp.Value;
                        if (code.isCreateParam && !createParamDict.ContainsKey(code.nameStr))
                        {
                            createParamDict.Add(code.nameStr, code);
                        }

                        if (!ineritData.ContainsKey(kvp.Key) && !dataDict.ContainsKey(kvp.Key))
                        {
                            ineritData.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (!string.IsNullOrEmpty(codeHelper.inheritName))
                    {
                        CheckIneritData(codeHelper.inheritName, ineritData);
                    }
                }
            }

            if (!string.IsNullOrEmpty(inheritName))
            {
                if (hashSet == null)
                    hashSet = new HashSet<string>();

                if (ineritData == null)
                    ineritData = new Dictionary<string, _dataCodeStruct>();

                CheckIneritData(inheritName, ineritData);
            }
        }

        private static void GenerateMemberValueCode(StringBuilder sbr, Dictionary<string, _dataCodeStruct> dataDict, string inheritName)
        {
            foreach (var kvp in dataDict)
            {
                var dataCode = kvp.Value;
                if (dataCode.isNoMember)
                    continue;
                if (!string.IsNullOrEmpty(inheritName) &&
                    ExportExcelData.generateDefineDataCodeHelperDict.TryGetValue(inheritName, out var classItem) &&
                    classItem.codes.TryGetValue(dataCode.nameStr, out var _value))
                {
                    GenerateCodeUtil.GenerateAnnotationCode(sbr, dataCode.annotationStr, 1);
                    sbr.Append($"    public override {dataCode.typeStr} {dataCode.nameStr} {C_GETSETSTR}\r\n");
                }
                else
                {
                    if (dataCode.isConfig)
                    {
                        if (ExportExcelData.configCodeHelperDict.TryGetValue(dataCode.typeStr, out var config))
                        {
                            GenerateCodeUtil.GenerateAnnotationCode(sbr, dataCode.annotationStr, 1);
                            var memberName = $"{dataCode.nameStr}ID";
                            sbr.Append($"    public {config.keyStr} {memberName} {{ get; protected set; }}\r\n");
                            GenerateCodeUtil.GenerateAnnotationCode(sbr, dataCode.annotationStr, 1);
                            sbr.Append($"    public {dataCode.typeStr}.{config.structTypeStr} {dataCode.nameStr}\r\n");
                            sbr.Append("    {\r\n");
                            sbr.Append("        get\r\n");
                            sbr.Append("        {\r\n");
                            sbr.Append($"            return Configurations.{dataCode.typeStr}.GetDataByKey({memberName});\r\n");
                            sbr.Append("        }\r\n");
                            sbr.Append("    }\r\n");

                        }
                        else
                        {
                            Debug.Log($"没找到 表格{dataCode.typeStr}");
                        }
                    }
                    else
                    {
                        GenerateCodeUtil.GenerateAnnotationCode(sbr, dataCode.annotationStr, 1);
                        if (string.IsNullOrEmpty(dataCode.modifierStr))
                            sbr.Append($"    public {dataCode.typeStr} {dataCode.nameStr} {C_GETSETSTR}\r\n");
                        else
                            sbr.Append($"    public {dataCode.modifierStr} {dataCode.typeStr} {dataCode.nameStr} {C_GETSETSTR}\r\n");
                    }

                }
                sbr.Append("\r\n");
            }

        }
        /// <summary>
        /// 生成 CreateParam
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="dataDict"></param>
        /// <param name="ineritData"></param>
        private static void GenerateCreateParamCode(StringBuilder sbr, Dictionary<string, _dataCodeStruct> dataDict, Dictionary<string, _dataCodeStruct> ineritData)
        {
            if (ineritData != null)
            {
                sbr.Append($"    public new struct CreateParam\r\n");
            }
            else
            {
                sbr.Append($"    public struct CreateParam\r\n");
            }
            sbr.Append("    {\r\n");

            foreach (var kvp in dataDict)
            {
                var code = kvp.Value;
                if (code.isConfig)
                {
                    if (ExportExcelData.configCodeHelperDict.TryGetValue(code.typeStr, out var config))
                    {
                        var memberName = $"{code.nameStr}ID";
                        sbr.Append($"        public {config.keyStr} {memberName};\r\n");

                    }
                }
                else
                {
                    TransValueType(code.typeStr, out var typeStr);
                    sbr.Append($"        public {typeStr} {code.nameStr};\r\n");
                }

            }


            sbr.Append("    }\r\n");

        }
        /// <summary>
        /// 生成DefineData构造函数
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="dataDict"></param>
        /// <param name="className"></param>
        /// <param name="inheritName"></param>
        /// <param name="inheritCode"></param>
        private void GenerateCreateFuncCode(StringBuilder sbr, Dictionary<string, _dataCodeStruct> dataDict, string className, string inheritName, bool hasSetByConfig, Dictionary<string, _dataCodeStruct> inheritCode)
        {
            #region 带参数的构造函数
            if (string.IsNullOrEmpty(managerName))
            {
                sbr.Append($"    public {className}(CreateParam param)\r\n");
            }
            else
            {
                sbr.Append($"    public {className}(GameGID gid, CreateParam param)\r\n");
            }

            if (!string.IsNullOrEmpty(inheritName))
            {
                sbr.Append($"        : base(new {inheritName}.CreateParam\r\n");
                sbr.Append("        {\r\n");
                foreach (var kvp in inheritCode)
                {
                    var code = kvp.Value;
                    if (code.isCreateParam)
                        sbr.Append($"            {code.nameStr} = param.{code.nameStr},\r\n");
                }
                sbr.Append("        })\r\n");
            }

            sbr.Append("    {\r\n");
            foreach (var kvp in dataDict)
            {
                var code = kvp.Value;
                if (code.isAbstract)
                {
                    continue;
                }
                if (code.isNoMember) continue;
                if (code.isConfig)
                {
                    if (ExportExcelData.configCodeHelperDict.TryGetValue(code.typeStr, out var config))
                    {
                        if (code.isCreateParam)
                        {
                            sbr.Append($"        {code.nameStr}ID = param.{code.nameStr}ID;\r\n");
                        }
                        else
                        {
                            TransValueType(config.keyStr, out var setStr);
                            sbr.Append($"        {code.nameStr}ID = {setStr}.Parse({code.defaultValueStr});\r\n");
                        }
                    }
                }
                else
                {

                    var typeStr = $"{TransValueType(code.typeStr, out var setTypeStr)}";
                    if (createParamDict.ContainsKey(code.nameStr))
                    {

                        sbr.Append($"        {code.nameStr} = new {typeStr}(param.{code.nameStr});\r\n");
                    }
                    //else if (codes.TryGetValue(code.nameStr, out var dataCode))
                    //{

                    //}
                    else
                    {
                        if (string.IsNullOrEmpty(code.defaultValueStr))
                        {
                            sbr.Append($"        {code.nameStr} = new {typeStr}();\r\n");
                        }
                        else
                        {
                            sbr.Append($"        {code.nameStr} = new {typeStr}(\"{code.defaultValueStr}\");\r\n");
                        }
                    }
                }
            }

            if (hasSetByConfig)
            {
                sbr.Append("        InitByConfig();\r\n");
            }
            sbr.Append("    }\r\n");

            #endregion

            #region 没参数的构造函数
            sbr.Append($"    public {className}()\r\n");
            if (!string.IsNullOrEmpty(inheritName))
            {
                sbr.Append($"        : base(new {inheritName}.CreateParam())\r\n");
            }
            sbr.Append("    {\r\n");

            foreach (var kvp in dataDict)
            {
                var code = kvp.Value;
                if (code.isAbstract)
                {
                    continue;
                }
                if (code.isNoMember) continue;
                if (code.isConfig)
                {
                    if (ExportExcelData.configCodeHelperDict.TryGetValue(code.typeStr, out var config))
                    {
                        //"        {setItem.valueName}ID = (int)({code.typeStr}.E_ValueName)System.Enum.Parse(typeof({code.typeStr}.E_ValueName), {tmp});\r\n"
                        if (!string.IsNullOrEmpty(code.defaultValueStr))
                        {
                            sbr.Append($"        {code.nameStr}ID = (int)({code.typeStr}.E_ValueName)System.Enum.Parse(typeof({code.typeStr}.E_ValueName), {code.defaultValueStr});\r\n");
                        }
                    }
                }
                else
                {

                    var typeStr = $"{TransValueType(code.typeStr, out var setTypeStr)}";
                    if (createParamDict.ContainsKey(code.nameStr))
                    {

                        sbr.Append($"        {code.nameStr} = new {typeStr}(\"{code.defaultValueStr}\");\r\n");
                    }
                    //else if (codes.TryGetValue(code.nameStr, out var dataCode))
                    //{

                    //}
                    else
                    {
                        if (string.IsNullOrEmpty(code.defaultValueStr))
                        {
                            sbr.Append($"        {code.nameStr} = new {typeStr}();\r\n");
                        }
                        else
                        {
                            sbr.Append($"        {code.nameStr} = new {typeStr}(\"{code.defaultValueStr}\");\r\n");
                        }
                    }
                }
            }

            if (hasSetByConfig)
            {
                sbr.Append("        InitByConfig();\r\n");
            }
            sbr.Append("    }\r\n");
            #endregion
        }
        private static void GenerateStatusCode(StringBuilder sbr, string inheritName)
        {
            if (!string.IsNullOrEmpty(inheritName))
                return;
            sbr.Append("    public bool IsDirty { get; private set; }\r\n");
            sbr.Append("    public void SetDirty()\r\n");
            sbr.Append("    {\r\n");
            sbr.Append("        IsDirty = true;\r\n");
            sbr.Append("    }\r\n");


        }
        private static string TransValueType(string typeStr, out string setStr)
        {
            var t = typeStr.ToLower().Trim();
            setStr = string.Empty;
            string result = string.Empty;
            switch (t)
            {
                case "ushort":
                case "ushortvalue":
                    setStr = "ushort";
                    result = "UshortValue";
                    break;
                case "long":
                case "longvalue":
                    setStr = "long";
                    result = "LongValue";
                    break;
                case "intvalue":
                case "int":
                    setStr = "int";
                    result = "IntValue";
                    break;
                case "gameint":
                    result = "GameInt";
                    break;
                case "floatvalue":
                case "float":
                    setStr = "float";
                    result = "FloatValue";
                    break;
                case "gamefloat":
                    result = "GameFloat";
                    break;
                case "string":
                    setStr = "string";
                    result = "StringValue";
                    break;
                case "actorname":
                case "name":
                    result = "ActorName";
                    break;
            }
            if (!string.IsNullOrEmpty(result))
            {
                if (string.IsNullOrEmpty(setStr))
                    setStr = $"{typeStr}Set";
            }
            else
            {
                setStr = typeStr;
                result = typeStr;
            }

            return result;
        }

        private static List<string> AnalysisMutualList(string mutualStr)
        {
            if (string.IsNullOrEmpty(mutualStr)) return null;
            var arr = mutualStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<string> result = new List<string>(arr);
            return result;
        }
        private static bool IsCreateParam(string tagStr)
        {
            if (string.IsNullOrEmpty(tagStr)) return false;
            var arr = tagStr.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in arr)
            {
                var str = temp.ToLower();
                if (str.Equals("createparam"))
                    return true;
            }
            return false;
        }

        private static bool IsConfig(string tagStr)
        {
            if (string.IsNullOrEmpty(tagStr)) return false;
            var arr = tagStr.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in arr)
            {
                var str = temp.ToLower();
                if (str.Equals("config"))
                    return true;
            }
            return false;
        }

        private static bool IsNomember(string tagStr)
        {
            if (string.IsNullOrEmpty(tagStr)) return false;
            var arr = tagStr.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in arr)
            {
                var str = temp.ToLower();
                if (str.Equals("nomember"))
                    return true;
            }
            return false;
        }
    }

    public class GenerateDefineDataFactoryCodeHelper
    {
        private string filePath;
        private string fileName;

        public GenerateDefineDataFactoryCodeHelper(string filePath, string fileName)
        {
            this.filePath = filePath;
            this.fileName = fileName;
        }

        public void Generate()
        {
            CreateData();
        }

        private void CreateData()
        {
            var folderPath = string.Format("{0}", filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            StringBuilder sbr = new StringBuilder();
            sbr.Append("\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.AppendFormat("//更新时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.Append("\r\n");

            sbr.Append("public class DefineObjectFactory\r\n");
            sbr.Append("{\r\n");

            foreach (var kvp in ExportExcelData.generateDefineDataCodeHelperDict)
            {
                var code = kvp.Value;
                if (code.isAbstract)
                {
                    continue;
                }

                GenerateCreateObjCode(sbr, code);
            }
            sbr.Append("}\r\n");
            sbr.Append("\r\n");
            string fPath = $"{folderPath}/_{fileName}.cs";
            using (FileStream fs = new FileStream(fPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }
        }

        private void GenerateCreateObjCode(StringBuilder sbr, GenerateDefineDataCodeHelper codeItem)
        {
            sbr.Append($"    public static {codeItem.className} Create{codeItem.className}({codeItem.className}.CreateParam pam)\r\n");
            sbr.Append("    {\r\n");
            sbr.Append($"        var result = new {codeItem.className}(pam);\r\n");
            sbr.Append($"        result.OnCreated();\r\n");
            sbr.Append($"        return result;\r\n");
            sbr.Append("    }\r\n");

        }
    }

    public class GenerateManagerFileCodeHelper
    {
        private string filePath;
        private string fileName;

        private List<GenerateDefineDataCodeHelper> list;

        public GenerateManagerFileCodeHelper(string filePath, string fileName, List<GenerateDefineDataCodeHelper> list)
        {
            this.filePath = filePath;
            this.fileName = fileName;
            this.list = list;
        }

        private string className;


        public void Analysis()
        {
            className = fileName;

            for (int i = 0; i < list.Count; i++)
            {
                var code = list[i];
            }
        }

        public void Generate()
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            StringBuilder sbr = new StringBuilder();
            sbr.Append("//=============================================\r\n");
            sbr.AppendFormat("//更新时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.Append("\r\n");

            sbr.Append("using System.Collections.Generic;\r\n");
            sbr.Append("\r\n");
            sbr.Append($"public class {className} : ManagerBase\r\n");
            sbr.Append("{\r\n");
            for (int i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                var dictName = $"_{temp.className}Dict";
                sbr.Append($"    private Dictionary<GameGID, {temp.className}> {dictName} = new Dictionary<GameGID, {temp.className}>();\r\n");

                GenerateTryGetCodeFile(sbr, temp.className, dictName);

                GenerateCreateCodeFile(sbr, temp.className, dictName, null);
            }

            sbr.Append("}\r\n");
            sbr.Append("\r\n");
            string fPath = $"{filePath}/_{fileName}.cs";
            using (FileStream fs = new FileStream(fPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }
        }

        private void GenerateTryGetCodeFile(StringBuilder sbr, string className, string dictName)
        {
            GenerateCodeUtil.GenerateAnnotationCode(sbr, $"尝试获取{className}的对象,这个方法不具备构造对象能力");
            sbr.Append($"    public bool TryGet{className}(GameGID gid, out {className} result)\r\n");
            sbr.Append("    {\r\n");
            sbr.Append($"        if ({dictName}.TryGetValue(gid, out result))\r\n");
            sbr.Append("        {\r\n");
            sbr.Append("            return true;\r\n");
            sbr.Append("        }\r\n");
            sbr.Append("        return false;\r\n");
            sbr.Append("    }\r\n");
        }

        private void GenerateCreateCodeFile(StringBuilder sbr, string className, string dictName, string createPram)
        {
            GenerateCodeUtil.GenerateAnnotationCode(sbr, $"创建一个对象");
            sbr.Append($"    public {className} Create{className}({className}.CreateParam pam)\r\n");
            sbr.Append("    {\r\n");
            sbr.Append($"        {className} result = new {className}(pam);\r\n");
            sbr.Append($"        {dictName}.Add(result.Gid, result);\r\n");
            sbr.Append($"        result.OnCreated();\r\n");
            sbr.Append($"        return result;\r\n");
            sbr.Append("    }\r\n");
        }
    }


    public class GenerateCodeUtil
    {
        public static void GenerateAnnotationCode(StringBuilder sbr, string annotation, int spaceCount = 1)
        {
            string forward = "";
            for (int i = 0; i < spaceCount; ++i)
            {
                forward += "    ";
            }
            sbr.Append($"{forward}/// <summary>\r\n");
            sbr.Append($"{forward}/// {annotation}\r\n");
            sbr.Append($"{forward}/// </summary>\r\n");
        }
    }

}

#endif
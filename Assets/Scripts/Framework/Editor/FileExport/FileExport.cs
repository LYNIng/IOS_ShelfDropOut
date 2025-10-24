using ReadExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;


namespace FileExport
{
    public class FileExportData
    {
        public FileInfo fileInfo;
        public object sourceReadData;
        public object sourceResolveData;
    }

    /// <summary>
    /// 文件导出装配器
    /// 这里提供操作
    /// </summary>
    public abstract class FileExportBase
    {
        public abstract string LinkName { get; }

        protected List<string> exportDir = new List<string>();
        public Dictionary<string, FileExportData> fileExportDatas { get; private set; } = new Dictionary<string, FileExportData>();

        public abstract HashSet<string> fileTypeFilterater { get; }

        /// <summary>
        /// 源文件解析器
        /// </summary>
        public SourceFileResolver sourceFileResolver { get; private set; }


        /// <summary>
        /// 数据文件创建器
        /// </summary>
        public List<SourceFileDataMaker> sourceFileDataMakers { get; private set; } = new List<SourceFileDataMaker>();

        /// <summary>
        /// 源文件读取器
        /// </summary>
        public SourceFileReader sourceFileReader { get; private set; }
        public FileExportBase(SourceFileReader reader, SourceFileResolver resolver, SourceFileDataMaker[] dataMaker)
        {
            Init(reader, resolver, dataMaker);
        }
        public FileExportBase(SourceFileReader reader, SourceFileResolver resolver, SourceFileDataMaker dataMaker = null)
        {
            Init(reader, resolver, dataMaker);
        }

        private void Init(SourceFileReader reader, SourceFileResolver resolver, SourceFileDataMaker dataMaker)
        {
            Init(reader, resolver, new SourceFileDataMaker[] { dataMaker });
        }
        private void Init(SourceFileReader reader, SourceFileResolver resolver, SourceFileDataMaker[] dataMaker)
        {
            sourceFileReader = reader;
            sourceFileResolver = resolver;
            if (dataMaker != null)
                sourceFileDataMakers.AddRange(dataMaker);
        }

        public void AddFileDataMaker(SourceFileDataMaker dataMaker)
        {
            sourceFileDataMakers.Add(dataMaker);
        }

        public void AddExportDir(string dir)
        {
            exportDir.Add(dir);
        }
        public void AddExportDir(string[] dirs)
        {
            exportDir.AddRange(dirs);
        }
        public void AddExportDir(List<string> dirs)
        {
            exportDir.AddRange(dirs);
        }
        public void RemoveExportDir(string dir)
        {
            exportDir.Remove(dir);
        }
        //读取文件
        public void ReadFile()
        {
            fileExportDatas.Clear();

            if (sourceFileReader == null) return;

            var fileInfoLS = new List<FileInfo>();
            for (int i = 0; i < exportDir.Count; ++i)
            {
                var tempPath = exportDir[i];

                ExcelCodeUtil.GetAllFile(tempPath, fileTypeFilterater, ref fileInfoLS);

                for (int j = 0; j < fileInfoLS.Count; ++j)
                {
                    var info = fileInfoLS[j];
                    var filePath = info.FullName;
                    if (!fileExportDatas.ContainsKey(filePath) && !sourceFileReader.IsIgnoreFile(filePath))
                    {
                        var exportData = new FileExportData();
                        exportData.fileInfo = info;

                        fileExportDatas.Add(filePath, exportData);
                    }
                }
            }


            foreach (var kvp in fileExportDatas)
            {
                var filePath = kvp.Key;
                var data = kvp.Value;
                if (sourceFileReader.ReadFile(filePath, out var result))
                {
                    data.sourceReadData = result;
                }
            }

        }

        /// <summary>
        /// 解析已读取的文件
        /// </summary>
        public void Resolve()
        {
            if (sourceFileResolver == null)
                return;

            if (fileExportDatas.Count == 0)
                return;

            foreach (var kvp in fileExportDatas)
            {
                var data = kvp.Value;
                if (sourceFileResolver.Resolve(data.sourceReadData, out var result))
                {
                    data.sourceResolveData = result;
                }
            }
        }

        public void ExportFile(string exportDir)
        {
            if (fileExportDatas.Count == 0)
            {
                //Debug.Log("AutoCodeTool Error -------->> 生成失败 没有导出数据");
                return;
            }

            if (sourceFileDataMakers.Count == 0)
            {

                return;
            }

            for (int i = 0; i < sourceFileDataMakers.Count; i++)
            {
                var maker = sourceFileDataMakers[i];
                if (maker == null) continue;

                maker.GenerateFile(exportDir, fileExportDatas);
            }

        }

        /// <summary>
        /// 文件读取器
        /// </summary>
        public abstract class SourceFileReader
        {
            public abstract bool ReadFile(string path, out object result);

            public virtual bool IsIgnoreFile(string filePath) => false;

        }

        /// <summary>
        /// 源文件解析器 用于解析原始文件 如Excel
        /// </summary>
        public abstract class SourceFileResolver
        {
            /// <summary>
            /// readData 是 SourceFileReader 文件读取器读取出来的Data
            /// </summary>
            /// <param name="readData"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public abstract bool Resolve(object readData, out object result);

        }

        /// <summary>
        /// 源文件创建器 在获取源文件数据后创建数据文件
        /// </summary>
        public abstract class SourceFileDataMaker
        {
            public abstract bool GenerateFile(string exportDir, Dictionary<string, FileExportData> exportData);

            public void CreateFile(string fileExportDir, string fileName, string suffix, StringBuilder sbr)
            {
                string filePath = $"{fileExportDir}/{fileName}.{suffix}";

                if (!Directory.Exists(fileExportDir))
                {
                    Directory.CreateDirectory(fileExportDir);
                }


                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(sbr.ToString().TrimEnd());
                    }
                }
            }
        }

        private static void LogMsg(string msg)
        {
            Debug.Log($"AutoCodeTool Error -------->> {msg}");
        }
    }

    public static class CodeHelperExporter
    {
        public static string ToCode(this CodeHelper.Modifier propertyModifier)
        {
            return $"{propertyModifier.ToString().ToLower()} ";
        }
        public static string ToCode(this CodeHelper.Modifier? propertyModifier)
        {

            return $"{(propertyModifier.HasValue ? propertyModifier.Value.ToCode() : "")}";
        }
    }

    public partial class CodeHelper
    {
        public enum Modifier
        {
            Public,
            Private,
            Protected,
            Internal
        }

        public static void GenerateRegionBegin(StringBuilder sbr, int alignCount, string des)
        {

        }

        public static void GenerateRegionEnd(StringBuilder sbr, int alignCount)
        {

        }

        /// <summary>
        /// 生成文件头
        /// </summary>
        /// <param name="sbr"></param>
        public static void GenerateTitle(StringBuilder sbr)
        {
            sbr.Append("//=============================================\r\n");
            sbr.AppendFormat("//更新时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//=============================================\r\n");
            sbr.Append("\r\n");

        }
        /// <summary>
        /// 生成类定义
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="className"></param>
        /// <param name="inheritClassArr"></param>
        /// <param name="isAbstract"></param>
        public static void GenerateClassCodeLine(StringBuilder sbr, string className, string[] inheritClassArr = null, bool isAbstract = false, int alignCount = 1)
        {
            AlignSpace(sbr, alignCount);
            string inheritStr = "";
            if (inheritClassArr != null && inheritClassArr.Length > 0)
            {
                inheritStr = " : ";
                for (int i = 0; i < inheritClassArr.Length; i++)
                {
                    inheritStr += inheritClassArr[i];
                    if (i != inheritClassArr.Length - 1)
                    {
                        inheritStr += ",";
                    }
                }
            }

            if (isAbstract)
            {
                sbr.Append($"public abstract class {className}{inheritStr}\r\n");
            }
            //else if (isPartial)
            //{
            //    sbr.Append($"public partial class {className}{inheritStr}\r\n");
            //}
            else
            {
                sbr.Append($"public class {className}{inheritStr}\r\n");
            }

        }
        public static void GenerateClassCode(StringBuilder sbr, string className, string[] inheritClassArr = null, bool isAbstract = false, bool isPartial = false, int alignCount = 1, Action<int> contentAction = null)
        {
            AlignSpace(sbr, alignCount);
            string inheritStr = "";
            if (inheritClassArr != null && inheritClassArr.Length > 0)
            {
                inheritStr = " : ";
                for (int i = 0; i < inheritClassArr.Length; i++)
                {
                    inheritStr += inheritClassArr[i];
                    if (i != inheritClassArr.Length - 1)
                    {
                        inheritStr += ",";
                    }
                }
            }

            if (isAbstract)
            {
                sbr.Append($"public abstract class {className}{inheritStr}\r\n");
            }
            else if (isPartial)
            {
                sbr.Append($"public partial class {className}{inheritStr}\r\n");
            }
            else
            {
                sbr.Append($"public class {className}{inheritStr}\r\n");
            }
            GenerateActionScopeBegin(sbr, alignCount);
            contentAction?.Invoke(alignCount + 1);
            GenerateActionScopeEnd(sbr, alignCount);

        }
        public static void GenerateUsingCode(StringBuilder sbr, string nameSpace)
        {
            sbr.Append($"using {nameSpace};\r\n");
        }
        /// <summary>
        /// 开始作用域
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="alignCount"></param>
        public static void GenerateActionScopeBegin(StringBuilder sbr, int alignCount)
        {
            AlignSpace(sbr, alignCount);
            sbr.Append("{\r\n");

        }
        /// <summary>
        /// 结束作用域
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="alignCount"></param>
        public static void GenerateActionScopeEnd(StringBuilder sbr, int alignCount)
        {
            AlignSpace(sbr, alignCount);
            sbr.Append("}\r\n");

        }

        public static void GenerateActionScope(StringBuilder sbr, int alignCount, Action<int> contentCode)
        {
            AlignSpace(sbr, alignCount);
            sbr.Append("{\r\n");
            contentCode?.Invoke(alignCount + 1);
            AlignSpace(sbr, alignCount);
            sbr.Append("}\r\n");
        }

        public static void GenerateAnnotation(StringBuilder sbr, int alignCount, string desContent)
        {
            AlignSpace(sbr, alignCount);
            sbr.Append("/// <summary>\r\n");
            AlignSpace(sbr, alignCount);
            sbr.Append($"/// {desContent}\r\n");
            AlignSpace(sbr, alignCount);
            sbr.Append("/// </summary>\r\n");
        }


        /// <summary>
        /// 插入4个空格
        /// </summary>
        /// <param name="sbr"></param>
        /// <param name="count"></param>
        public static void AlignSpace(StringBuilder sbr, int count)
        {
            string result = "";
            for (int i = 0; i < count; ++i)
            {
                result += "    ";
            }
            sbr.Append(result);
        }

        public static void GenerateMemberValue(StringBuilder sbr, int alignCount, Modifier modifier, string typeName, string memberName)
        {
            AlignSpace(sbr, alignCount);
            sbr.Append($"{modifier.ToCode()}{typeName} {memberName};\r\n");
        }

        public static void GenerateConstValue(StringBuilder sbr, int alignCount, Modifier modifier, string typeName, string memberName, string Value)
        {
            AlignSpace(sbr, alignCount);
            sbr.Append($"{modifier.ToCode()}const {typeName} {memberName} = {Value};\r\n");
        }

        public static void GenerateConstructor(StringBuilder sbr, int alignCount, Modifier modifier, string className, Action<StringBuilder, int> onContent, string insertParamStr = "")
        {
            AlignSpace(sbr, alignCount);
            sbr.Append($"{modifier.ToCode()}{className}({insertParamStr})\r\n");
            GenerateActionScope(sbr, alignCount, (cnt1) =>
            {
                onContent?.Invoke(sbr, cnt1);
            });
        }


        public class Json
        {
            public static void GenerateJsonUsing(StringBuilder sbr)
            {
                sbr.Append("using Newtonsoft.Json;\r\n");
            }

            public static void GenerateJsonProperty(StringBuilder sbr, int alignCount, string des, string type, string vName, int index)
            {
                CodeHelper.AlignSpace(sbr, alignCount);
                sbr.Append($"[JsonProperty(\"d{index}\")]\r\n");
                CodeHelper.Property.GenerateProperty_GetSet(sbr, alignCount, type, vName);
            }
            public static void GenerateJsonClass(StringBuilder sbr, int alignCount, string des, string className, Action<int> onContent)
            {
                if (!string.IsNullOrEmpty(des))
                {
                    CodeHelper.GenerateAnnotation(sbr, alignCount, des);
                }
                CodeHelper.AlignSpace(sbr, alignCount);
                sbr.Append($"[JsonObject(MemberSerialization = MemberSerialization.OptIn)]\r\n");
                CodeHelper.GenerateClassCodeLine(sbr, "Data", alignCount: alignCount);
                CodeHelper.GenerateActionScopeBegin(sbr, alignCount);
                onContent?.Invoke(alignCount + 1);
                CodeHelper.GenerateActionScopeEnd(sbr, alignCount);
            }
        }

        public class Enum
        {
            public static void GenerateEnumCode(StringBuilder sbr, int alignCount, string desContent, string enumName, Action<int> contentCode)
            {
                if (!string.IsNullOrEmpty(desContent))
                {
                    AlignSpace(sbr, alignCount);
                    GenerateAnnotation(sbr, alignCount, desContent);
                }
                AlignSpace(sbr, alignCount);
                sbr.Append($"public enum E{enumName}\r\n");
                GenerateActionScope(sbr, alignCount, (count) =>
                {
                    contentCode?.Invoke(count);
                });
            }

            public static void GenerateEnumCode_LayerMask(StringBuilder sbr, int alignCount, string desContent, string enumName, Action<int> contentCode)
            {

            }

            public static void GenerateEnumValue(StringBuilder sbr, int alignCount, string desContent, string enumValueName, int index = -1)
            {
                AlignSpace(sbr, alignCount);
                if (!string.IsNullOrEmpty(desContent))
                {
                    GenerateAnnotation(sbr, alignCount, desContent);
                }
                if (index < 0)
                {
                    sbr.Append($"{enumValueName},\r\n");
                }
                else
                {
                    sbr.Append($"{enumValueName} = {index},\r\n");
                }
            }

            public static void GenerateEnumValue_LayerMask(StringBuilder sbr, int alignCount, string desContent, string enumValueName, int layer)
            {

            }
        }

        public class Property
        {

            /// <summary>
            /// 生成属性代码格式<br/>
            ///<code>
            /// private {propertyTypeName} _{propertyName};<br/>
            /// public {propertyTypeName} {propertyName}<br/>
            /// {<br/>
            ///     {getPropertyModifier} get<br/>
            ///     {<br/>
            ///         ... getGenerateCodeAction 生成代码 ...
            ///     }<br/>
            ///     {setPropertyModifier} set<br/>
            ///     {<br/>
            ///         ... setGenerateCodeAction 生成代码...
            ///     }<br/>
            /// }<br/>
            /// </code>
            /// </summary>
            /// <param name="sbr"></param>
            /// <param name="alignCount"></param>
            /// <param name="getPropertyModifier"></param>
            /// <param name="setPropertyModifier"></param>
            public static void GeneratePropertyBegin(StringBuilder sbr, int alignCount, string propertyTypeName, string propertyName, Modifier? getPropertyModifier = null,
                Action<int> getGenerateCodeAction = null, Modifier? setPropertyModifier = null, Action<int> setGenerateCodeAction = null)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"public {propertyTypeName} {propertyName}\r\n");
                GenerateActionScopeBegin(sbr, alignCount);
                //生成get
                if (getGenerateCodeAction != null)
                {
                    AlignSpace(sbr, alignCount + 1);
                    sbr.Append($"{getPropertyModifier.ToCode()}get\r\n");
                    GenerateActionScopeBegin(sbr, alignCount + 1);
                    getGenerateCodeAction?.Invoke(alignCount + 2);
                    GenerateActionScopeEnd(sbr, alignCount + 1);
                }
                else
                {
                    AlignSpace(sbr, alignCount + 1);
                    sbr.Append($"{getPropertyModifier.ToCode()}get;\r\n");
                }
                //生成set
                if (setGenerateCodeAction != null)
                {
                    AlignSpace(sbr, alignCount + 1);
                    sbr.Append($"{setPropertyModifier.ToCode()}set\r\n");
                    GenerateActionScopeBegin(sbr, alignCount + 1);
                    setGenerateCodeAction?.Invoke(alignCount + 2);
                    GenerateActionScopeEnd(sbr, alignCount + 1);
                }
                else
                {
                    AlignSpace(sbr, alignCount + 1);
                    sbr.Append($"{setPropertyModifier.ToCode()}set;\r\n");
                }

                GenerateActionScopeEnd(sbr, alignCount);

            }

            /// <summary>
            /// 生成属性代码格式<br/>
            ///<code>
            /// private {propertyTypeName} _{propertyName};<br/>
            /// public {propertyTypeName} {propertyName}<br/>
            /// {<br/>
            ///     {getPropertyModifier} get<br/>
            ///     {<br/>
            ///         return _{propertyName};<br/>
            ///     }<br/>
            ///     {setPropertyModifier} set<br/>
            ///     {<br/>
            ///         _{propertyName} = value;<br/>
            ///     }<br/>
            /// }<br/>
            /// </code>
            /// </summary>
            /// <param name="sbr"></param>
            /// <param name="alignCount"></param>
            /// <param name="getPropertyModifier"></param>
            /// <param name="setPropertyModifier"></param>
            public static void GenerateProperty_ReferenceMode(StringBuilder sbr, int alignCount, string propertyTypeName, string propertyName, string des = null, Modifier? getPropertyModifier = null, Modifier? setPropertyModifier = null)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"private {propertyTypeName} _{propertyName};\r\n");

                if (!string.IsNullOrEmpty(des))
                {
                    GenerateAnnotation(sbr, alignCount, des);
                }
                AlignSpace(sbr, alignCount);
                sbr.Append($"public {propertyTypeName} {propertyName}\r\n");
                GenerateActionScopeBegin(sbr, alignCount);
                #region get
                AlignSpace(sbr, alignCount + 1);
                sbr.Append($"{getPropertyModifier.ToCode()}get\r\n");
                GenerateActionScopeBegin(sbr, alignCount + 1);
                AlignSpace(sbr, alignCount + 2);
                sbr.Append($"return _{propertyName};\r\n");
                GenerateActionScopeEnd(sbr, alignCount + 1);
                #endregion

                #region set
                AlignSpace(sbr, alignCount + 1);
                sbr.Append($"{setPropertyModifier.ToCode()}set\r\n");
                GenerateActionScopeBegin(sbr, alignCount + 1);
                AlignSpace(sbr, alignCount + 2);
                sbr.Append($"_{propertyName} = value;\r\n");
                GenerateActionScopeEnd(sbr, alignCount + 1);
                #endregion

                GenerateActionScopeEnd(sbr, alignCount);
            }

            public static void GeneratePropertyEnd(StringBuilder sbr, int alignCount)
            {

            }

            public static void GenerateProperty_GetSet(StringBuilder sbr, int alignCount, string propertyTypeName, string propertyName, string des = null, Modifier? getPropertyModifier = null, Modifier? setPropertyModifier = null)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"public {propertyTypeName} {propertyName} {{ {getPropertyModifier.ToCode()}get; {getPropertyModifier.ToCode()}set; }}\r\n");
            }


        }

        public class Struct
        {
            public static void GenrateStructCode(StringBuilder sbr, string structName, int alignCount, Action<StringBuilder, int> onFuncContent)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"public struct {structName}\r\n");
                GenerateActionScopeBegin(sbr, alignCount);
                onFuncContent?.Invoke(sbr, alignCount + 1);
                GenerateActionScopeEnd(sbr, alignCount);
            }
        }

        public class ExcelBaseData
        {


            public static void GenerateCreateDataFunc(StringBuilder sbr, int alignCount, string keyType, Action<StringBuilder, int> onFuncContent)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"protected override bool CreateData(string[] strArr, out {keyType} resultKey, out Data resultData)\r\n");
                GenerateActionScopeBegin(sbr, alignCount);
                AlignSpace(sbr, alignCount + 1);
                sbr.Append($"resultData = new Data();\r\n");
                onFuncContent?.Invoke(sbr, alignCount + 1);
                AlignSpace(sbr, alignCount + 1);
                sbr.Append($"return true;\r\n");
                GenerateActionScopeEnd(sbr, alignCount);
            }
            public static void GenerateGetKeyByIndexFunc(StringBuilder sbr, int alignCount, string keyType, string keyName)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"public {keyType} Get{keyName}ByIndex(int idx) => GetKeyByIndex(idx);\r\n");

            }

            public static void GenerateGetValueByKeyNameFunc(StringBuilder sbr, int alignCount, string valueType, string valueName, string keyType, string keyName)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"public {valueType} Get{valueName}By{keyName}({keyType} {keyName}) => GetDataByKey({keyName}).{valueName};\r\n");
            }

            public static void GenerateReadData(StringBuilder sbr, int alignCount, string typeName, string valueName, int readIndex)
            {
                AlignSpace(sbr, alignCount);
                sbr.Append($"{valueName} = {GetReadDataFuncString(typeName, readIndex)}\r\n");
            }

            private static string GetReadDataFuncString(string typeName, int readIndex)
            {
                string resultStr = "";
                switch (typeName)
                {
                    case "double":
                        resultStr = $"GetDouble(strArr, {readIndex});";
                        break;
                    case "int":
                        resultStr = $"int.Parse(strArr[{readIndex}]);";
                        break;
                    case "float":
                        resultStr = $"GetFloat(strArr, {readIndex});";
                        break;
                    case "string":
                        resultStr = $"strArr[{readIndex}];";
                        break;
                    case "long":
                        resultStr = $"GetLong(strArr, {readIndex});";
                        break;
                    case "bool":
                        resultStr = $"GetBool(strArr, {readIndex});";
                        break;
                    case "int[]":
                        resultStr = $"GetIntArray(strArr, {readIndex});";
                        break;
                    case "float[]":
                        resultStr = $"GetFloatArray(strArr, {readIndex});";
                        break;
                    case "long[]":
                        resultStr = $"GetLongArray(strArr, {readIndex});";
                        break;
                    case "string[]":
                        resultStr = $"GetStringArray(strArr, {readIndex});";
                        break;
                    case "Vector3":
                        resultStr = $"GetVector3(strArr, {readIndex});";
                        break;
                }

                if (string.IsNullOrEmpty(resultStr))
                {
                    resultStr = $"new {typeName}(strArr, {readIndex});";
                    //Debug.Log($"发现不支持的配置类型 {argType} ");
                }
                return resultStr;
            }

        }
    }
}
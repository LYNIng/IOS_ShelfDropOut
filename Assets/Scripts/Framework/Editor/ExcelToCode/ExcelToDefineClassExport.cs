
using FileExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UIElements;
using static FileExport.FileExportBase;

namespace ReadExcel
{
    public class ExcelDefineClassParam
    {
        public string valueName;
        public string des;
        public string type;
        public string modifier;
        public string defaultValue;
        public string port;
        public string configTag;
        public string configLink;

        private List<string> tags;
        public bool IsSaveDataTag()
        {
            return CheckCofnigTag("SaveData");
        }

        public bool CheckCofnigTag(string tag)
        {
            if (string.IsNullOrEmpty(configTag))
                return false;
            if (tags == null)
            {
                tags = new List<string>();
                var arr = configTag.Split('|', StringSplitOptions.RemoveEmptyEntries);
                tags.AddRange(arr);
            }

            return configTag.Contains(tag);
        }
    }
    public class ExcelDefineClass
    {
        public string[,] dataArr;

        public int rows;
        public int columns;

        public List<ExcelDefineClassParam> paramList;

        public string GetName(int row)
        {
            return dataArr[0, row];
        }

        public string GetDes(int row)
        {

            return dataArr[1, row];
        }
        public string GetType(int row)
        {

            return dataArr[2, row];
        }
        public string GetModifier(int row)
        {

            return dataArr[3, row];
        }
        public string GetDefineValue(int row)
        {

            return dataArr[4, row];
        }
        public string GetPort(int row)
        {

            return dataArr[5, row];
        }
        public string GetConfigTag(int row)
        {

            return dataArr[6, row];
        }
        public string GetConfigLink(int row)
        {

            return dataArr[7, row];
        }
    }



    public class ExcelDefineClassResolver : SourceFileResolver
    {
        public override bool Resolve(object readData, out object result)
        {
            result = null;
            var st = readData as SheetSource;
            if (st == null) return false;

            ExcelDefineClass exportData = new ExcelDefineClass();
            int row = exportData.rows = st.row;
            int columns = exportData.columns = st.column;
            exportData.dataArr = new string[columns, row];

            for (int j = 0; j < columns; ++j)
            {
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
                }
            }
            exportData.paramList = new List<ExcelDefineClassParam>();

            for (int i = 4; i < row; i++)
            {
                var tmp = new ExcelDefineClassParam();
                tmp.valueName = exportData.GetName(i);
                tmp.des = exportData.GetDes(i);
                tmp.type = exportData.GetType(i);
                if (string.IsNullOrEmpty(tmp.valueName) || string.IsNullOrEmpty(tmp.des)) continue;
                tmp.modifier = exportData.GetModifier(i);
                tmp.defaultValue = exportData.GetDefineValue(i);
                tmp.port = exportData.GetPort(i);
                tmp.configTag = exportData.GetConfigTag(i);
                tmp.configLink = exportData.GetConfigLink(i);
                exportData.paramList.Add(tmp);
            }

            result = exportData;
            return true;
        }
    }
    public class ExcelDefineClassMaker : SourceFileDataMaker
    {
        public override bool GenerateFile(string exportDir, Dictionary<string, FileExportData> exportData)
        {
            string exportFileDir = $"{exportDir}/DefineClass";
            if (!Directory.Exists(exportFileDir))
            {
                Directory.CreateDirectory(exportFileDir);
            }
            foreach (var kvp in exportData)
            {
                var fex = kvp.Value;
                var excelData = fex.sourceResolveData as ExcelDefineClass;
                var fileName = fex.fileInfo.Name.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];

                int row = excelData.rows;
                int col = excelData.columns;
                var dataArr = excelData.dataArr;

                StringBuilder sbr = new StringBuilder();
                int endCol = col - 1;

                CodeHelper.GenerateTitle(sbr);
                CodeHelper.Json.GenerateJsonUsing(sbr);


                CodeHelper.GenerateClassCode(sbr, fileName, alignCount: 0, isPartial: true, contentAction: (align) =>
                {
                    GenerateSaveData(sbr, align, excelData);

                    for (int i = 0; i < excelData.paramList.Count; ++i)
                    {
                        var param = excelData.paramList[i];
                        var vName = param.valueName;
                        var type = param.type;
                        var des = param.des;

                        CodeHelper.Property.GenerateProperty_ReferenceMode(sbr, align, type, vName, des,
                            null, CodeHelper.Modifier.Private);


                    }
                });

                CreateFile(exportFileDir, $"{fileName}.Code", "cs", sbr);
            }

            return true;
        }

        public void GenerateSaveData(StringBuilder sbr, int alignCount, ExcelDefineClass excelDefineClass)
        {

            CodeHelper.Json.GenerateJsonClass(sbr, alignCount, null, "SaveData", (align) =>
            {
                for (int i = 0; i < excelDefineClass.paramList.Count; ++i)
                {
                    var param = excelDefineClass.paramList[i];


                    var vName = param.valueName;
                    var type = param.type;
                    var des = param.des;

                    if (param.IsSaveDataTag())
                    {
                        CodeHelper.Json.GenerateJsonProperty(sbr, align, null, type, vName, i);
                    }
                }
            });
        }
    }


    public class ExcelToDefineClassExport : FileExportBase
    {
        public const string C_LINKNAME = " DataDefines";
        public ExcelToDefineClassExport() :
            base(new ExcelFileReader(), new ExcelDefineClassResolver()
                , new SourceFileDataMaker[] { new ExcelDefineClassMaker() })
        {

        }

        public override HashSet<string> fileTypeFilterater => new HashSet<string> { ".xlsx" };

        public override string LinkName => C_LINKNAME;
    }

}
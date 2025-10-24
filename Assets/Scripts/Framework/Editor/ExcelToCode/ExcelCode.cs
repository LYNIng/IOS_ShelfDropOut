using FileExport;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

#if UNITY_EDITOR

namespace ReadExcel
{

    public class ExcelExportHelper
    {
        private Dictionary<string, FileExportBase> fileExportBase;

        public static ExcelExportHelper Instance { get; private set; }
        public ExcelExportHelper()
        {
            Instance = this;

            fileExportBase = new Dictionary<string, FileExportBase>();
            string[] dirs = new string[] {
                EditorPathHelper.Editor_ExcelConfigsFolderPath,
            };

            ExcelToConfigFileExport export = new ExcelToConfigFileExport();
            fileExportBase.Add(export.LinkName, export);
            export.AddExportDir(dirs);


            string[] classDirs = new string[] {
                EditorPathHelper.Editor_ExcelDataDefineFolderPath,
            };
            ExcelToDefineClassExport defineClassExport = new ExcelToDefineClassExport();
            fileExportBase.Add(defineClassExport.LinkName, defineClassExport);
            defineClassExport.AddExportDir(classDirs);
        }

        public void ExcuteReadFile()
        {
            foreach (var kvp in fileExportBase)
            {
                kvp.Value.ReadFile();
            }
        }

        public void ExcuteResolve()
        {
            foreach (var kvp in fileExportBase)
            {
                kvp.Value.Resolve();
            }
        }

        public void ExcuteExportFile()
        {
            if (fileExportBase.TryGetValue(ExcelToConfigFileExport.C_LINKNAME, out var resultItem))
            {
                resultItem.ExportFile(EditorPathHelper.Editor_HotUpdate_AutoCode_GenerateCodePath);
            }
            if (fileExportBase.TryGetValue(ExcelToDefineClassExport.C_LINKNAME, out resultItem))
            {
                resultItem.ExportFile(EditorPathHelper.Editor_HotUpdate_AutoCode_GenerateCodePath);
            }

        }
    }

    public class ExcelCodeUtil
    {
        [MenuItem("ExcelCode/导出Excel数据配置文件")]
        public static void ExportExcelCode()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var helper = new ExcelExportHelper();

            //读取Excel
            helper.ExcuteReadFile();
            //解析
            helper.ExcuteResolve();
            //导出文件
            helper.ExcuteExportFile();

            AssetDatabase.Refresh();
        }

        public static void GetAllFile(string dirPath, out string allfile)
        {
            GetAllFile(dirPath, null, out allfile);
        }

        public static void GetAllFile(string dirPath, HashSet<string> filtrater, ref List<FileInfo> resultList)
        {
            if (resultList == null)
            {
                return;
            }
            DirectoryInfo dirs = new DirectoryInfo(dirPath);
            if (!dirs.Exists)
            {
                return;
            }
            FileInfo[] files = dirs.GetFiles();//获得目录下文件对          


            for (int i = 0; i < files.Length; ++i)
            {
                var file = files[i];
                if (ExcelCodeUtil.IsTemporaryFile(file.Extension))
                {
                    continue;
                }

                if (filtrater == null)
                {
                    resultList.Add(file);
                }
                if (filtrater.Contains(file.Extension))
                {
                    resultList.Add(file);
                }
            }


        }

        /// <summary>
        /// 将所有文件路径整合在一起
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="allfile"></param>
        public static void GetAllFile(string dirPath, HashSet<string> filtrater, out string allfile)
        {
            allfile = "";
            DirectoryInfo dirs = new DirectoryInfo(dirPath);
            FileInfo[] file = dirs.GetFiles();//获得目录下文件对          
                                              //循环文件
            for (int j = 0; j < file.Length; j++)
            {
                var temp = file[j];
                if (ExcelCodeUtil.IsTemporaryFile(temp.Extension))
                {
                    continue;
                }

                if (filtrater == null)
                {
                    allfile += dirPath + "/" + temp.Name + "\r\n";
                }
                else if (filtrater.Contains(temp.Extension))
                {
                    allfile += dirPath + "/" + temp.Name + "\r\n";
                }
            }
            allfile = allfile.Replace(@"/", "\\");

        }

        /// <summary>
        /// like ~$Equip
        /// </summary>
        /// <returns></returns>
        public static bool IsTemporaryFile(string fileName)
        {
            return Regex.Match(fileName, @"^\~\$").Success;
        }
    }


}

#endif
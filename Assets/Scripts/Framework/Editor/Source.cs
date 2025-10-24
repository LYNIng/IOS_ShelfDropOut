
using System.Collections.Generic;


namespace ReadExcel
{

    public static class Util
    {
        public static bool IsNullOrEmpty(this string value)
        {
            if (value == null)
            {
                return true;
            }
            return string.IsNullOrEmpty(value.Trim());
        }


    }

    /// <summary>
    /// 源数据
    /// </summary>
    public class Source
    {
        /// <summary>
        /// 原始内容(string/DataTable)
        /// </summary>
        public object original;

        /// <summary>
        /// 源文件的文件名
        /// </summary>
        public string originalName;

        /// <summary>
        /// 类名
        /// </summary>
        public string className;
    }


    /// <summary>
    /// 源类型
    /// </summary>
    public enum SourceType
    {
        Sheet,//表格
        Struct,//结构
    }

       
    /// <summary>
    /// 结构型数据源
    /// </summary>
    public class StructSource : Source
    {
        public Dictionary<string, object> obj;
    }


    /// <summary>
    /// 表格（.txt .csv .xls .xlsx）
    /// </summary>
    public class SheetSource : Source
    {
        /// <summary>
        /// 解析出来的矩阵
        /// </summary>
        public string[,] matrix;

        /// <summary>
        /// 行
        /// </summary>
        public int row
        {
            get { return matrix.GetLength(0); }
        }

        /// <summary>
        /// 列
        /// </summary>
        public int column
        {
            get { return matrix.GetLength(1); }
        }
    }
}

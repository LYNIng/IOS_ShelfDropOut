using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace ReadExcel
{

    /// <summary>
    /// SheetParser
    /// </summary>
    public class SheetParser
    {

        /// <summary>
        /// Excel
        /// </summary>
        /// <param name="tabel"></param>
        /// <returns></returns>
        public static SheetSource Parse(ExcelWorksheet sheet, string fileName)
        {
            int row = sheet.Dimension.Rows;
            int column = sheet.Dimension.Columns;

            string[,] matrix = new string[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    string value = sheet.GetValue<string>(i + 1, j + 1);
                    matrix[i, j] = value;
                }
            }
            //ConvertOriginalType(matrix);
            string originalName = fileName.Substring(0, fileName.LastIndexOf('.'));
            string className = originalName;
            return CreateSource(sheet, originalName, className, matrix);
        }


        private static SheetSource CreateSource(object original, string originalName, string className, string[,] matrix)
        {
            SheetSource source = new SheetSource();
            source.original = original;
            source.originalName = originalName;//文件名
            source.className = className;//类名
            source.matrix = matrix;
            return source;
        }

        /// <summary>
        /// 从配置转换成矩阵数组（0行1列）
        /// </summary>
        /// <param name="config">配置文件</param>
        /// <param name="sv">分隔符 Separated Values</param>
        /// <param name="lf">换行符 Line Feed</param>
        /// <returns></returns>
        private static string[,] Content2Matrix(string config, string sv, string lf)
        {
            config = config.Trim();//清空末尾的空白

            //分割
            string[] lines = Regex.Split(config, lf);
            string[] firstLine = Regex.Split(lines[0], sv, RegexOptions.Compiled);

            int row = lines.Length;
            int col = firstLine.Length;
            string[,] matrix = new string[row, col];
            //为第一行赋值
            for (int i = 0, l = firstLine.Length; i < l; i++)
            {
                matrix[0, i] = firstLine[i];
            }
            //为其他行赋值
            for (int i = 1, l = lines.Length; i < l; i++)
            {
                string[] line = Regex.Split(lines[i], sv);
                for (int j = 0, k = line.Length; j < k; j++)
                {
                    matrix[i, j] = line[j];
                }
            }
            return matrix;
        }



        /// <summary>
        /// 去除CSV引号
        /// </summary>
        /// <param name="matrix"></param>
        private static void ClearCsvQuotation(string[,] matrix)
        {
            int row = matrix.GetLength(0);
            int column = matrix.GetLength(1);
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    string v = matrix[y, x];
                    if (string.IsNullOrEmpty(v) || v.Length < 2)
                        continue;

                    if (v[0] == '"')
                    {
                        v = v.Remove(0, 1);
                        v = v.Remove(v.Length - 1, 1);
                        matrix[y, x] = v;
                    }
                }
            }
        }





    }

}

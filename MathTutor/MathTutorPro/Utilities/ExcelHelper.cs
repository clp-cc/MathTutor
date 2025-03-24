using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.Distributions;
using System.Numerics;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace MathTutorPro.Utilities
{
    public class ExcelHelper
    {
        public static List<StudentImportModel> ReadStudentList(Stream stream)
        {
            var students = new List<StudentImportModel>();

            // 创建Excel工作簿对象（支持xlsx格式）
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);  // 获取第一个工作表
            var formatter = new DataFormatter(); // 用于获取单元格文本内容

            // 从第2行开始读取（索引1对应Excel中的第2行）
            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;  // 跳过空行

                if (string.IsNullOrEmpty(formatter.FormatCellValue(row.GetCell(0)).Trim()))
                    throw new InvalidDataException($"第{row}行学号不能为空");
                if (!Regex.IsMatch(formatter.FormatCellValue(row.GetCell(3)).Trim(), @"^1[3-9]\d{9}$"))
                    throw new InvalidDataException($"第{row}行手机号格式错误");

                students.Add(new StudentImportModel
                {
                    // 注意NPOI的列索引从0开始
                    StudentNo = formatter.FormatCellValue(row.GetCell(0)).Trim(),
                    UserName = formatter.FormatCellValue(row.GetCell(1)).Trim(),
                    Gender = formatter.FormatCellValue(row.GetCell(2)).Trim() == "男" ? 1 : 2,
                    Phone = formatter.FormatCellValue(row.GetCell(3)).Trim()
                });
            }

            return students;
        }

        public class StudentImportModel
        {
            public string StudentNo { get; set; }
            public string UserName { get; set; }
            public int Gender { get; set; }
            public string Phone { get; set; }
        }
    }
}

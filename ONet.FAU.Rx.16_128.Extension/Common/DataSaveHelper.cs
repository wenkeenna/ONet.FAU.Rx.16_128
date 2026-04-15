using DM.Foundation.Logging.Interfaces;
using ONet.FAU.Rx._16_128.Extension.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Common
{
    public class DataSaveHelper
    {
        public static void ExportToCsv(List<CouplingData> data, ILogger logger, string name)
        {

            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}-{name}.csv";
            string directory = @"D:\TestData\";
            string fullPath = Path.Combine(directory, fileName);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (data == null || data.Count == 0)
            {
                logger?.Error("ExportToCsv: 数据为空，没有可导出的内容。");
                return;
            }

            try
            {
                int maxValueCount = data.Max(d => d.Values != null ? d.Values.Count : 0);

                using (StreamWriter writer = new StreamWriter(fullPath, false, Encoding.UTF8))
                {
                    // 表头
                    writer.Write("Pos");
                    for (int i = 0; i < maxValueCount; i++)
                        writer.Write($",Value{i}");
                    writer.WriteLine();

                    // 数据
                    foreach (var item in data)
                    {
                        writer.Write($"{item.Pos}");

                        for (int i = 0; i < maxValueCount; i++)
                        {
                            if (item.Values != null && i < item.Values.Count)
                                writer.Write($",{item.Values[i]}");
                            else
                                writer.Write(",");
                        }
                        writer.WriteLine();
                    }

                    logger?.Info($"ExportToCsv: 成功导出 {data.Count} 条数据到文件 {fullPath}");
                }
            }
            catch (Exception ex)
            {
                logger?.Error(ex.ToString(), "ExportToCsv: 导出CSV失败");
                throw;
            }

        }
    }
}

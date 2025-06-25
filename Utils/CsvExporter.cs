using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CumbyMinerScan.Models;
using CumbyMinerScanV2.Models;

public static class CsvExporter
{
    public static void ExportToCsv(List<MinerDetail> miners, string filePath)
    {
        var csv = new StringBuilder();

        // 写入标题行
        csv.AppendLine("IP,Model,Hashrate,Fan1Speed,Fan2Speed,Status");

        // 写入数据行
        foreach (var miner in miners)
        {
            var line =
                $"{miner.Ip},{miner.Issue},{miner.HashRealTime},{miner.HashAverage},{miner.IssueDetail},{miner.IssueMemo}";
            csv.AppendLine(line);
        }

        // 写入到文件
        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }
}
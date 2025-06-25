namespace CumbyMinerScanV2.Utils;
using System;
using System.Collections.Generic;
public class IpRangeHelper
{
    public static List<string> GetIpRanges(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            throw new ArgumentException("输入格式不正确，应为类似 '1A' 或 '10B'");

        // 解析数字部分（支持1位或2位数字）
        int index = 0;
        while (index < input.Length && char.IsDigit(input[index]))
            index++;

        if (index == 0 || index >= input.Length)
            throw new ArgumentException("输入格式不正确，缺少字母部分");

        string numberPart = input.Substring(0, index);
        string letterPart = input.Substring(index).ToUpper();

        if (!int.TryParse(numberPart, out int number))
            throw new ArgumentException("数字部分格式错误");

        var ipList = new List<string>();

        // 根据字母部分确定IP规则
        if (letterPart == "A")
        {
            // A组：第二段是 number + "1"，第三段是1和2
            string secondSegment = $"{number}1";
            for (int i = 1; i <= 168; i++)
            {
                ipList.Add($"10.{secondSegment}.1.{i}");
            }
            for (int i = 1; i <= 168; i++)
            {
                ipList.Add($"10.{secondSegment}.2.{i}");
            }
        }
        else if (letterPart == "B")
        {
            // B组：第二段是 number + "2"，第三段是1和2
            string secondSegment = $"{number}2";
            for (int i = 1; i <= 168; i++)
            {
                ipList.Add($"10.{secondSegment}.1.{i}");
            }
            for (int i = 1; i <= 168; i++)
            {
                ipList.Add($"10.{secondSegment}.2.{i}");
            }
        }
        else
        {
            throw new ArgumentException("字母部分应为 A 或 B");
        }

        return ipList;
    }
}
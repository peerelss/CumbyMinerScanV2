using System.Threading.Tasks;
using CumbyMinerScanV2.Models;

namespace CumbyMinerScanV2.Utils;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

//给定一个ip判断这个ip是否有问题。首先判断这个ip是否在线。如果离线则返回ip,offline
public class MinerHelper
{
    public static async Task<MinerDetail> IsMinerAvailable(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            throw new ArgumentException("IP地址不能为空");
        }

        bool isOnline = false;
        try
        {
            using (Ping ping = new Ping())
            {
                var reply = await ping.SendPingAsync(ip, 1000);
                isOnline = reply.Status == IPStatus.Success;
            }
        }
        catch
        {
        }

        if (!isOnline)
        {
            return new MinerDetail(ip, "offline", 0f, 0f, "设备不在线", "");
        }
        else
        {
            return new MinerDetail(ip, "OK", 85.3f, 90.1f, "正常运行", "模拟数据");
        }

        // 模拟获取矿机数据
    }
}
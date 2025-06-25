using System.Text.Json;
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
            return new MinerDetail(ip, "offline", 0f, 0f, 0,"offline","设备不在线");
        }
        else
        {
            try
            {
                string json =
                    await HttpHelper.GetDigestProtectedResourceAsync($"http://{ip}/cgi-bin/stats.cgi", "root", "root",
                        "{}");
                MinerBasicInfo info = MinerParser.ParseMinerInfo(json);
                Console.WriteLine($"Rate5s: {info.Rate5s}");
                Console.WriteLine($"RateAvg: {info.RateAvg}");
                Console.WriteLine($"FanNum: {info.FanNum}");
                Console.WriteLine($"Fans: {string.Join(", ", info.Fans)}");
                Console.WriteLine($"ChainNum: {info.ChainNum}");
                if (info.Rate5s > 0)
                {
                    // 正常
                    return new MinerDetail(ip, "OK", info.Rate5s, info.RateAvg, info.Elapsed,"正常运行", "正常运行");
                }
                else
                {
                    if (info.FanNum < 4) //风扇问题
                    {
                        return new MinerDetail(ip, "fans", info.Rate5s, info.RateAvg,info.Elapsed, "fans issue",
                            $"Fans: {string.Join(", ", info.Fans)}");
                    }
                    else if (info.ChainNum < 3) //芯片问题
                    {
                        return new MinerDetail(ip, "chains", info.Rate5s, info.RateAvg, info.Elapsed,"chains issue",
                            $"ChainNum: {info.ChainNum}");
                    }

                    string log =
                        await HttpHelper.GetDigestProtectedResourceAsync($"http://{ip}//cgi-bin/log.cgi", "root",
                            "root",
                            "{}");

                    var logResult = LogHelper.ParseLog(log);
                    var logMemo = logResult[1];
                    if (LogHelper.ErrorFanLost.Equals(logResult[0]))
                    {
                        logMemo = $"Fans: {string.Join(", ", info.Fans)}";
                    }

                    return new MinerDetail(ip, "no good", info.Rate5s, info.RateAvg,info.Elapsed, logResult[0], logMemo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"出错: {ex.Message}");
            }

            return new MinerDetail(ip, "OK", 85.3f, 90.1f, 0,"正常运行", "模拟数据");
        }

        // 模拟获取矿机数据
    }
}
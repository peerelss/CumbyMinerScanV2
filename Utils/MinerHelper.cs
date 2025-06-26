using System.Text.Json;
using System.Threading.Tasks;
using CumbyMinerScanV2.Models;

namespace CumbyMinerScanV2.Utils;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq; // 安装 Newtonsoft.Json

//给定一个ip判断这个ip是否有问题。首先判断这个ip是否在线。如果离线则返回ip,offline
public class MinerHelper
{
    public static async Task<bool> IsMinerOnline(string ip)
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

        return isOnline;
    }

    public static async Task<MinerDetail> IsMinerAvailable(string ip)
    {
        var isOnline = await IsMinerOnline(ip);

        if (!isOnline)
        {
            return new MinerDetail(ip, "offline", 0f, 0f, 0, LogHelper.ErrorOffline, "设备不在线");
        }
        else
        {
            try
            {
                string json =
                    await HttpHelper.GetDigestProtectedResourceAsync($"http://{ip}/cgi-bin/stats.cgi", "root", "root",
                        "{}");
                MinerBasicInfo info = MinerParser.ParseMinerInfo(json);

                if (info.Rate5s > 0)
                {
                    // 正常
                    return new MinerDetail(ip, "OK", info.Rate5s, info.RateAvg, info.Elapsed, LogHelper.NormalOk,
                        "正常运行");
                }
                else
                {
                    if (info.FanNum < 4) //风扇问题
                    {
                        return new MinerDetail(ip, "fans", info.Rate5s, info.RateAvg, info.Elapsed,
                            LogHelper.ErrorFanLost,
                            $"Fans: {string.Join(", ", info.Fans)}");
                    }
                    else if (info.ChainNum < 3) //芯片问题
                    {
                        return new MinerDetail(ip, "chains", info.Rate5s, info.RateAvg, info.Elapsed,
                            LogHelper.ErrorNotEnoughChain,
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

                    return new MinerDetail(ip, "no good", info.Rate5s, info.RateAvg, info.Elapsed, logResult[0],
                        logMemo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"出错: {ex.Message}");
            }

            return new MinerDetail(ip, "OK", 85.3f, 90.1f, 0, LogHelper.NormalOk, "模拟数据");
        }

        // 模拟获取矿机数据
    }

    public static async Task<MinerDetail> RebootMiner(MinerDetail minerDetail)
    {
        bool isOnline = await IsMinerOnline(minerDetail.Ip);
        if (isOnline)
        {
            try
            {
                string result =
                    await HttpHelper.GetDigestProtectedResourceAsync($"http://{minerDetail.Ip}/cgi-bin/reboot.cgi",
                        "root", "root",
                        "{}");
                string[] results = result.Split(' ');
                if (results.Length > 1 && int.Parse(results[1]) == 200)
                {
                    return new MinerDetail(minerDetail.Ip, minerDetail.Issue, minerDetail.HashRealTime,
                        minerDetail.HashAverage, minerDetail.ElapsedTime, minerDetail.IssueDetail,
                        "重启成功");
                }
            }
            catch
            {
            }
        }

        return new MinerDetail(minerDetail.Ip, minerDetail.Issue, minerDetail.HashRealTime,
            minerDetail.HashAverage, minerDetail.ElapsedTime, minerDetail.IssueDetail,
            "重启失败");
        ;
    }

    public static async Task<MinerDetail> LightOnMiner(MinerDetail minerDetail)
    {
        bool isOnline = await IsMinerOnline(minerDetail.Ip);
        if (isOnline)
        {
            try
            {
                string resultJSon =
                    await HttpHelper.GetDigestProtectedResourceAsync($"http://{minerDetail.Ip}/cgi-bin/blink.cgi",
                        "root", "root",
                        HttpHelper._lightOnData);

                var code = JObject.Parse(resultJSon)["code"]?.ToString();
                if (code.Equals("B000"))
                {
                    return new MinerDetail(minerDetail.Ip, minerDetail.Issue, minerDetail.HashRealTime,
                        minerDetail.HashAverage, minerDetail.ElapsedTime, minerDetail.IssueDetail,
                        "点亮成功");
                }
            }
            catch
            {
            }
        }

        return new MinerDetail(minerDetail.Ip, minerDetail.Issue, minerDetail.HashRealTime,
            minerDetail.HashAverage, minerDetail.ElapsedTime, minerDetail.IssueDetail,
            "点亮失败");
        ;
    }
}
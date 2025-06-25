using System;
using System.Collections.Generic;
using System.Text.Json;

public class MinerBasicInfo
{
    public double Rate5s { get; set; }
    public double RateAvg { get; set; }
    public int ChainNum { get; set; }
    public int FanNum { get; set; }
    public int Elapsed { get; set; }
    public List<int> Fans { get; set; }
}

public class MinerStatResponse
{
    public List<Stat> STATS { get; set; }
}

public class Stat
{
    public double rate_5s { get; set; }
    public double rate_avg { get; set; }
    public int chain_num { get; set; }
    public int fan_num { get; set; }
    public int elapsed { get; set; }
    public List<int> fan { get; set; }
}

public static class MinerParser
{
    public static MinerBasicInfo ParseMinerInfo(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<MinerStatResponse>(json, options);

        if (data?.STATS == null || data.STATS.Count == 0)
            throw new Exception("Invalid or empty STATS array");

        var stat = data.STATS[0];

        return new MinerBasicInfo
        {
            Rate5s = stat.rate_5s,
            RateAvg = stat.rate_avg,
            ChainNum = stat.chain_num,
            FanNum = stat.fan_num,
            Fans = stat.fan,
            Elapsed = stat.elapsed,
        };
    }
}
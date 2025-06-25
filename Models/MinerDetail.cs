namespace CumbyMinerScanV2.Models;

public class MinerDetail
{
    public string Ip { get; set; }
    public string Issue { get; set; }
    public double HashRealTime { get; set; }
    public double HashAverage { get; set; }
    public string IssueDetail { get; set; }
    public string IssueMemo { get; set; }

    public MinerDetail(string ip, string issue, double hashRealTime, double hashAverage, string issueDetail,
        string issueMemo)
    {
        // ip
        Ip = ip;
        // 问题
        Issue = issue;
        // 实时算力
        HashRealTime = hashRealTime;
        // 平均算力
        HashAverage = hashAverage;
        // 故障详情，比如几号风扇有问题
        IssueDetail = issueDetail;
        // 关于故障的原始数据
        IssueMemo = issueMemo;
    }

    public override string ToString()
    {
        return $"IP: {Ip}\n" +
               $"Issue: {Issue}\n" +
               $"hash real time: {HashRealTime} TH/s\n" +
               $"hash average: {HashAverage} TH/s\n" +
               $"issue detail: {IssueDetail}\n" +
               $"issue memo: {IssueMemo}";
    }
}
using System.Collections.Generic;

namespace CumbyMinerScanV2.Models;

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
    public List<int> fan { get; set; }
}
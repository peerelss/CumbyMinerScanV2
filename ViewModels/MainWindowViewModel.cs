using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CumbyMinerScan.Models;
using CumbyMinerScanV2.Utils;
using ReactiveUI; // Avalonia 官方 MVVM 推荐用 ReactiveUI
using System.Threading.Tasks;
using CumbyMinerScanV2.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CumbyMinerScanV2.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _issueSummary;

    public string IssueSummary
    {
        get => _issueSummary;
        set { _issueSummary = value; }
    }

    private string _messageText = "这里显示操作结果";
    public bool FanIssue { get; set; }
    public bool TempIssue { get; set; }
    public bool NetIssue { get; set; }
    public bool PowerIssue { get; set; }
    public bool UnknownIssue { get; set; }
    public bool OfflineIssue { get; set; }
    public bool NoIssue { get; set; }

    public string MessageText
    {
        get => _messageText;
        set => this.RaiseAndSetIfChanged(ref _messageText, value);
    }

    /// <summary>
    /// 这里用来记录显示矿机的数据
    /// </summary>
    public ObservableCollection<MinerDetail> MinerDetails { get; } = new ObservableCollection<MinerDetail>();

    /// <summary>
    ///  这里记录所有矿机的原始数据
    /// </summary>
    public List<MinerDetail> OriginMinerData = new List<MinerDetail>();
    public ObservableCollection<IssueFilter> Filters { get; } = new();
    public ICommand ButtonClickCommand { get; }
    public ICommand TestCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand LightMinerCommand { get; }
    public ICommand RebootMinerCommand { get; }
    private bool _showOK;

    public bool ShowOK
    {
        get => _showOK;
        set => this.RaiseAndSetIfChanged(ref _showOK, value);
    }

    private bool _showOverheat;

    public bool ShowOverheat
    {
        get => _showOverheat;
        set => this.RaiseAndSetIfChanged(ref _showOverheat, value);
    }

    private bool _showFanLost;

    public bool ShowFanLost
    {
        get => _showFanLost;
        set => this.RaiseAndSetIfChanged(ref _showFanLost, value);
    }

    private bool _showHashBroad;

    public bool ShowHashBroad
    {
        get => _showHashBroad;
        set => this.RaiseAndSetIfChanged(ref _showHashBroad, value);
    }

    private bool _showOffline;

    public bool ShowOffline
    {
        get => _showOffline;
        set => this.RaiseAndSetIfChanged(ref _showOffline, value);
    }

    private bool _showPowerLost;

    public bool ShowPowerLost
    {
        get => _showPowerLost;
        set => this.RaiseAndSetIfChanged(ref _showPowerLost, value);
    }

    private bool _showUnknown;

    public bool ShowUnknown
    {
        get => _showUnknown;
        set => this.RaiseAndSetIfChanged(ref _showUnknown, value);
    }

    private bool _showNetIssue;
    public bool ShowNetIssue
    {
        get => _showNetIssue;
        set => this.RaiseAndSetIfChanged(ref _showNetIssue, value);
    }


    private async void testUrl()
    {
        string log =
            await HttpHelper.GetDigestProtectedResourceAsync($"http://10.11.1.5/cgi-bin/log.cgi", "root", "root",
                "{}");

        var logResult = LogHelper.ParseLog(log);
        Console.WriteLine(logResult[0]);
    }

    public MainWindowViewModel()
    {
        ButtonClickCommand = ReactiveCommand.CreateFromTask<string>(OnButtonClicked);
        TestCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (FanIssue)
            {
                Console.WriteLine("FanIssue");
            }
            else if (TempIssue)
            {
                Console.WriteLine("TempIssue");
            }
        });
        ExportCommand = ReactiveCommand.Create(async () =>
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"miner_data_{timestamp}.csv";
                CsvExporter.ExportToCsv(MinerDetails.ToList(), fileName);
                Console.WriteLine("✅ 导出成功！");
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Caption", $"{fileName}文件已经成功导出",
                        ButtonEnum.YesNo);

                var result = await box.ShowAsync();
                OpenFolderAndSelectFile(fileName);
            }
            catch (Exception ex)
            {
                // 记录错误或通知用户
                Console.WriteLine("导出时发生异常: " + ex.Message);
            }
        });
        ///
        this.WhenAnyValue(x => x.ShowOK,
                x => x.ShowOverheat,
                x => x.ShowFanLost,
                x => x.ShowNetIssue,
                x => x.ShowOffline,
                x => x.ShowPowerLost, 
                x => x.ShowHashBroad)
            .Subscribe(_ => ApplyFilter());

        foreach (var filter in Filters)
        {
            filter.WhenAnyValue(f => f.IsChecked)
                .Subscribe(_ => ApplyFilter());
        }
        ///
        
    }

    private async Task OnButtonClicked(string buttonName)
    {
        MessageText = "开始扫描";
        var ips = IpRangeHelper.GetIpRanges(buttonName);
        OriginMinerData.Clear();
        MinerDetails.Clear();
        foreach (var ip in ips)
        {
            var detail = await MinerHelper.IsMinerAvailable(ip);
            OriginMinerData.Add(detail);
            MinerDetails.Add(detail);
        }

        UpdateIssueStatistics();
    }

    public static void OpenFolderAndSelectFile(string relativePath)
    {
        string fullPath = Path.GetFullPath(relativePath);

        if (OperatingSystem.IsWindows() && File.Exists(fullPath))
        {
            var argument = $"/select,\"{fullPath}\"";
            Process.Start("explorer.exe", argument);
        }
    }

    public void UpdateIssueStatistics()
    {
        var total = OriginMinerData.Count;
        var grouped = MinerDetails
            .GroupBy(m => m.IssueDetail)
            .Select(g => new { Issue = g.Key, Count = g.Count() })
            .ToList();

        IssueSummary = string.Join("\n", grouped.Select(g =>
            $"{g.Issue}: {g.Count} ({(g.Count * 100.0 / total):F1}%)"));
        MessageText = IssueSummary;
    }

    private void ApplyFilter()
    {
        MinerDetails.Clear();

        var selectedIssues = new List<string>();
        if (ShowOK) selectedIssues.Add(LogHelper.NormalOk);
        if (ShowOverheat) selectedIssues.Add(LogHelper.ErrorTempTooHigh);
        if (ShowFanLost) selectedIssues.Add(LogHelper.ErrorFanLost);
        if (ShowOffline) selectedIssues.Add(LogHelper.ErrorOffline);
        if (ShowNetIssue) selectedIssues.Add(LogHelper.ErrorNetIssue);
        if (ShowPowerLost) selectedIssues.Add(LogHelper.ErrorPowerLost);
        if (ShowUnknown) selectedIssues.Add(LogHelper.ErrorUnknown);
        if (ShowHashBroad) selectedIssues.Add(LogHelper.ErrorNotEnoughChain);
        var result = OriginMinerData
            .Where(m => selectedIssues.Contains(m.IssueDetail));

        foreach (var m in result)
            MinerDetails.Add(m);
    }
}
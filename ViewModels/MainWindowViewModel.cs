using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
    private bool _suppressFilterUpdates = false;

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
        // 使用方式
        
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

    private IEnumerable<IObservable<bool>> GetFilterProperties()
    {
        yield return this.WhenAnyValue(x => x.ShowOK);
        yield return this.WhenAnyValue(x => x.ShowOverheat);
        yield return this.WhenAnyValue(x => x.ShowFanLost);
        yield return this.WhenAnyValue(x => x.ShowNetIssue);
        yield return this.WhenAnyValue(x => x.ShowOffline);
        yield return this.WhenAnyValue(x => x.ShowPowerLost);
        yield return this.WhenAnyValue(x => x.ShowHashBroad);
        yield return this.WhenAnyValue(x => x.ShowUnknown);
    }


    private void ApplyFilter()
    {
        MinerDetails.Clear();

        var selected = Filters
            .Where(f => f.IsChecked)
            .Select(f => f.Issue)
            .ToHashSet();

        foreach (var miner in OriginMinerData.Where(m => selected.Contains(m.IssueDetail)))
        {
            MinerDetails.Add(miner);
        }
    }

    public void UpdateIssueStatistics()
    {
        _suppressFilterUpdates = true;

        var total = OriginMinerData.Count;
        var grouped = OriginMinerData
            .GroupBy(m => m.IssueDetail)
            .Select(g => new { Issue = g.Key, Count = g.Count() })
            .ToList();

        Filters.Clear();
        foreach (var group in grouped)
        {
            var filter = new IssueFilter
            {
                Issue = group.Issue,
                Label = $"{group.Issue}: {group.Count} 台 ({(group.Count * 100.0 / total):F1}%)",
                IsChecked = true // 默认全部勾选
            };

            // 监听 CheckBox 的变化来触发过滤
            filter.WhenAnyValue(f => f.IsChecked)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    if (!_suppressFilterUpdates) ApplyFilter();
                });

            Filters.Add(filter);
        }

        _suppressFilterUpdates = false;

        ApplyFilter();
    }
}
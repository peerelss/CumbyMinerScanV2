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
    public ObservableCollection<MinerDetail> MinerDetails { get; } = new ObservableCollection<MinerDetail>();
    public ICommand ButtonClickCommand { get; }
    public ICommand TestCommand { get; }
    public ICommand ExportCommand { get; }
    public MainWindowViewModel()
    {
        ButtonClickCommand = ReactiveCommand.CreateFromTask<string>(OnButtonClicked);
        TestCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            string log =
                await HttpHelper.GetDigestProtectedResourceAsync($"http://10.11.1.5/cgi-bin/log.cgi", "root", "root",
                    "{}");
          
            var logResult = LogHelper.ParseLog(log);
            Console.WriteLine(logResult[0]);
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
        
    }

    private async Task OnButtonClicked(string buttonName)
    {
        var ips = IpRangeHelper.GetIpRanges(buttonName);
        MinerDetails.Clear();
        foreach (var ip in ips)
        {
            var detail = await MinerHelper.IsMinerAvailable(ip);
            MinerDetails.Add(detail);
        }
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
}
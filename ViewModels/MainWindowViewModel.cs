using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CumbyMinerScan.Models;
using CumbyMinerScanV2.Utils;
using ReactiveUI; // Avalonia 官方 MVVM 推荐用 ReactiveUI
using System.Threading.Tasks;
using CumbyMinerScanV2.Models;

namespace CumbyMinerScanV2.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<MinerDetail> MinerDetails { get; } = new ObservableCollection<MinerDetail>();
    public ICommand ButtonClickCommand { get; }
    public ObservableCollection<Person> People { get; }

    public MainWindowViewModel()
    {
        ButtonClickCommand = ReactiveCommand.CreateFromTask<string>(OnButtonClicked);
        var people = new List<Person>
        {
            new Person("Neil", "Armstrong"),
            new Person("Buzz", "Lightyear"),
            new Person("James", "Kirk")
        };
        People = new ObservableCollection<Person>(people);
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
}
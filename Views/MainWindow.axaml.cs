using Avalonia.Controls;
using CumbyMinerScanV2.ViewModels;

namespace CumbyMinerScanV2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
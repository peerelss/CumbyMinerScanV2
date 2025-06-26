using ReactiveUI;
public class IssueFilter : ReactiveObject
{
    public string Issue { get; set; } // 实际值（如 "FanLost"）
    public string Label { get; set; } // 显示文本（如 "FanLost: 20台 (15%)"）

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }
}

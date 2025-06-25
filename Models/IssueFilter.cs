using ReactiveUI;

public class IssueFilter : ReactiveObject
{
    public string Name { get; set; }         // 显示给用户看的名称，例如 "OK"
    public string Code { get; set; }         // 实际用于过滤的标识，如 LogHelper.NormalOk

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }
}
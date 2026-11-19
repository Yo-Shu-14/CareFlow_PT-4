using CommunityToolkit.Mvvm.ComponentModel;

namespace CareFlow.Desktop.ViewModels;

public partial class ServiceSelectionViewModel : ViewModelBase
{
    public int DiagnosticServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }

    [ObservableProperty]
    private int _bookedCount;

    [ObservableProperty]
    private int _remainingCapacity;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private bool _isSelected;

    public string CapacityStatus =>
        RemainingCapacity > 0
            ? $"{RemainingCapacity} of {DailyCapacity} available"
            : "Fully Booked";
}

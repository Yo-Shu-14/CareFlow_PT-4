using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IBookingService _bookingService;

    public MainViewModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
        _selectedDateOffset = new DateTimeOffset(DateTime.Today);

        // Run initialization asynchronously
        _ = InitializeAsync();
    }

    // Parameterless constructor for XAML designer preview
    public MainViewModel()
    {
        _bookingService = null!;
        _selectedDateOffset = new DateTimeOffset(DateTime.Today);
    }

    [ObservableProperty]
    private ObservableCollection<PatientDto> _patients = new();

    [ObservableProperty]
    private PatientDto? _selectedPatient;

    [ObservableProperty]
    private DateTimeOffset? _selectedDateOffset;

    [ObservableProperty]
    private ObservableCollection<ServiceSelectionViewModel> _availableServices = new();

    [ObservableProperty]
    private ObservableCollection<BookingResultDto> _confirmedBookings = new();

    [ObservableProperty]
    private BookingResultDto? _selectedBooking;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private bool _isBusy;

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    partial void OnStatusMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasStatusMessage));
    }

    partial void OnSelectedDateOffsetChanged(DateTimeOffset? value)
    {
        _ = LoadAvailabilityForSelectedDateAsync();
    }

    public async Task InitializeAsync()
    {
        if (_bookingService is null) return;

        IsBusy = true;
        try
        {
            await LoadPatientsAsync();
            await LoadAvailabilityForSelectedDateAsync();
            await LoadConfirmedBookingsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RefreshAvailabilityAsync()
    {
        await LoadAvailabilityForSelectedDateAsync();
    }

    [RelayCommand]
    public async Task RefreshBookingsAsync()
    {
        await LoadConfirmedBookingsAsync();
    }

    [RelayCommand]
    public async Task ConfirmBookingAsync()
    {
        if (_bookingService is null) return;

        StatusMessage = null;

        if (SelectedPatient is null)
        {
            StatusMessage = "Please select an existing patient.";
            IsSuccess = false;
            return;
        }

        var bookingDate = (SelectedDateOffset?.DateTime.Date) ?? DateTime.Today;

        var selectedServiceIds = AvailableServices
            .Where(s => s.IsSelected)
            .Select(s => s.DiagnosticServiceId)
            .ToList();

        if (selectedServiceIds.Count < 1 || selectedServiceIds.Count > 3)
        {
            StatusMessage = "Please select between 1 and 3 diagnostic services.";
            IsSuccess = false;
            return;
        }

        var request = new BookingRequestDto
        {
            PatientId = SelectedPatient.PatientId,
            BookingDate = bookingDate,
            DiagnosticServiceIds = selectedServiceIds
        };

        IsBusy = true;
        try
        {
            var result = await _bookingService.CreateBookingAsync(request);

            IsSuccess = result.Success;
            StatusMessage = result.Message;

            if (result.Success)
            {
                // Refresh availability and bookings list
                await LoadAvailabilityForSelectedDateAsync();
                await LoadConfirmedBookingsAsync();

                // Select the newly created booking in the list
                if (result.BookingId.HasValue)
                {
                    SelectedBooking = ConfirmedBookings.FirstOrDefault(b => b.BookingId == result.BookingId);
                }

                // Reset service selections
                foreach (var service in AvailableServices)
                {
                    service.IsSelected = false;
                }
            }
            else
            {
                // Re-fetch availability to ensure accurate UI state
                await LoadAvailabilityForSelectedDateAsync();
            }
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            StatusMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPatientsAsync()
    {
        try
        {
            var patients = await _bookingService.GetPatientsAsync();
            Patients = new ObservableCollection<PatientDto>(patients);
            if (Patients.Any() && SelectedPatient is null)
            {
                SelectedPatient = Patients.First();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading patients: {ex.Message}";
            IsSuccess = false;
        }
    }

    private async Task LoadAvailabilityForSelectedDateAsync()
    {
        if (_bookingService is null) return;

        var date = (SelectedDateOffset?.DateTime.Date) ?? DateTime.Today;

        try
        {
            // Remember currently selected IDs so user doesn't lose their checkmarks if valid
            var previouslySelectedIds = AvailableServices
                .Where(s => s.IsSelected)
                .Select(s => s.DiagnosticServiceId)
                .ToHashSet();

            var availabilityList = await _bookingService.GetServiceAvailabilityAsync(date);

            var viewModels = availabilityList.Select(a => new ServiceSelectionViewModel
            {
                DiagnosticServiceId = a.DiagnosticServiceId,
                ServiceName = a.ServiceName,
                DailyCapacity = a.DailyCapacity,
                BookedCount = a.BookedCount,
                RemainingCapacity = a.RemainingCapacity,
                IsAvailable = a.IsAvailable,
                IsSelected = previouslySelectedIds.Contains(a.DiagnosticServiceId)
            }).ToList();

            AvailableServices = new ObservableCollection<ServiceSelectionViewModel>(viewModels);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error checking service availability: {ex.Message}";
            IsSuccess = false;
        }
    }

    private async Task LoadConfirmedBookingsAsync()
    {
        if (_bookingService is null) return;

        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            ConfirmedBookings = new ObservableCollection<BookingResultDto>(bookings);

            if (SelectedBooking is not null)
            {
                SelectedBooking = ConfirmedBookings.FirstOrDefault(b => b.BookingId == SelectedBooking.BookingId);
            }
            else if (ConfirmedBookings.Any())
            {
                SelectedBooking = ConfirmedBookings.First();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading bookings: {ex.Message}";
            IsSuccess = false;
        }
    }
}

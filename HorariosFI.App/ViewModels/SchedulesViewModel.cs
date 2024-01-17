using CommunityToolkit.Mvvm.ComponentModel;

namespace HorariosFI.App.ViewModels;

public partial class SchedulesViewModel : ObservableObject
{
    [ObservableProperty] private string _message = "Hello World!";
    public string? Input { get; set; }
}
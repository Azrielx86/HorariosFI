using CommunityToolkit.Mvvm.ComponentModel;
using HorariosFI.Core;

namespace HorariosFI.App.ViewModels;

public partial class SchedulesViewModel(SchedulesDb schedulesDb) : ObservableObject
{
    [ObservableProperty] private string _message = "Hello World!";
    public string? Input { get; set; }
}
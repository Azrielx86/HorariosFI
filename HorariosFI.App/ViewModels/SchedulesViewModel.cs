using System.Linq;
using System.Threading.Tasks;
using Avalonia.SimpleRouter;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorariosFI.Core;

namespace HorariosFI.App.ViewModels;

public partial class SchedulesViewModel(HistoryRouter<ViewModelBase> router, SchedulesDb schedulesDb) : ViewModelBase
{
    public int ClassCode { get; set; }

    public override void ReadyToShow()
    {
        var fiClass = schedulesDb.FiClasses.FirstOrDefault(c => c.Code == ClassCode);
        if (fiClass is null)
        {
            router.Back();
            return;
        }

        Message = $"Clase: {fiClass!.Name} - {ClassCode}";
        base.ReadyToShow();
    }

    [ObservableProperty] private string _message = "Materia - Código";

    [RelayCommand]
    private async Task GetScores()
    {
    }

    [RelayCommand]
    private void Return() => router.Back();
}
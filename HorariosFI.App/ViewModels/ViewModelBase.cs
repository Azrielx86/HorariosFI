using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace HorariosFI.App.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected bool IsReadyToShow { get; set; } = false;

    public virtual Task ReadyToShowAsync()
    {
        IsReadyToShow = true;
        return Task.CompletedTask;
    }
}
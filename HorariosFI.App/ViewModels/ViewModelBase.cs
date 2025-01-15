using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace HorariosFI.App.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected bool IsReadyToShow { get; set; } = false;

    public virtual void ReadyToShow()
    {
        IsReadyToShow = true;
    }
}
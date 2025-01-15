using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HorariosFI.App.ViewModels;

namespace HorariosFI.App;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock() { Text = "Data is null" };
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock() { Text = $"Not found: {name}" };
        }
    }

    public bool Match(object? data) => data is ViewModelBase;
}
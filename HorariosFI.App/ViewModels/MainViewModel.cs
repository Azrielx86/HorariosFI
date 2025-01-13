using System;
using HorariosFI.App.Models;
using HorariosFI.Core;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace HorariosFI.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SchedulesDb _schedulesDb;
    [ObservableProperty] private string _clave = "";
    [ObservableProperty] private ObservableCollection<InputClassModel> _classCollection = [];
    [ObservableProperty] private bool _progreso;
    [ObservableProperty] private int _progresoScrapMp;
    [ObservableProperty] private bool _openSeleniumWindow;

    public MainViewModel(SchedulesDb schedulesDb)
    {
        _schedulesDb = schedulesDb;
        schedulesDb.Database.EnsureCreated();
        
#if DEBUG
        ClassCollection.Add(new InputClassModel
        {
            Clave = 119,
            Nombre = "Estructuras Discretas"
        });
#endif
    }

    [RelayCommand]
    private async Task AddClass()
    {
        if (string.IsNullOrEmpty(Clave)) return;
        if (!int.TryParse(Clave, out var c)) return;

        var className = await FiScrapper.GetClassName(c);
        if (className is null)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Error", "La clase no existe")
                .ShowAsync();
            Clave = "";
            return;
        }

        var data = new InputClassModel
        {
            Clave = c,
            Nombre = className
        };

        ClassCollection.Add(data);

        Clave = "";
    }

    [RelayCommand]
    private async Task ViewSchedules(int clave)
    {
        try
        {
            Progreso = true;
            var item = ClassCollection.FirstOrDefault(c => c.Clave == clave);
            if (item is null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", $"Clave: {clave} no encontrada en las clases agregadas.")
                    .ShowAsync();
                return;
            }

            item.Horarios ??= await FiScrapper.GetClassShcedules(item.Clave);
            
            await MpScrapper.Run(item.Horarios!, new Progress<int>(p => ProgresoScrapMp = p), OpenSeleniumWindow);
            
            // TODO : Add custom filename option
            var exporter = new SpreadSheetExporter();
            
            exporter.Export(item.Clave, item.Nombre ?? "Unknown", item.Horarios);
            ClassCollection.Remove(item);
        }
        catch (Exception e)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Error", e.ToString())
                .ShowAsync();
        }
        finally
        {
            ProgresoScrapMp = 0;
            Progreso = false;
        }
    }
}
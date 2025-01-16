using System;
using HorariosFI.App.Models;
using HorariosFI.Core;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.SimpleRouter;
using DynamicData;
using HorariosFI.Core.Models;
using Microsoft.EntityFrameworkCore;


namespace HorariosFI.App.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly HistoryRouter<ViewModelBase> _router;
    private readonly SchedulesDb _schedulesDb;
    [ObservableProperty] private string _clave = "";
    [ObservableProperty] private ObservableCollection<InputClassModel> _classCollection = [];
    [ObservableProperty] private bool _progreso;
    [ObservableProperty] private int _progresoScrapMp;
    [ObservableProperty] private bool _openSeleniumWindow;

    public HomeViewModel(HistoryRouter<ViewModelBase> router, SchedulesDb schedulesDb)
    {
        _router = router;
        _schedulesDb = schedulesDb;
        schedulesDb.Database.EnsureCreated();

        ClassCollection.AddRange(schedulesDb.FiClasses.Select(c => new InputClassModel { Clave = c.Code, Nombre = c.Name }));
    }

    [RelayCommand]
    private async Task AddClass()
    {
        if (string.IsNullOrEmpty(Clave)) return;
        if (!int.TryParse(Clave, out var c)) return;

        var fiClass = await _schedulesDb.FiClasses.FirstOrDefaultAsync(fiClass => fiClass.Code == c);

        if (fiClass is null)
        {
            var className = await FiScrapper.GetClassName(c);
            if (className is null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", "La clase no existe")
                    .ShowAsync();
                Clave = "";
                return;
            }

            fiClass = new FiClass { Code = c, Name = className };
            _schedulesDb.FiClasses.Add(fiClass);
            await _schedulesDb.SaveChangesAsync();
        }

        var data = new InputClassModel
        {
            Clave = fiClass.Code,
            Nombre = fiClass.Name
        };

        ClassCollection.Add(data);

        Clave = "";
    }

    [RelayCommand]
    private async Task ViewSchedules(int clave)
    {
        var schedulesViewModel = _router.GoTo<SchedulesViewModel>();
        schedulesViewModel.ClassCode = clave;
        await schedulesViewModel.ReadyToShowAsync();

//         try
//         {
//             Progreso = true;
//             var item = ClassCollection.FirstOrDefault(c => c.Clave == clave);
//             if (item is null)
//             {
//                 await MessageBoxManager
//                     .GetMessageBoxStandard("Error", $"Clave: {clave} no encontrada en las clases agregadas.")
//                     .ShowAsync();
//                 return;
//             }
//
//             // item.Horarios ??= await FiScrapper.GetClassShcedules(item.Clave);
//             await FiScrapper.GetSchedules(_schedulesDb, item.Clave);
//
// #if ENABLE_AUTOMPSCRAP
//             await MpScrapper.Run(item.Horarios!, new Progress<int>(p => ProgresoScrapMp = p), OpenSeleniumWindow);
//
//             
//             // TODO : Add custom filename option
//             var exporter = new SpreadSheetExporter();
//             
//             exporter.Export(item.Clave, item.Nombre ?? "Unknown", item.Horarios);
//             ClassCollection.Remove(item);
// #endif
//         }
//         catch (Exception e)
//         {
//             await MessageBoxManager
//                 .GetMessageBoxStandard("Error", e.ToString())
//                 .ShowAsync();
//         }
//         finally
//         {
//             ProgresoScrapMp = 0;
//             Progreso = false;
//         }
    }
}
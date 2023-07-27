using DocumentFormat.OpenXml.Office.CustomUI;
using HorariosFI.App.Models;
using HorariosFI.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace HorariosFI.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Properties

    private string? _clave;
    private string? _nombreClase;
    private bool _progreso;
    public ObservableCollection<InputClassModel> ClassItems { get; set; }
    public string? Clave { get => _clave; set => this.RaiseAndSetIfChanged(ref _clave, value); }
    public string? NombreClase { get => _nombreClase; set => this.RaiseAndSetIfChanged(ref _nombreClase, value); }
    public bool Progreso { get => _progreso; set => this.RaiseAndSetIfChanged(ref _progreso, value); }

    #endregion Properties

    #region Commands

    public ReactiveCommand<Unit, Unit> AddClass { get; }
    public ReactiveCommand<Unit, Unit> Buscar { get; }

    #endregion Commands

    public MainViewModel()
    {
        Progreso = false;
        ClassItems = new ObservableCollection<InputClassModel>();
        AddClass = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrEmpty(Clave) || string.IsNullOrEmpty(NombreClase)) return;
            if (!int.TryParse(_clave, out int clave)) return;

            ClassItems.Add(new InputClassModel()
            {
                Clave = clave,
                Nombre = _nombreClase
            });

            Clave = "";
            NombreClase = "";
        });

        Buscar = ReactiveCommand.CreateFromTask(async () =>
        {
            foreach (var item in ClassItems)
            {
                var (clase, _) = await FIScrapper.GetClassList(item.Clave);
                var classdict = new Dictionary<string, IEnumerable<ClassModel>>() { { $"{item.Clave}-{item.Nombre}", clase } };

                var mp = new MPScrapper();
                foreach (var cl in classdict)
                {
                    Progreso = true;
                    await mp.Run(cl.Value);
                    Progreso = false;
                }
                try
                {
                    ExcelExport.Export(classdict);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                }
            }
            ClassItems.Clear();
        });
    }
}
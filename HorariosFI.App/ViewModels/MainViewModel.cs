using HorariosFI.App.Models;
using HorariosFI.Core;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using HorariosFI.App.Views;
using ReactiveUI;

namespace HorariosFI.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _clave = "";
    [ObservableProperty] private ObservableCollection<InputClassModel> _classCollection = [];
    [ObservableProperty] private bool _progreso = false;
    public Interaction<SchedulesViewModel, string?> SchedulesDialog { get; }

    public MainViewModel()
    {
#if DEBUG
        ClassCollection.Add(new InputClassModel
        {
            Clave = 119,
            Nombre = "Estructuras Discretas"
        });
#endif

        SchedulesDialog = new Interaction<SchedulesViewModel, string?>();
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

        await MPScrapper.Run(item.Horarios!);
        ExcelExport.Export(item.Clave, item.Nombre ?? "Unknown", item.Horarios);
        ClassCollection.Remove(item);
        Progreso = false;
    }

    [RelayCommand]
    private async Task ViewMpInfo(int clave)
    {
    }

    private static async void GetClassSchedules(InputClassModel item)
    {
        if (item.Horarios is not null) return;
        item.Horarios = await FiScrapper.GetClassShcedules(item.Clave);
    }

#if OLD_VER
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
                try
                {
                    var (clase, errores) = await FIScrapper.GetClassList(item.Clave);
                    var classdict =
 new Dictionary<string, IEnumerable<ClassModel>>() { { $"{item.Clave}-{item.Nombre}", clase } };

                    var mp = new MPScrapper();
                    foreach (var cl in classdict)
                    {
                        Progreso = true;
                        await mp.Run(cl.Value);
                        Progreso = false;
                    }

                    if (errores.Any())
                        throw new Exception(string.Join("\n", errores));

                    ExcelExport.Export(classdict);
                }
                catch (Exception e)
                {
                    Progreso = false;
                    await MessageBoxManager
                          .GetMessageBoxStandard("Error", e.ToString(), ButtonEnum.Ok)
                          .ShowAsync();
                }
            }
            ClassItems.Clear();
        });
    }
#endif
}
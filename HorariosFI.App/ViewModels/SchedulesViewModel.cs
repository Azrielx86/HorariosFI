using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.SimpleRouter;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HorariosFI.Core;
using HorariosFI.Core.Models;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;

namespace HorariosFI.App.ViewModels;

public partial class SchedulesViewModel(HistoryRouter<ViewModelBase> router, SchedulesDb schedulesDb) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<FiClassModel> _schedules = [];
    [ObservableProperty] private bool _progreso;
    [ObservableProperty] private int _progresoScrapMp;
    [ObservableProperty] private bool _openSeleniumWindow;

    public int ClassCode { get; set; }
    private string _className = string.Empty;

    public override async Task ReadyToShowAsync()
    {
        var fiClass = schedulesDb.FiClasses.FirstOrDefault(c => c.Code == ClassCode);
        if (fiClass is null)
        {
            router.Back();
            return;
        }

        Message = $"Clase: {fiClass.Name} - {ClassCode}";
        _className = fiClass.Name;

        var groupsCount = schedulesDb.FiGroups
            .Count(g => g.FiClassId == ClassCode);

        if (groupsCount == 0)
        {
            groupsCount = await FiScrapper.GetSchedules(schedulesDb, ClassCode);
            if (groupsCount == 0)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Error", "No hay grupos disponibles")
                    .ShowAsync();
                router.Back();
                return;
            }
        }

        var schedules = await GetSchedules();

        Schedules.AddRange(schedules);

        await base.ReadyToShowAsync();
    }

    private async Task<FiClassModel[]> GetSchedules() => await (from fiGroup in schedulesDb.FiGroups
            join teacher in schedulesDb.FiTeachers on fiGroup.FiTeacherId equals teacher.Id
            where fiGroup.FiClassId == ClassCode
            orderby fiGroup.Group
            select new FiClassModel
            {
                TeacherId = teacher.Id,
                Code = ClassCode,
                Classroom = fiGroup.Classroom,
                Days = fiGroup.Days,
                Difficult = teacher.Difficult,
                Grade = teacher.Grade,
                Group = fiGroup.Group,
                Recommend = teacher.Recommend,
                Quota = fiGroup.Quota,
                Schedules = fiGroup.Schedules,
                Teacher = teacher.Name,
                Vacancies = fiGroup.Vacancies,
                MisProfesoresUrl = teacher.MisProfesoresUrl,
                Type = fiGroup.Type
            }
        ).ToArrayAsync();

    [ObservableProperty] private string _message = "Materia - Código";

    [RelayCommand]
    private async Task GetScores()
    {
        Progreso = true;
        await MpScrapper.Run(Schedules, new Progress<int>(p => ProgresoScrapMp = p), OpenSeleniumWindow);
        foreach (var fiClass in Schedules)
        {
            var teacher = await schedulesDb.FiTeachers.Where(t => t.Id == fiClass.TeacherId).FirstOrDefaultAsync();
            if (teacher is null) continue;
            teacher.Recommend = fiClass.Recommend;
            teacher.Grade = fiClass.Grade;
            teacher.Difficult = fiClass.Difficult;
            teacher.MisProfesoresUrl = fiClass.MisProfesoresUrl;
        }

        await schedulesDb.SaveChangesAsync();
        Schedules.Clear();
        Schedules.AddRange(await GetSchedules());
        Progreso = false;
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        SpreadSheetExporter exporter = new();
        exporter.Export(ClassCode, _className, Schedules.ToList());
        await MessageBoxManager
            .GetMessageBoxStandard("Archivo exportado", $"El archivo ha sido exportado con el nombre: {exporter.Filename}")
            .ShowAsync();
    }

    [RelayCommand]
    private static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", url);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", url);
    }

    [RelayCommand]
    private void Return() => router.Back();
}
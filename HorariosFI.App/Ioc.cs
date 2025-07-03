using Avalonia.SimpleRouter;
using HorariosFI.App.ViewModels;
using HorariosFI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace HorariosFI.App;

public static class Ioc
{
    public static ServiceProvider AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<HistoryRouter<ViewModelBase>>(s => new HistoryRouter<ViewModelBase>(t => (ViewModelBase)s.GetRequiredService(t)));
        services.AddDbContext<SchedulesDb>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SchedulesViewModel>();
        services.AddTransient<SchedulePlannerViewModel>();
        return services.BuildServiceProvider();
    }
}
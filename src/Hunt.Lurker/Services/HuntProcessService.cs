using System.Windows;
using ProcessLurker;

namespace Hunt.Lurker.Services;

internal class HuntProcessService : ProcessService
{
    public HuntProcessService() 
        : base("HuntGame")
    {
    }

    protected override void OnExit()
        => Caliburn.Micro.Execute.OnUIThread(Application.Current.Shutdown);
}

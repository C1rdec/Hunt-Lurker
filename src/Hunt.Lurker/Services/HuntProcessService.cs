using ProcessLurker;

namespace Hunt.Lurker.Services;

internal class HuntProcessService : ProcessService
{
    public HuntProcessService() 
        : base("HuntGame")
    {
    }
}

using System.IO;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using Hunt.Lurker.Services;
using Lurker.Steam.Services;

namespace Hunt.Lurker.ViewModels;

internal class ShellViewModel : Screen, IViewAware
{
    private string _matchMakingRating;
    private string _playerName;
    private string _attributeFilePath;
    private HuntProcessService _processService;

    public ShellViewModel()
    {
        _processService = new HuntProcessService();
        _processService.ProcessClosed += ProcessService_ProcessClosed;
    }

    public string MatchMakingRating
    {
        get => _matchMakingRating;
        set
        {
            _matchMakingRating = value;
            NotifyOfPropertyChange();
        }
    }

    protected override async void OnViewLoaded(object view)
    {
        var taskbarHeight = 20;
        var window = view as Window;
        window.Top = SystemParameters.WorkArea.Height + taskbarHeight;

        var steamService = new SteamService();
        await steamService.InitializeAsync();
        var games = steamService.FindGames();
        var huntGame = games.FirstOrDefault(g => g.Id == "594650");

        // Hunt Showdown is not installed
        if (huntGame == null)
        {
            Application.Current.Shutdown();

            return;
        }

        await huntGame.Open();      

        _attributeFilePath = Path.Combine(Path.GetDirectoryName(huntGame.ExeFilePath), "user", "profiles", "default", "attributes.xml");
        _playerName = steamService.FindUsername();

        _ = WatchMatchMakingRating();
        _ = _processService.WaitForProcess(true, false, 66666);
        base.OnViewLoaded(view);
    }

    private async Task WatchMatchMakingRating()
    {
        while (true)
        {
            try
            {
                var content = File.ReadAllText(_attributeFilePath);

                var document = XDocument.Parse(content);
                var attributes = document.Descendants("Attr").ToArray();
                var elements = attributes.Where(e => e.Attribute("value").Value.Contains(_playerName));

                var playerElement = elements.OrderBy(e => int.Parse(e.Attribute("name").Value.Split("_")[1])).FirstOrDefault();
                var bagplayerTag = playerElement.Attribute("name").Value.Replace("_blood_line_name", "");
                var mmrAttribute = attributes.FirstOrDefault(a => a.Attribute("name").Value == $"{bagplayerTag}_mmr");

                Execute.OnUIThread(() =>
                {
                    MatchMakingRating = mmrAttribute.Attribute("value").Value;
                });

                await Task.Delay(3000);
            }
            catch(Exception)
            {
            }
        }
    }

    private void ProcessService_ProcessClosed(object sender, EventArgs e)
    {
        Execute.OnUIThread(Application.Current.Shutdown);
    }
}

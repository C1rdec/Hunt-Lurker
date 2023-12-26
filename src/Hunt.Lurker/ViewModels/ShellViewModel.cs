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

        var processLurker = new HuntProcessService();

        // Hunt Showdown is not started (ShutDown)
        if (await processLurker.WaitForProcess(true, 30000) == -1)
        {
            Application.Current.Shutdown();

            return;
        }

        var steamService = new SteamService();
        await steamService.InitializeAsync();

        var games = steamService.FindGames();
        var huntGame = games.FirstOrDefault(g => g.Id == "594650");

        _attributeFilePath = Path.Combine(Path.GetDirectoryName(huntGame.ExeFilePath), "user", "profiles", "default", "attributes.xml");
        _playerName = steamService.FindUsername();

        _ = WatchMatchMakingRating();
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
                var element = attributes.Where(e => e.Attribute("value").Value.Contains(_playerName)).FirstOrDefault();
                var bagplayerTag = element.Attribute("name").Value.Replace("_blood_line_name", "");
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
}

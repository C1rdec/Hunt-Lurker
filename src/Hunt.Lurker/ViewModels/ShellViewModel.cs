using System.IO;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using Lurker.Steam.Services;
using ProcessLurker;

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
        var processLurker = new ProcessService("HuntGame");

        await processLurker.WaitForProcess();
        var steamService = new SteamService();
        await steamService.InitializeAsync();

        var games = steamService.FindGames();

        var huntGame = games.FirstOrDefault(g => g.Id == "594650");

        _attributeFilePath = Path.Combine(Path.GetDirectoryName(huntGame.ExeFilePath), "user", "profiles", "default", "attributes.xml");
        _playerName = steamService.FindUsername();

        var window = view as Window;

        var taskbarHeight = 20;
        window.Top = SystemParameters.WorkArea.Height + taskbarHeight;

        _ = WatchMatchMakingRating();
        base.OnViewLoaded(view);
    }

    private async Task WatchMatchMakingRating()
    {
        while (true)
        {
            GetMatchMakingRating();
            await Task.Delay(3000);
        }
    }

    private void GetMatchMakingRating()
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
        }
        catch (Exception)
        {
        }
    }
}

namespace PluginManager;
using System.Text.Json;
using System.IO;

public class PluginStateManager
{
      private readonly string _configFilePath;
      private Dictionary<string, PluginState> _pluginsState;
      private readonly IFileSystem _fileSystem; 
      private readonly INotificationSystem _notifier; 
      
      public PluginStateManager(string configFileName, IFileSystem fileSystem, INotificationSystem notifier)
      {
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
            _fileSystem = fileSystem;
            _notifier = notifier;
            _pluginsState = new Dictionary<string, PluginState>();
      }


      public Dictionary<string, PluginState> LoadState()
      {
            if (_fileSystem.FileExists(_configFilePath))
            {
                  try
                  {
                        string jsonText = _fileSystem.ReadAllText(_configFilePath);
                        var list = JsonSerializer.Deserialize<List<PluginState>>(jsonText);
                        _pluginsState = list?.ToDictionary(p => Path.GetFileNameWithoutExtension(p.name),
                              p => p) 
                                        ??  new Dictionary<string, PluginState>();
                  }
                  catch (Exception e)
                  {
                        _notifier.Message(e.Message);
                        _pluginsState = new Dictionary<string, PluginState>();
                  }
            }
            else
            {
                  _notifier.Message($"INFO: Configuration file not found '{_configFilePath}'. Creating new one.");
                  _pluginsState = new Dictionary<string, PluginState>();
            }
            return _pluginsState;
      }

      public void SaveState()
      {
            try
            {
                  var listToSerialize = _pluginsState.Values.ToList();
                  JsonSerializerOptions options = new JsonSerializerOptions {WriteIndented = true};
                  string jsonText =  JsonSerializer.Serialize(listToSerialize, options);
                  
                  _fileSystem.WriteAllText(_configFilePath, jsonText);
                  _notifier.Message("Plugins state saved");
            }
            catch (Exception ex)
            {
                  _notifier.Message($"Error saving state to {_configFilePath}");
            }
      }
}
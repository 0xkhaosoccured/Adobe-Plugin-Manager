namespace PluginManager;

public sealed class PluginRegistry
{
      private readonly List<Plugin> _plugins;
      public int Count => _plugins.Count;

      public PluginRegistry()
      {
            _plugins = new List<Plugin>();
      }

      public void AddPlugin(Plugin plugin)
      {
            if (plugin == null)
            {
                  throw new ArgumentNullException(nameof(plugin));
            }
            if (!_plugins.Any(p => p.Path.Equals(plugin.Path, StringComparison.OrdinalIgnoreCase)))
            {
                  _plugins.Add(plugin);
            }
      }

      public void RemovePlugin(Plugin plugin)
      {
            if (plugin == null)
            {
                  throw new ArgumentNullException(nameof(plugin));
            }
            _plugins.RemoveAll(p => p.Path.Equals(plugin.Path, StringComparison.OrdinalIgnoreCase));
      }

      public IReadOnlyCollection<Plugin> GetPlugins()
      {
            return _plugins.AsReadOnly();
      }

      public void Clear()
      {
            _plugins.Clear();
      }
}
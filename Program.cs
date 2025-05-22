using PluginManager;
using System;
using System.Collections.Generic;
using System.IO;

public class Program
{
    static void Main(string[] args)
    {
        IFileSystem fileSystem = new DefaultFileSystem();
        INotificationSystem notifier = new NotificationSystem();
        AeFinderOptions options = new AeFinderOptions();

        PluginStateManager pluginStateManager = new PluginStateManager("plugins_state.json", fileSystem, notifier);

        Dictionary<string, PluginState> currentPluginsState = pluginStateManager.LoadState();

        FolderFinder folderFinder = new FolderFinder(fileSystem, options, notifier);
        List<Plugin> discoveredPlugins = folderFinder.FindAllPlugins();

        foreach (var discoveredPlugin in discoveredPlugins)
        {
            if (!currentPluginsState.ContainsKey(discoveredPlugin.Name))
            {
                currentPluginsState.Add(discoveredPlugin.Name, new PluginState(
                    discoveredPlugin.Name,
                    Path.GetExtension(discoveredPlugin.Path),
                    false,
                    discoveredPlugin.Path
                ));
                notifier.Message($"New plugin discovered and added to state: {discoveredPlugin.Name}");
            }
            else
            {
                if (currentPluginsState[discoveredPlugin.Name].isRemoved)
                {
                    if (!discoveredPlugin.Path.EndsWith(".removed", StringComparison.OrdinalIgnoreCase))
                    {
                        notifier.Message($"Warning: Plugin '{discoveredPlugin.Name}' is marked as disabled, but the file '{discoveredPlugin.Path}' does not have a '.removed' extension.");
                    }
                }
                else
                {
                     if (discoveredPlugin.Path.EndsWith(".removed", StringComparison.OrdinalIgnoreCase))
                     {
                         notifier.Message($"Warning: Plugin '{discoveredPlugin.Name}' is marked as enabled, but the file '{discoveredPlugin.Path}' has a '.removed' extension.");
                     }
                }
                currentPluginsState[discoveredPlugin.Name]._path = discoveredPlugin.Path;
            }
        }
        
        pluginStateManager.SaveState();

        Console.WriteLine("Program execution completed.");
        Console.ReadLine();
    }
}
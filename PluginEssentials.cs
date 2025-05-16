using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PluginManager
{

    public interface INotificationSystem
    {
        void Message(string message);
    }

    public class NotificationSystem : INotificationSystem
    {
        public void Message(string message) => Console.WriteLine(message);
    }
    
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        string[] GetDirectories(string path);
        string[] GetDirectories(string path, string searchPattern);
        string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
        string GetFileName(string path);
        string Combine(string path1, string path2);
        void RemoveFile(string path);
        void OpenDirectory(string path);
    }

    public class DefaultFileSystem : IFileSystem
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public string[] GetDirectories(string path) => Directory.GetDirectories(path);
        public string[] GetDirectories(string path, string searchPattern) => Directory.GetDirectories(path, searchPattern);
        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
        public string GetFileName(string path) => Path.GetFileName(path);
        public string Combine(string path1, string path2) => Path.Combine(path1, path2);
        public void RemoveFile(string path) => File.Delete(path);
        public void OpenDirectory(string path) => Process.Start("explorer.exe", path);
    }

    public class AeFinderOptions
    {
        public string ProgramFilesAdobeRoot { get; set; } = "C:\\Program Files\\Adobe";
        public string AeDirSearchPattern { get; set; } = "Adobe After Effects*";
        public string AeVersionRegexPattern { get; set; } = @"^Adobe After Effects \d{4}$";
        public string CommonPluginsRelativePath { get; set; } = Path.Combine("C:\\Program Files\\Adobe", "Common", "Plug-ins", "7.0", "MediaCore");
        public string FfxFilePattern { get; set; } = "*.ffx";
    }

    sealed class PluginRegistry
    {
        private readonly List<Plugin> _plugins;
        public int Count => _plugins.Count;

        PluginRegistry(Plugin plugin)
        {
            _plugins = new List<Plugin>();
        }

        public void AddPlugin(Plugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }
            _plugins.Add(plugin);
        }

        public void RemovePlugin(Plugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }
            _plugins.Remove(plugin);
        }

        public IReadOnlyCollection<Plugin> GetPlugins()
        {
            return _plugins.AsReadOnly();
        }
    }
    
    sealed class Plugin
    {
        public string Name { get; }
        public string Description { get; }
        public string Path { get; }
        public string ImagePath { get; }

        public Plugin(string name, string path, string description, string imagePath)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ImagePath = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }

    namespace AeFolders
    {
        public class FolderFinder
        {
            private readonly IFileSystem _fileSystem;
            private readonly AeFinderOptions _options;

            public FolderFinder(IFileSystem fileSystem, AeFinderOptions options)
            {
                _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
                _options = options ?? throw new ArgumentNullException(nameof(options));
            }

            // Собирает в список все существующие директории с плагинами
            public List<AeFolderItem> Find()
            {
                List<AeFolderItem> foundFolders = [];
                string commonPluginsPath = _fileSystem.Combine(_options.ProgramFilesAdobeRoot, _options.CommonPluginsRelativePath);

                if (_fileSystem.DirectoryExists(_options.ProgramFilesAdobeRoot))
                {
                    if (_fileSystem.DirectoryExists(commonPluginsPath))
                    {
                        foundFolders.Add(new AeFolderItem(commonPluginsPath));
                    }

                    try
                    {
                        string[] aeDirs = _fileSystem.GetDirectories(_options.ProgramFilesAdobeRoot, _options.AeDirSearchPattern);
                        foreach (string aeDir in aeDirs)
                        {
                            string dirName = _fileSystem.GetFileName(aeDir);
                            if (Regex.IsMatch(dirName, _options.AeVersionRegexPattern))
                            {
                                foundFolders.Add(new AeFolderItem(aeDir));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                return foundFolders;
            }

            // Собирает в список все .ffx файлы
            public List<AeFileItem> CollectItems()
            {
                List<AeFileItem> items = [];
                List<AeFolderItem> foundFolders = Find();

                if (foundFolders.Any())
                {
                    foreach (var folder in foundFolders)
                    {
                        if (_fileSystem.DirectoryExists(folder.Path))
                        {
                            try
                            {
                                string[] rawFoundAeFiles = _fileSystem.GetFiles(folder.Path, _options.FfxFilePattern, SearchOption.AllDirectories);
                                List<AeFileItem> finalAeFileItems = rawFoundAeFiles
                                    .Select(filePath => new AeFileItem(filePath))
                                    .ToList();
                                items.AddRange(finalAeFileItems);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }

                return items;
            }

            // Собирает в словарь все категории с файлами в них
            public Dictionary<string, List<AeFileItem>> Categorize()
            {
                Dictionary<string, List<AeFileItem>> categories = new Dictionary<string, List<AeFileItem>>();
                List<AeFolderItem> foundFolders = Find();

                if (foundFolders.Any())
                {
                    foreach (var folder in foundFolders)
                    {
                        if (_fileSystem.DirectoryExists(folder.Path))
                        {
                            try
                            {
                                string[] categoriesDirs = _fileSystem.GetDirectories(folder.Path);

                                foreach (string categoryDir in categoriesDirs)
                                {
                                    string categoryName = _fileSystem.GetFileName(categoryDir);
                                    List<AeFileItem> categoryFiles = new List<AeFileItem>();

                                    try
                                    {
                                        string[] files = _fileSystem.GetFiles(categoryDir, _options.FfxFilePattern, SearchOption.TopDirectoryOnly);

                                        foreach (string file in files)
                                        {
                                            categoryFiles.Add(new AeFileItem(file));
                                        }

                                        if (categoryFiles.Any())
                                        {
                                            if (categories.ContainsKey(categoryName))
                                            {
                                                categories[categoryName].AddRange(categoryFiles);
                                            }
                                            else
                                            {
                                                categories.Add(categoryName, categoryFiles);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                }

                                try
                                {
                                     string[] rootFiles = _fileSystem.GetFiles(folder.Path, _options.FfxFilePattern, SearchOption.TopDirectoryOnly);
                                     if (rootFiles.Any())
                                     {
                                         string rootCategoryName = "Uncategorized";
                                         List<AeFileItem> rootCategoryFiles = rootFiles
                                            .Select(filePath => new AeFileItem(filePath))
                                            .ToList();

                                         if (categories.ContainsKey(rootCategoryName))
                                         {
                                             categories[rootCategoryName].AddRange(rootCategoryFiles);
                                         }
                                         else
                                         {
                                             categories.Add(rootCategoryName, rootCategoryFiles);
                                         }
                                     }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }

                return categories;
            }

            public class AeFolderItem(string path)
            {
                public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));
            }

            public class AeFileItem(string path)
            {
                public string Name { get; } = System.IO.Path.GetFileName(path);
                public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));
            }
        }
    }
}
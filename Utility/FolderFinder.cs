using System.Text.RegularExpressions;
namespace PluginManager;

public class FolderFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly AeFinderOptions _options;
    private readonly INotificationSystem _notifier;

    public FolderFinder(IFileSystem fileSystem, AeFinderOptions options, INotificationSystem notifier)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public FolderFinder() : this(new DefaultFileSystem(), new AeFinderOptions(), new NotificationSystem())
    {
    }


    public List<string> FindPluginRootFolders()
    {
        _notifier.Message($"Program Files Adobe Root: {_options.ProgramFilesAdobeRoot}");
        _notifier.Message($"Common Plugins Path to check: {_options.CommonPluginsRelativePath}");
        _notifier.Message($"Ae Dir Search Pattern: {_options.AeDirSearchPattern}");
        _notifier.Message($"Ae Version Regex Pattern: {_options.AeVersionRegexPattern}");

        List<string> foundFolders = new List<string>();
        string commonPluginsPath = _fileSystem.Combine(_options.ProgramFilesAdobeRoot, _options.CommonPluginsRelativePath);

        if (_fileSystem.DirectoryExists(_options.ProgramFilesAdobeRoot))
        {
            if (_fileSystem.DirectoryExists(commonPluginsPath))
            {
                foundFolders.Add(commonPluginsPath);
            }

            try
            {
                string[] aeDirs = _fileSystem.GetDirectories(_options.ProgramFilesAdobeRoot, _options.AeDirSearchPattern);

                _notifier.Message($"Found directories matching pattern '{_options.AeDirSearchPattern}' in '{_options.ProgramFilesAdobeRoot}':");
                if (aeDirs.Length == 0)
                {
                     _notifier.Message("No directory found");
                }
                else
                {
                    foreach (string rawAeDir in aeDirs)
                    {
                        _notifier.Message($" Raw result {rawAeDir}");
                    }
                }

                foreach (string aeDir in aeDirs)
                {
                    string dirName = _fileSystem.GetFileName(aeDir);
                    if (Regex.IsMatch(dirName, _options.AeVersionRegexPattern))
                    {
                        string aeVersionPluginsPath = _fileSystem.Combine(aeDir, "Support Files", "Plug-ins");
                        if (_fileSystem.DirectoryExists(aeVersionPluginsPath))
                        {
                             foundFolders.Add(aeVersionPluginsPath);
                        }
                        else
                        {
                             _notifier.Message($"AE version plugin folder not found: {aeVersionPluginsPath}");
                        }

                    }
                     else
                     {
                         _notifier.Message($"Directory name '{dirName}' does not match regex '{_options.AeVersionRegexPattern}'");
                     }
                }
            }
            catch (Exception e)
            {
                _notifier.Message($"Error searching AE directories: {e.Message}");
            }
        }
         else
         {
             _notifier.Message($"Adobe Folder '{_options.ProgramFilesAdobeRoot}' not found");
         }

         _notifier.Message("--- Found Plugin Root Folders ---");
         if (foundFolders.Any())
         {
             foreach(var folderPath in foundFolders)
             {
                 _notifier.Message($"  - {folderPath}");
             }
         }
         else
         {
             _notifier.Message("No plugin root folders found.");
         }
         _notifier.Message("-------------------------------");


        return foundFolders;
    }

    public List<Plugin> FindAllPlugins()
    {
        List<Plugin> plugins = new List<Plugin>();
        List<string> rootFolders = FindPluginRootFolders();

        if (!rootFolders.Any())
        {
            _notifier.Message("No root plugin folders found to search for plugins.");
            return plugins;
        }

        _notifier.Message("--- Searching for Plugin Files ---");
        foreach (var rootFolder in rootFolders)
        {
            if (!_fileSystem.DirectoryExists(rootFolder))
            {
                _notifier.Message($"Root folder not found, skipping: {rootFolder}");
                continue;
            }

            try
            {
                var allPossibleFiles = _options.PluginFilePatterns
                    .SelectMany(pattern => _fileSystem.GetFiles(rootFolder, pattern, SearchOption.AllDirectories))
                    .ToList();

                _notifier.Message($"Found {allPossibleFiles.Count} potential plugin files in {rootFolder} and subdirectories.");

                var filteredFiles = allPossibleFiles
                    .Where(filePath =>
                    {
                        string directory = _fileSystem.GetDirectoryName(filePath);
                        return !_options.ExcludedFolderNames.Any(excludedName =>
                        {
                            string pattern = $"{Path.DirectorySeparatorChar}{excludedName}{Path.DirectorySeparatorChar}";
                            return directory.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                        });
                    })
                    .ToList();

                 _notifier.Message($"Filtered down to {filteredFiles.Count} files after excluding certain subfolders.");


                foreach (var filePath in filteredFiles)
                {
                    string fileName = _fileSystem.GetFileName(filePath);
                    plugins.Add(new Plugin(fileName, filePath, "", ""));
                    _notifier.Message($"  - Added Plugin: {fileName} ({filePath})");
                }
            }
            catch (Exception e)
            {
                _notifier.Message($"Error searching for plugin files in '{rootFolder}': {e.Message}");
            }
        }
        _notifier.Message("-------------------------------");

        return plugins;
    }

    public Dictionary<string, List<Plugin>> CategorizePlugins()
    {
        Dictionary<string, List<Plugin>> categories = new Dictionary<string, List<Plugin>>();
        List<string> rootFolders = FindPluginRootFolders();

         if (!rootFolders.Any())
        {
            _notifier.Message("No root plugin folders found for categorization.");
            return categories;
        }

        _notifier.Message("--- Categorizing Plugins ---");

        foreach (var rootFolder in rootFolders)
        {
            if (!_fileSystem.DirectoryExists(rootFolder))
            {
                 _notifier.Message($"Root folder not found during categorization, skipping: {rootFolder}");
                continue;
            }

            try
            {
                string[] categoryDirs = _fileSystem.GetDirectories(rootFolder);

                _notifier.Message($"Searching for category subfolders in {rootFolder}");

                foreach (string categoryDir in categoryDirs)
                {
                    string categoryName = _fileSystem.GetFileName(categoryDir);

                    if (_options.ExcludedFolderNames.Contains(categoryName, StringComparer.OrdinalIgnoreCase))
                    {
                        _notifier.Message($"  - Skipping excluded folder: {categoryName}");
                        continue;
                    }

                    _notifier.Message($"  - Checking category folder: {categoryName}");

                    List<Plugin> categoryFiles = new List<Plugin>();
                    try
                    {
                        string[] files = _fileSystem.GetFiles(categoryDir, _options.AexFilePattern, SearchOption.TopDirectoryOnly);

                        if (files.Length == 0)
                        {
                             _notifier.Message($"    - No { _options.AexFilePattern} files found in {categoryDir}");
                        }
                        else
                        {
                            foreach (string file in files)
                            {
                                string fileName = _fileSystem.GetFileName(file);
                                categoryFiles.Add(new Plugin(fileName, file));
                                _notifier.Message($"      - Found {fileName} ({file})");
                            }

                            if (categoryFiles.Any())
                            {
                                if (!categories.ContainsKey(categoryName))
                                {
                                    categories[categoryName] = new List<Plugin>();
                                }
                                categories[categoryName].AddRange(categoryFiles);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _notifier.Message($"Error reading category folder '{categoryDir}': {e.Message}");
                    }
                }

                try
                {
                     _notifier.Message($"Searching for root { _options.FfxFilePattern} files in {rootFolder}");
                     string[] rootFiles = _fileSystem.GetFiles(rootFolder, _options.FfxFilePattern, SearchOption.TopDirectoryOnly);

                     if (rootFiles.Any())
                     {
                         string rootCategoryName = "Uncategorized";
                         List<Plugin> rootCategoryFiles = rootFiles
                            .Select(filePath => new Plugin(_fileSystem.GetFileName(filePath), filePath))
                            .ToList();

                         _notifier.Message($"  - Found {rootCategoryFiles.Count} uncategorized files:");
                          foreach(var rootFile in rootCategoryFiles)
                          {
                              _notifier.Message($"      - {rootFile.Name} ({rootFile.Path})");
                          }

                         if (!categories.ContainsKey(rootCategoryName))
                         {
                             categories[rootCategoryName] = new List<Plugin>();
                         }
                         categories[rootCategoryName].AddRange(rootCategoryFiles);
                     }
                     else
                     {
                         _notifier.Message("    - No uncategorized files found");
                     }
                }
                catch (Exception e)
                {
                    _notifier.Message($"Error reading root folder '{rootFolder}': {e.Message}");
                }
            }
            catch (Exception e)
            {
                _notifier.Message($"Error processing root folder '{rootFolder}' during categorization: {e.Message}");
            }
        }

        _notifier.Message("--- Categorization Complete ---");
        foreach(var category in categories)
        {
            _notifier.Message($"Category '{category.Key}' has {category.Value.Count} plugins.");
        }
        
        _notifier.Message("-----------------------------");
        
        return categories;
    }
}
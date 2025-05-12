using System.Text.RegularExpressions;

namespace PluginManager
{
      // Основной прототип класса для плагина, масштабируемый (слава богу)
      sealed class Plugin
      {
            private static int _count = 0;
            public static int Count => _count;
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
                  _count++;
            }
      }

      namespace AeFolders
      {
            public static class FolderFinder
            {

                  public static string ProgramFilesAdobeRoot = "C:\\Program Files\\Adobe";
                  
                  // Собирает в список все существующие директории с плагинами
                  public static List<AeFolderItem> Find()   
                  {
                        List<AeFolderItem> foundFolders = [];
                        string pattern = "Adobe After Effects*";
                        if (Directory.Exists(ProgramFilesAdobeRoot))
                        {
                              if (Directory.Exists(System.IO.Path.Combine(ProgramFilesAdobeRoot, "Common",
                                        "Plug-ins", "7.0", "MediaCore")))
                              {
                                    foundFolders.Add(new AeFolderItem(System.IO.Path.Combine(ProgramFilesAdobeRoot, 
                                          "Common", 
                                          "Plug-ins",
                                          "7.0",
                                          "MediaCore")));
                              }
                              try
                              {
                                    string[] aeDirs = Directory.GetDirectories(ProgramFilesAdobeRoot, pattern);
                                    foreach (string aeDir in aeDirs)
                                    {
                                          string dirName = Path.GetFileName(aeDir);
                                          if (Regex.IsMatch(dirName, @"^Adobe After Effects \d{4}$"))
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
                  public static List<AeFileItem> CollectItems()
                  {
                        List<AeFileItem> items = [];
                        List<AeFolderItem> foundFolders = Find();
                        if (foundFolders.Any())
                        {
                              foreach (var folder in foundFolders)
                              {
                                    if (Directory.Exists(folder.Path))
                                    {
                                          string pattern = "*.ffx";
                                          string[] rawFoundAeFiles = Directory.GetFiles(folder.Path, pattern, SearchOption.AllDirectories);
                                          List<AeFileItem> finalAeFileItems = rawFoundAeFiles
                                                .Select(filePath => new AeFileItem(filePath))
                                                .ToList();
                                          items.AddRange(finalAeFileItems);
                                    }
                              }
                        }

                        return items;
                  }

                  // Собирает в словарь все категории с файлами в них
                  public static Dictionary<string, List<AeFileItem>> Categorize()
                  { 
                        Dictionary<string, List<AeFileItem>> categories = new Dictionary<string, List<AeFileItem>>();
                        List<AeFolderItem> foundFolders = Find();
                        if (foundFolders.Any())
                        {
                              foreach (var folder in foundFolders)
                              {
                                    string[] categoriesDirs = Directory.GetDirectories(folder.Path);
                                    foreach (string category in categoriesDirs)
                                    {
                                          List<AeFileItem> categoryFiles = new List<AeFileItem>();
                                          string[] files = Directory.GetFiles(category, "*.ffx");
                                          foreach (string file in files)
                                          {
                                                categoryFiles.Add(new AeFileItem(file));
                                          }

                                          if (categories.ContainsKey(category)) categories[category].AddRange(categoryFiles);
                                          else
                                          {
                                                categories.Add(category, categoryFiles);
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
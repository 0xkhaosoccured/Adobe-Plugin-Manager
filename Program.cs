using System;
using PluginManager.AeFolders;

namespace PluginManager
{
      class Program
      {
            static void Main(string[] args)
            {

                  AeFolders.FolderFinder folderFinder = new AeFolders.FolderFinder();
                  Dictionary<string, List<FolderFinder.AeFileItem>> categories = folderFinder.Categorize();
                  Console.WriteLine("Founded categories for .ffx:");

                  if (categories.Any())
                  {
                        foreach (var category in categories)
                        {
                              Console.WriteLine($"Category: {category.Key} ({category.Value.Count} files)");
                              foreach (var fileItem in category.Value)
                              {
                                  Console.WriteLine($"  - {fileItem.Name}");
                              }
                        }
                  }
                  else
                  {
                        Console.WriteLine("Categories is not found");
                  }
                  
            }
      }
}




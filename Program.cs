using System;
using PluginManager.AeFolders;

namespace PluginManager
{
      class Program
      {
            static void Main(string[] args)
            {
                  List<FolderFinder.AeFolderItem> folders = AeFolders.FolderFinder.Find();
                  if (folders.Any())
                  {
                        foreach (var folder in folders)
                        {
                              Console.WriteLine(folder.Path);
                        }
                  }
                  
                  List<FolderFinder.AeFileItem> files = AeFolders.FolderFinder.CollectItems();
                  foreach (var file in files)
                  {
                        Console.Write(file.Path);
                        Console.Write(file.Name);
                  }
                  
                  Dictionary<string,List<FolderFinder.AeFileItem>> categories = AeFolders.FolderFinder.Categorize();
                  foreach (var category in categories)
                  {
                        Console.WriteLine(category.Key);
                  }
            }
      }
}




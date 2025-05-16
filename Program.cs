using System;
using PluginManager.AeFolders;

namespace PluginManager
{
      class Program
      {
            static void Main(string[] args)
            {
                  AeFolders.FolderFinder filemanipulation = new FolderFinder();
                  NotificationSystem notificationSystem = new NotificationSystem();
                  
                  List<FolderFinder.AeFolderItem> folders = filemanipulation.Find();
                  if (folders.Any())
                  {
                        foreach (var folder in folders)
                        {     
                              notificationSystem.Message(folder.Path);
                        }
                  }
                  
                  List<FolderFinder.AeFileItem> files = filemanipulation.CollectItems();
                  foreach (var file in files)
                  {
                        Console.Write(file.Path);
                        Console.Write(file.Name);
                  }
                  
                  Dictionary<string,List<FolderFinder.AeFileItem>> categories = filemanipulation.Categorize();
                  foreach (var category in categories)
                  {
                        Console.WriteLine(category.Key);
                  }
            }
      }
}




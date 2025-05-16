using System;
using System.Collections.Generic;
using System.Linq;
using PluginManager;


class Program
{
      static void Main(string[] args)
      {
            FolderFinder folderFinder = new FolderFinder();

            Console.WriteLine("Starting plugin categorization...");

            try
            {
                  Dictionary<string, List<Plugin>> categories = folderFinder.CategorizePlugins();

                  Console.WriteLine("\n--- Founded categories and plugins ---");

                  if (categories != null && categories.Any())
                  {
                        foreach (var category in categories)
                        {
                              Console.WriteLine($"Category: {category.Key} ({category.Value.Count} plugins)");
                              if (category.Value.Any())
                              {
                                    foreach (var plugin in category.Value)
                                    {
                                          Console.WriteLine($"  - {plugin.Name} ({plugin.Path})");
                                    }
                              }
                              else
                              {
                                    Console.WriteLine("  (No plugins in this category)");
                              }
                              Console.WriteLine();
                        }
                  }
                  else
                  {
                        Console.WriteLine("No categories or plugins were found.");
                  }
            }
            catch (Exception ex)
            {
                  Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            Console.WriteLine("--- Categorization process finished ---");
      }
}
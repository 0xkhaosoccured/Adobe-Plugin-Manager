namespace PluginManager;
using System.Diagnostics;

public interface IFileSystem
{
      void WriteAllText(string path, string text) => File.WriteAllText(path, text);
      string ReadAllText(string text) => File.ReadAllText(text);
      bool FileExists(string path) => File.Exists(path);
      bool DirectoryExists(string path);
      string[] GetDirectories(string path);
      string[] GetDirectories(string path, string searchPattern);
      string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
      string GetFileName(string path);
      string GetDirectoryName(string path);
      string Combine(string path1, string path2);
      string Combine(params string[] paths);
      void RemoveFile(string path);
      void OpenDirectory(string path);
      bool TurnOffFile(string filePath, Dictionary<string, PluginState> dictionary);
      public bool TurnOnFile(string filePath, Dictionary<string, PluginState> dictionary);
}

public class DefaultFileSystem : IFileSystem
{
      void WriteAllText(string path, string text) => File.WriteAllText(path, text);
      string ReadAllText(string text) => File.ReadAllText(text);
      bool FileExists(string path) => File.Exists(path);
      public bool DirectoryExists(string path) => Directory.Exists(path);
      public string[] GetDirectories(string path) => Directory.GetDirectories(path);
      public string[] GetDirectories(string path, string searchPattern) => Directory.GetDirectories(path, searchPattern);
      public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
      public string GetFileName(string path) => Path.GetFileName(path);
      public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
      public string Combine(string path1, string path2) => Path.Combine(path1, path2);
      public string Combine(params string[] paths) => Path.Combine(paths);
      public void RemoveFile(string path) => File.Delete(path);
      public void OpenDirectory(string path) => Process.Start("explorer.exe", path);
      public bool TurnOffFile(string filePath, Dictionary<string, PluginState> dictionary) 
      {
          string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
          string originalExtension = Path.GetExtension(filePath);

          if (!File.Exists(filePath))
          {
              Console.WriteLine($"ERROR: File not found: '{filePath}'.");
              return false;
          }

          if (dictionary.TryGetValue(fileNameWithoutExtension, out var pluginInfo) && pluginInfo.isRemoved)
          {
              Console.WriteLine($"INFO: Plugin '{fileNameWithoutExtension}' already removed.");
              return false;
          }
        
          if (!dictionary.ContainsKey(fileNameWithoutExtension))
          {
              dictionary.Add(fileNameWithoutExtension, new PluginState(fileNameWithoutExtension, originalExtension, false, filePath));
          }

          string pathWithoutFileName = Path.GetDirectoryName(filePath);
          string newFilePath = Path.Combine(pathWithoutFileName, fileNameWithoutExtension + ".removed");

          try
          {
              File.Move(filePath, newFilePath, overwrite: true);

              dictionary[fileNameWithoutExtension].isRemoved = true;
              dictionary[fileNameWithoutExtension]._path = newFilePath; 
              Console.WriteLine($"Plugin '{fileNameWithoutExtension}' successfully removed.");
              return true;
          }
          catch (IOException ex) when (ex.HResult == -2147024816)
          {
              Console.WriteLine($"ERROR: '{fileNameWithoutExtension}': File with this name already exists. '{newFilePath}'");
              return false;
          }
          catch (UnauthorizedAccessException)
          {
              Console.WriteLine($"ERROR: '{fileNameWithoutExtension}': Access is denied. Check file permissions.");
              return false;
          }
          catch (Exception ex)
          {
              Console.WriteLine($"ERROR: Unexpected error with '{fileNameWithoutExtension}': {ex.Message}");
              return false;
          }
      }
      
      public bool TurnOnFile(string filePath, Dictionary<string, PluginState> dictionary)
      {
          string fileNameToRestore = Path.GetFileNameWithoutExtension(filePath);
        
          if (!File.Exists(filePath))
          {
              Console.WriteLine($"ERROR: File not found: '{filePath}'.");
              return false;
          }

          if (!dictionary.TryGetValue(fileNameToRestore, out var pluginInfo) || !pluginInfo.isRemoved)
          {
              Console.WriteLine($"INFO: Plugin '{fileNameToRestore}' not found in dictionary.");
              return false;
          }

          string pathWithoutFileName = Path.GetDirectoryName(filePath);
          string originalExtension = pluginInfo.extension; 
          string newFilePath = Path.Combine(pathWithoutFileName, fileNameToRestore + originalExtension);

          try
          {
              File.Move(filePath, newFilePath, overwrite: true);

              dictionary[fileNameToRestore].isRemoved = false;
              dictionary[fileNameToRestore]._path = newFilePath; 
              Console.WriteLine($"Плагин '{fileNameToRestore}' успешно включен.");
              return true;
          }
          catch (IOException ex) when (ex.HResult == -2147024816)
          {
              Console.WriteLine($"ERROR: '{fileNameToRestore}': File with this name already exists. '{newFilePath}'");
              return false;
          }
          catch (UnauthorizedAccessException)
          {
              Console.WriteLine($"ERROR: '{fileNameToRestore}': Access is denied. Check file permissions.");
              return false;
          }
          catch (Exception ex)
          {
              Console.WriteLine($"ERROR: Unexpected error with '{fileNameToRestore}': {ex.Message}");
              return false;
          }
      }
}

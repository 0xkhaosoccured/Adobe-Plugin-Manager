namespace PluginManager;
using System.Diagnostics;

public interface IFileSystem
{
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
}

public class DefaultFileSystem : IFileSystem
{
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
}

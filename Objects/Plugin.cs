namespace PluginManager;

public sealed class Plugin
{
      public string Name { get; }
      public string Description { get; }
      public string Path { get; }
      public string ImagePath { get; }

      public Plugin(string name, string path, string description = "", string imagePath = "")
      {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Description = description;
            ImagePath = imagePath;
      }

      public override string ToString()
      {
            return $"Name: {Name}, Path: {Path}";
      }
}
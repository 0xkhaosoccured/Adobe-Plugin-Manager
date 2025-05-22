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

public sealed class PluginState
{
      public string name { get; set; }
      public string extension { get; set; }
      public bool isRemoved { get; set; }
      public string _path { get; set; }

      public PluginState() { }
      public PluginState(string name, string extension, bool isRemoved, string path)
      {
            this.name = Path.GetFileNameWithoutExtension(name) ?? throw new ArgumentNullException(nameof(name));
            this.extension = extension ?? throw new ArgumentNullException(nameof(extension));
            this.isRemoved = isRemoved;
            this._path = path ?? throw new ArgumentNullException(nameof(path));
      }
}
namespace PluginManager;

public class AeFinderOptions
{
      public string ProgramFilesAdobeRoot { get; set; } = "C:\\Program Files\\Adobe";
      public string AeDirSearchPattern { get; set; } = "Adobe After Effects*";
      public string AeVersionRegexPattern { get; set; } = @"^Adobe After Effects \d{4}$";
      public string CommonPluginsRelativePath { get; set; } = Path.Combine("Common", "Plug-ins", "7.0", "MediaCore");
      public string FfxFilePattern { get; set; } = "*.ffx";
      public string AexFilePattern { get; set; } = "*.aex";

      public string[] PluginFilePatterns => new[] { FfxFilePattern, AexFilePattern };
      
      public List<string> ExcludedFolderNames { get; set; } = new List<string>
      {
            "(AdobePSL)",
            "Cineware by Maxon",
            "Effects",
            "Extensions",
            "Format",
            "Keyframe"
      };
}
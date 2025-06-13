using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Configuration
{
    /// <summary>
    /// Configuration for multiple music directories
    /// Allows users to add additional directories for local music scanning
    /// </summary>
    public class MusicDirectoryConfig
    {
        private static readonly string ConfigFilePath = Path.Combine("Mods", "BackSpeaker", "music_directories.json");
        private static MusicDirectoryConfig? _instance;
        
        public static MusicDirectoryConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadConfig();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Dictionary of configured music directories with path hash as key
        /// Key: SHA256 hash of normalized directory path
        /// Value: MusicDirectory object with metadata
        /// </summary>
        public Dictionary<string, MusicDirectory> Directories { get; set; } = new Dictionary<string, MusicDirectory>();
        
        /// <summary>
        /// Legacy support for old list-based configuration
        /// This will be automatically converted to dictionary format
        /// </summary>
        [JsonIgnore]
        public List<MusicDirectory>? LegacyDirectories { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public MusicDirectoryConfig()
        {
            // Convert legacy list format to dictionary if needed
            ConvertLegacyFormat();
            
            // Ensure default directory is present
            EnsureDefaultDirectory();
        }
        
        /// <summary>
        /// Generate a unique hash for a directory path
        /// </summary>
        private static string GeneratePathHash(string path)
        {
            // Normalize the path for consistent hashing
            var normalizedPath = Path.GetFullPath(path).ToLowerInvariant().Replace('\\', '/');
            
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath));
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Convert legacy list-based configuration to dictionary format
        /// </summary>
        private void ConvertLegacyFormat()
        {
            if (LegacyDirectories != null && LegacyDirectories.Count > 0)
            {
                LoggingSystem.Info($"Converting {LegacyDirectories.Count} legacy directory entries to new format", "MusicDirectoryConfig");
                
                foreach (var legacyDir in LegacyDirectories)
                {
                    var pathHash = GeneratePathHash(legacyDir.Path);
                    if (!Directories.ContainsKey(pathHash))
                    {
                        Directories[pathHash] = legacyDir;
                        LoggingSystem.Debug($"Converted legacy directory: {legacyDir.Name} -> {pathHash}", "MusicDirectoryConfig");
                    }
                }
                
                // Clear legacy data after conversion
                LegacyDirectories = null;
                SaveConfig();
                LoggingSystem.Info("Legacy directory format conversion completed", "MusicDirectoryConfig");
            }
        }
        
        /// <summary>
        /// Add a new music directory with duplicate validation
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="name">Display name (optional)</param>
        /// <param name="description">Description (optional)</param>
        /// <param name="errorMessage">Error message if addition fails</param>
        /// <returns>True if added successfully, false otherwise</returns>
        public bool AddDirectory(string path, out string errorMessage, string name = "", string description = "")
        {
            errorMessage = "";
            
            try
            {
                // Validate path
                if (string.IsNullOrWhiteSpace(path))
                {
                    errorMessage = "Directory path cannot be empty";
                    LoggingSystem.Warning("Cannot add empty directory path", "MusicDirectoryConfig");
                    return false;
                }
                
                // Convert to absolute path if relative
                var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
                
                // Generate hash for the normalized path
                var pathHash = GeneratePathHash(fullPath);
                
                // Check if directory already exists using hash lookup (O(1) operation)
                if (Directories.ContainsKey(pathHash))
                {
                    var existingDir = Directories[pathHash];
                    errorMessage = $"Directory already exists: '{existingDir.Name}' ({existingDir.Path})";
                    LoggingSystem.Warning($"Directory already configured: {fullPath} (hash: {pathHash})", "MusicDirectoryConfig");
                    return false;
                }
                
                // Validate that the path is not a subdirectory or parent of existing directories
                foreach (var existingEntry in Directories.Values)
                {
                    var existingPath = existingEntry.Path;
                    
                    // Check if new path is a subdirectory of existing path
                    if (IsSubdirectory(existingPath, fullPath))
                    {
                        errorMessage = $"Directory is a subdirectory of existing directory: '{existingEntry.Name}' ({existingPath})";
                        LoggingSystem.Warning($"Directory {fullPath} is a subdirectory of {existingPath}", "MusicDirectoryConfig");
                        return false;
                    }
                    
                    // Check if new path is a parent directory of existing path
                    if (IsSubdirectory(fullPath, existingPath))
                    {
                        errorMessage = $"Directory contains existing directory: '{existingEntry.Name}' ({existingPath})";
                        LoggingSystem.Warning($"Directory {fullPath} contains existing directory {existingPath}", "MusicDirectoryConfig");
                        return false;
                    }
                }
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullPath);
                        LoggingSystem.Info($"Created music directory: {fullPath}", "MusicDirectoryConfig");
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Failed to create directory: {ex.Message}";
                        LoggingSystem.Error($"Failed to create directory {fullPath}: {ex.Message}", "MusicDirectoryConfig");
                        return false;
                    }
                }
                
                // Add to configuration using hash as key
                var musicDir = new MusicDirectory
                {
                    Path = fullPath,
                    Name = string.IsNullOrWhiteSpace(name) ? Path.GetFileName(fullPath) : name,
                    Description = description,
                    IsEnabled = true,
                    IsDefault = false,
                    DateAdded = DateTime.Now
                };
                
                Directories[pathHash] = musicDir;
                SaveConfig();
                
                LoggingSystem.Info($"Added music directory: {musicDir.Name} ({fullPath}) with hash {pathHash}", "MusicDirectoryConfig");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Unexpected error: {ex.Message}";
                LoggingSystem.Error($"Error adding directory: {ex.Message}", "MusicDirectoryConfig");
                return false;
            }
        }
        
        /// <summary>
        /// Overload for backward compatibility
        /// </summary>
        public bool AddDirectory(string path, string name = "", string description = "")
        {
            return AddDirectory(path, out _, name, description);
        }
        
        /// <summary>
        /// Check if one directory is a subdirectory of another
        /// </summary>
        private static bool IsSubdirectory(string parentPath, string childPath)
        {
            try
            {
                var parent = new DirectoryInfo(parentPath);
                var child = new DirectoryInfo(childPath);
                
                // Normalize paths for comparison
                var parentFullPath = parent.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();
                var childFullPath = child.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();
                
                // Check if child path starts with parent path
                return childFullPath.StartsWith(parentFullPath + Path.DirectorySeparatorChar) || 
                       childFullPath.StartsWith(parentFullPath + Path.AltDirectorySeparatorChar);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Remove a music directory by path
        /// </summary>
        public bool RemoveDirectory(string path, out string errorMessage)
        {
            errorMessage = "";
            
            try
            {
                var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
                var pathHash = GeneratePathHash(fullPath);
                
                if (!Directories.ContainsKey(pathHash))
                {
                    errorMessage = "Directory not found in configuration";
                    LoggingSystem.Warning($"Directory not found in config: {fullPath} (hash: {pathHash})", "MusicDirectoryConfig");
                    return false;
                }
                
                var directory = Directories[pathHash];
                
                if (directory.IsDefault)
                {
                    errorMessage = "Cannot remove the default directory";
                    LoggingSystem.Warning("Cannot remove default directory", "MusicDirectoryConfig");
                    return false;
                }
                
                Directories.Remove(pathHash);
                SaveConfig();
                
                LoggingSystem.Info($"Removed music directory: {directory.Name} (hash: {pathHash})", "MusicDirectoryConfig");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error removing directory: {ex.Message}";
                LoggingSystem.Error($"Error removing directory: {ex.Message}", "MusicDirectoryConfig");
                return false;
            }
        }
        
        /// <summary>
        /// Remove a music directory by hash key
        /// </summary>
        public bool RemoveDirectoryByHash(string pathHash, out string errorMessage)
        {
            errorMessage = "";
            
            try
            {
                if (!Directories.ContainsKey(pathHash))
                {
                    errorMessage = "Directory not found";
                    LoggingSystem.Warning($"Directory hash not found: {pathHash}", "MusicDirectoryConfig");
                    return false;
                }
                
                var directory = Directories[pathHash];
                
                if (directory.IsDefault)
                {
                    errorMessage = "Cannot remove the default directory";
                    LoggingSystem.Warning("Cannot remove default directory", "MusicDirectoryConfig");
                    return false;
                }
                
                Directories.Remove(pathHash);
                SaveConfig();
                
                LoggingSystem.Info($"Removed music directory: {directory.Name} (hash: {pathHash})", "MusicDirectoryConfig");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error removing directory: {ex.Message}";
                LoggingSystem.Error($"Error removing directory: {ex.Message}", "MusicDirectoryConfig");
                return false;
            }
        }
        
        /// <summary>
        /// Overload for backward compatibility
        /// </summary>
        public bool RemoveDirectory(string path)
        {
            return RemoveDirectory(path, out _);
        }
        
        /// <summary>
        /// Enable or disable a directory by path
        /// </summary>
        public bool SetDirectoryEnabled(string path, bool enabled)
        {
            try
            {
                var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
                var pathHash = GeneratePathHash(fullPath);
                
                if (!Directories.ContainsKey(pathHash))
                {
                    LoggingSystem.Warning($"Directory not found in config: {fullPath} (hash: {pathHash})", "MusicDirectoryConfig");
                    return false;
                }
                
                var directory = Directories[pathHash];
                directory.IsEnabled = enabled;
                SaveConfig();
                
                LoggingSystem.Info($"Set directory {directory.Name} enabled: {enabled}", "MusicDirectoryConfig");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error setting directory enabled state: {ex.Message}", "MusicDirectoryConfig");
                return false;
            }
        }
        
        /// <summary>
        /// Enable or disable a directory by hash key
        /// </summary>
        public bool SetDirectoryEnabledByHash(string pathHash, bool enabled)
        {
            try
            {
                if (!Directories.ContainsKey(pathHash))
                {
                    LoggingSystem.Warning($"Directory hash not found: {pathHash}", "MusicDirectoryConfig");
                    return false;
                }
                
                var directory = Directories[pathHash];
                directory.IsEnabled = enabled;
                SaveConfig();
                
                LoggingSystem.Info($"Set directory {directory.Name} enabled: {enabled} (hash: {pathHash})", "MusicDirectoryConfig");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error setting directory enabled state: {ex.Message}", "MusicDirectoryConfig");
                return false;
            }
        }
        
        /// <summary>
        /// Get all directories as a list (for backward compatibility)
        /// </summary>
        public List<MusicDirectory> GetAllDirectories()
        {
            return Directories.Values.ToList();
        }
        
        /// <summary>
        /// Get all directories as key-value pairs
        /// </summary>
        public Dictionary<string, MusicDirectory> GetAllDirectoriesWithHashes()
        {
            return new Dictionary<string, MusicDirectory>(Directories);
        }
        
        /// <summary>
        /// Get all enabled directories
        /// </summary>
        public List<MusicDirectory> GetEnabledDirectories()
        {
            return Directories.Values.Where(d => d.IsEnabled).ToList();
        }
        
        /// <summary>
        /// Get all valid (existing) enabled directories
        /// </summary>
        public List<MusicDirectory> GetValidEnabledDirectories()
        {
            return Directories.Values
                .Where(d => d.IsEnabled && Directory.Exists(d.Path))
                .ToList();
        }
        
        /// <summary>
        /// Get directory by hash key
        /// </summary>
        public MusicDirectory? GetDirectoryByHash(string pathHash)
        {
            return Directories.ContainsKey(pathHash) ? Directories[pathHash] : null;
        }
        
        /// <summary>
        /// Get directory hash by path
        /// </summary>
        public string? GetDirectoryHash(string path)
        {
            try
            {
                var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
                var pathHash = GeneratePathHash(fullPath);
                return Directories.ContainsKey(pathHash) ? pathHash : null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Validate all directories and update their status
        /// </summary>
        public void ValidateDirectories()
        {
            foreach (var kvp in Directories)
            {
                var pathHash = kvp.Key;
                var directory = kvp.Value;
                
                directory.IsValid = Directory.Exists(directory.Path);
                
                if (directory.IsValid)
                {
                    try
                    {
                        // Count music files
                        var supportedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".flac" };
                        var fileCount = 0;
                        
                        foreach (var ext in supportedExtensions)
                        {
                            fileCount += Directory.GetFiles(directory.Path, "*" + ext, SearchOption.TopDirectoryOnly).Length;
                        }
                        
                        directory.FileCount = fileCount;
                        directory.LastScanned = DateTime.Now;
                        
                        LoggingSystem.Debug($"Validated directory {directory.Name} (hash: {pathHash}): {directory.FileCount} files", "MusicDirectoryConfig");
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Warning($"Error scanning directory {directory.Path}: {ex.Message}", "MusicDirectoryConfig");
                        directory.FileCount = 0;
                    }
                }
                else
                {
                    directory.FileCount = 0;
                    LoggingSystem.Warning($"Music directory not found: {directory.Path} (hash: {pathHash})", "MusicDirectoryConfig");
                }
            }
            
            SaveConfig();
        }
        
        /// <summary>
        /// Load configuration from file
        /// </summary>
        private static MusicDirectoryConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonConvert.DeserializeObject<MusicDirectoryConfig>(json);
                    
                    if (config != null)
                    {
                        // Ensure default directory is always present
                        config.EnsureDefaultDirectory();
                        
                        LoggingSystem.Info($"Loaded music directory config with {config.Directories.Count} directories", "MusicDirectoryConfig");
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading music directory config: {ex.Message}", "MusicDirectoryConfig");
            }
            
            // Return default config
            LoggingSystem.Info("Creating default music directory config", "MusicDirectoryConfig");
            return new MusicDirectoryConfig();
        }
        
        /// <summary>
        /// Save configuration to file
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                // Ensure directory exists
                var configDir = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
                
                LoggingSystem.Debug("Saved music directory configuration", "MusicDirectoryConfig");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error saving music directory config: {ex.Message}", "MusicDirectoryConfig");
            }
        }
        
        /// <summary>
        /// Get configuration summary
        /// </summary>
        public string GetSummary()
        {
            var enabled = GetEnabledDirectories();
            var valid = GetValidEnabledDirectories();
            var totalFiles = valid.Sum(d => d.FileCount);
            
            return $"Music Directories: {enabled.Count} enabled, {valid.Count} valid, {totalFiles} total files";
        }
        
        /// <summary>
        /// Ensure the default directory is always present
        /// </summary>
        private void EnsureDefaultDirectory()
        {
            var defaultPath = Path.Combine("Mods", "BackSpeaker", "Music");
            var defaultPathHash = GeneratePathHash(defaultPath);
            
            // Check if default directory already exists in config using hash lookup
            var hasDefault = Directories.Values.Any(d => d.IsDefault) || Directories.ContainsKey(defaultPathHash);
            
            if (!hasDefault)
            {
                // Add default directory using hash as key
                var defaultDir = new MusicDirectory
                {
                    Path = defaultPath,
                    Name = "Default BackSpeaker Music",
                    IsEnabled = true,
                    IsDefault = true,
                    Description = "Default music folder for BackSpeaker"
                };
                
                Directories[defaultPathHash] = defaultDir;
                
                LoggingSystem.Info($"Added default music directory to configuration (hash: {defaultPathHash})", "MusicDirectoryConfig");
                
                // Save the updated config
                SaveConfig();
            }
            else
            {
                // Ensure at least one directory is marked as default
                var defaultDir = Directories.Values.FirstOrDefault(d => d.IsDefault);
                if (defaultDir == null)
                {
                    // Check if the default path exists and mark it as default
                    if (Directories.ContainsKey(defaultPathHash))
                    {
                        Directories[defaultPathHash].IsDefault = true;
                        LoggingSystem.Info("Marked existing default path directory as default", "MusicDirectoryConfig");
                        SaveConfig();
                    }
                    else
                    {
                        // Mark the first directory as default if none is marked
                        var firstDir = Directories.Values.FirstOrDefault();
                        if (firstDir != null)
                        {
                            firstDir.IsDefault = true;
                            LoggingSystem.Info("Marked first directory as default", "MusicDirectoryConfig");
                            SaveConfig();
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Represents a configured music directory
    /// </summary>
    public class MusicDirectory
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public bool IsValid { get; set; } = true;
        public int FileCount { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastScanned { get; set; }
        
        public override string ToString()
        {
            return $"{Name} ({Path}) - {FileCount} files";
        }
    }
} 
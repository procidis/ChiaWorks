using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChiaWorks.FileChecker.Extensions;
using ChiaWorks.FileChecker.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ChiaWorks.FileChecker.Services
{
    public class DuplicateFileService
    {
        private readonly ILogger<DuplicateFileService> _logger;
        private readonly DuplicateFileServiceSettings _settings;
        private readonly List<string> _filePaths;
        private readonly List<string> _fileNames;
        private readonly List<string> _duplicateFilePaths;

        public DuplicateFileService(IOptions<DuplicateFileServiceSettings> settings,
            ILogger<DuplicateFileService> logger)
        {
            _logger = logger;
            _settings = settings.Value;

            _filePaths = new List<string>();
            _fileNames = new List<string>();
            _duplicateFilePaths = new List<string>();
        }

        public async Task RunAsync()
        {
            await FindDuplicatesAsync();
            await SaveResultAsync();
        }

        private async Task SaveResultAsync()
        {
            _duplicateFilePaths.Sort(CustomSort);
            await File.WriteAllTextAsync("duplicateFiles.json", _duplicateFilePaths.ToJson());
        }

        private int CustomSort(string x, string y)
        {
            x = x.ReverseString();
            y = y.ReverseString();
            var comp = string.Compare(x, y, StringComparison.CurrentCultureIgnoreCase);
            return comp;
        }

        private Task FindDuplicatesAsync()
        {
            foreach (var item in _settings.SourcePaths)
            {
                if (!Directory.Exists(item))
                    continue;
                var files = Directory.GetFiles(item, _settings.SerachPattern, _settings.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                if (files.Length == 0)
                    continue;
                var innerFileNames = files.Select(Path.GetFileName).ToList();

                var duplicates = innerFileNames.Where(w => _fileNames.Contains(w))
                    .Concat(innerFileNames.GroupBy(x => x)
                        .Where(g => g.Count() > 1)
                        .Distinct()
                        .Select(y => y.Key))
                    .ToList();

                foreach (var duplicate in duplicates)
                {
                    _logger.LogWarning($"Duplicate file {duplicate}");
                }

                var allDuplicates = files.Where(w => duplicates.Contains(w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1))).ToList();
                _duplicateFilePaths.AddRange(allDuplicates);
                _fileNames.AddRange(innerFileNames);
                _filePaths.AddRange(files);
            }

            return Task.CompletedTask;
        }
    }
}
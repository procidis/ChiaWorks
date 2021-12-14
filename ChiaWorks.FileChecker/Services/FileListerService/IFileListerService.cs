using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChiaWorks.FileChecker.Services.FileListerService
{
    public interface IFileListerService
    {
        string[] GetFileList(string path, string searchPattern, bool searchRecursive);
        bool DirectoryExists(string path);
    }
}
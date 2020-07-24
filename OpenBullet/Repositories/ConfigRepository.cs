using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RuriLib;
using RuriLib.Interfaces;
using RuriLib.ViewModels;

namespace OpenBullet.Repositories
{
    // The ID is a tuple containing the category and the filename without extension (they are enough to uniquely identify a config in the repo)
    public class ConfigRepository : IRepository<ConfigViewModel, (string, string)>
    {
        public static string defaultCategory = "Default";
        public string BaseFolder { get; set; }

        //private const string Suffix = ".loli"; 
        private const string Suffix = "svb";

        public ConfigRepository(string baseFolder)
        {
            BaseFolder = baseFolder;
        }

        public void Add(ConfigViewModel entity)
        {
            var path = GetPath(entity);

            // Create the category folder if it doesn't exist
            if (entity.Category != defaultCategory && !Directory.Exists(entity.Category))
            {
                Directory.CreateDirectory(Path.Combine(BaseFolder, entity.Category));
            }

            if (!File.Exists(path))
            {
                IOManager.SaveConfig(entity.Config, path);
            }
            else
            {
                throw new IOException("A config with the same name and category already exists");
            }
        }

        //I1iIil1Il1II

        //file.EndsWith(".loli") || file.EndsWith(".anom")
        public IEnumerable<ConfigViewModel> Get()
        {
            List<ConfigViewModel> configs = new List<ConfigViewModel>();

            bool loliX = false;
            // Load the configs in the root folder
            foreach (var file in Directory.EnumerateFiles(SB.configFolder)
                .Where(file => file.EndsWith(".svb") || file.EndsWith(".loli") ||
                file.EndsWith(".anom") ||
               (loliX = file.EndsWith(".loliX"))))
            {
                try
                {
                    configs.Add(new ConfigViewModel(file,
                        Path.GetFileNameWithoutExtension(file),
                        defaultCategory,
                        IOManager.LoadConfig(file, loliX)));
                    loliX = false;
                }
                catch { loliX = false; }
            }

            // Load the configs in the subfolders
            foreach (var categoryFolder in Directory.EnumerateDirectories(SB.configFolder))
            {
                foreach (var file in Directory.EnumerateFiles(categoryFolder).Where(file => file.EndsWith(".loli")))
                {
                    try
                    {
                        configs.Add(new ConfigViewModel(file,
                            Path.GetFileNameWithoutExtension(file),
                            Path.GetFileName(categoryFolder),
                            IOManager.LoadConfig(file)));
                    }
                    catch { }
                }
            }

            return configs;
        }

        public ConfigViewModel Get((string, string) id)
        {
            var category = id.Item1;
            var fileName = id.Item2;

            var path = GetPath(category, fileName, string.Empty);

            return new ConfigViewModel(path, fileName, category, IOManager.LoadConfig(path));
        }

        public void Remove(ConfigViewModel entity)
        {
            var path = GetPath(entity.Category, entity.FileName, Path.GetExtension(entity.Path));

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new IOException("File not found");
            }
        }

        public void RemoveAll()
        {
            Directory.Delete(BaseFolder, true);
            Directory.CreateDirectory(BaseFolder);
        }

        public void Update(ConfigViewModel entity)
        {
            var path = GetPath(entity);

            if (!File.Exists(path))
            {
                path = path.CreateFileName(entity.Name, true);
                using (File.Create(path)) ;
            }
            IOManager.SaveConfig(entity.Config, path);

            //else
            //{
            //    throw new IOException("File not found");
            //}
        }

        public void Add(IEnumerable<ConfigViewModel> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public void Remove(IEnumerable<ConfigViewModel> entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public void Update(IEnumerable<ConfigViewModel> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        private string GetPath(ConfigViewModel config)
        {
            return GetPath(config.Category, config.FileName, string.Empty);
        }

        //
        private string GetPath(string category, string fileName, string suffix)
        {
            var file = $"{fileName}.{(string.IsNullOrEmpty(suffix) ? Suffix : suffix.Remove(0, 1))}";

            if (category != defaultCategory)
            {
                return Path.Combine(BaseFolder, category, file);
            }
            else
            {
                return Path.Combine(BaseFolder, file);
            }
        }
    }
}

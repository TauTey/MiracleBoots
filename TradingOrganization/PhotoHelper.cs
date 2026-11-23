using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TradingOrganization
{
    /// <summary>
    /// Загрузка фотографий из ресурсов приложения и из папки Avatars.
    /// Если фотографии нет — возвращается заглушка.
    /// </summary>
    public static class PhotoHelper
    {
        private static readonly string[] Extensions = { "", ".jpg", ".jpeg", ".png", ".bmp" };
        private static readonly string[] Folders = { "Images_st", "Images_zv", "Images_dr" };
        private static BitmapImage _placeholder;

        public static string AvatarsDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars"); }
        }

        public static BitmapImage Placeholder
        {
            get
            {
                if (_placeholder == null)
                {
                    var image = LoadFromPack("res", "placeholder.png");
                    _placeholder = image ?? CreateEmptyImage();
                }
                return _placeholder;
            }
        }

        public static BitmapImage Load(string fileName, string preferredFolder = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Placeholder;

            fileName = fileName.Trim();

            try
            {
                if (Path.IsPathRooted(fileName) && File.Exists(fileName))
                    return LoadFromFile(fileName);

                var localPath = Path.Combine(AvatarsDirectory, fileName);
                if (File.Exists(localPath))
                    return LoadFromFile(localPath);
            }
            catch
            {
                // некорректное имя файла — пробуем ресурсы
            }

            var folders = new List<string>();
            if (!string.IsNullOrEmpty(preferredFolder))
                folders.Add(preferredFolder);
            folders.AddRange(Folders);

            foreach (var folder in folders)
            {
                foreach (var extension in Extensions)
                {
                    if (!string.IsNullOrEmpty(extension) &&
                        fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var image = LoadFromPack(folder, fileName + extension);
                    if (image != null)
                        return image;
                }
            }

            return Placeholder;
        }

        private static BitmapImage LoadFromPack(string folder, string fileName)
        {
            var assemblyName = typeof(PhotoHelper).Assembly.GetName().Name;
            var relative = string.Format("/{0};component/{1}/{2}", assemblyName, folder, fileName);
            var absolute = "pack://application:,,," + relative;

            var candidates = new[] { absolute, relative };

            foreach (var candidate in candidates)
            {
                try
                {
                    var uri = new Uri(candidate, UriKind.RelativeOrAbsolute);
                    var streamInfo = Application.GetResourceStream(uri);
                    if (streamInfo == null)
                        continue;

                    using (var stream = streamInfo.Stream)
                        return LoadFromStream(stream);
                }
                catch
                {
                    // пробуем следующий вариант адреса
                }
            }

            return null;
        }

        private static BitmapImage LoadFromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return LoadFromStream(stream);
        }

        private static BitmapImage LoadFromStream(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private static BitmapImage CreateEmptyImage()
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII="));
            image.EndInit();
            image.Freeze();
            return image;
        }
    }

    /// <summary>
    /// Вспомогательные операции с именами и датами.
    /// </summary>
    public static class NameHelper
    {
        public static string FullName(string lastName, string firstName, string patronymic)
        {
            var parts = new[] { lastName, firstName, patronymic };
            var result = new List<string>();

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                    result.Add(part.Trim());
            }

            return string.Join(" ", result);
        }

        public static int Age(DateTime birthDate)
        {
            if (birthDate == DateTime.MinValue)
                return 0;

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;

            return Math.Max(age, 0);
        }

        public static string Capitalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Trim();
            return char.ToUpper(value[0]) + value.Substring(1);
        }

        public static string DateText(DateTime date)
        {
            return date == DateTime.MinValue ? "—" : date.ToString("dd.MM.yyyy");
        }
    }
}

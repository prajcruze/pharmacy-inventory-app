using System.Text.Json;

namespace PharmacyApi.Services
{
    public class JsonFileStore<T>
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public JsonFileStore(string filePath)
        {
            _filePath = filePath;

            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public List<T> ReadAll()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
            }
        }

        public void WriteAll(List<T> items)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(items, _options);
                File.WriteAllText(_filePath, json);
            }
        }
    }
}

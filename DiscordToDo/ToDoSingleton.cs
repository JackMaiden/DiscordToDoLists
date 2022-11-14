using Newtonsoft.Json;

namespace DiscordToDo;

public class ToDoSingleton
{
    public Dictionary<ulong, ToDoList> _Lists = new Dictionary<ulong, ToDoList>();

    public string? _fileName { get; set; }
    public string? _filePath { get; set; }

    public DateTime _lastModified { get; set; }

    public bool _toDoLoaded { get; set; }

    private ToDoSingleton()
    {
        _lastModified = DateTime.Now;
        _toDoLoaded = false;
    }

    public async Task Save()
    {

        Console.WriteLine("Saving!");
        var savePath = Path.Combine(_filePath, _fileName);
        if(!File.Exists(savePath))
        {
            Console.WriteLine("Save Location does not exist creating");
            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
            File.Create(savePath);
        }

        string data = JsonConvert.SerializeObject(_Lists);
        Console.WriteLine($"Saving data {data}");

        await File.WriteAllTextAsync(savePath, data);
    }

    public async Task Load()
    {
        var savePath = Path.Combine(_filePath, _fileName);
        if (!File.Exists(savePath))
        {
            Console.WriteLine("Save Location does not exist creating");
            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
            File.Create(savePath);
        } else
        {
            Console.WriteLine($"Loading data from {savePath}");
            var loadedData = await File.ReadAllTextAsync(savePath);
            Console.WriteLine($"Loading data {loadedData}");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            _Lists = JsonConvert.DeserializeObject<Dictionary<ulong, ToDoList>>(loadedData) ?? _Lists;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
    }


    public static ToDoSingleton Instance { get { return Nested.instance; } }

    private class Nested
    {
        static Nested()
        {
        }

        internal static readonly ToDoSingleton instance = new ToDoSingleton();
    }
}

using DSharpPlus.Entities;

namespace DiscordToDo;
public class ToDoList
{
    public ulong _ListMessageId { get; set; }
    public string _creationUser { get; set; }
    public string _creationUserImage { get; set; }
    public string _ListName { get; set; }
    public string _ListDescription { get; set; }

    public bool _includeUsers { get; set; }

    public List<ToDo> _ToDoList { get; set; }

    public ToDoList(ulong listMessageId, string listName, string creationUser, string creationUserImage, string? listDescription = "", bool includeUsers = false)
    {
        _ListMessageId = listMessageId;
        _ListName = listName;
        _ListDescription = listDescription ?? "";
        _creationUser = creationUser;
        _creationUserImage = creationUserImage;
        _ToDoList = new List<ToDo>();
        _includeUsers = includeUsers;
    }
}

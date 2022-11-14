using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordToDo
{
    public class ToDo
    {
        public string Name { get; set; }

        public string User { get; set; }

        public bool IsComplete { get; set; }

        public long Progress { get; set; }

        public long? MaxProgress { get; set; }

        public ToDo(string name, string user, bool isComplete, long? maxProgress = 1, long progress = 0)
        {
            Name = name;
            User = user;
            IsComplete = isComplete;
            Progress = progress;
            MaxProgress = maxProgress;
        }

    }
}

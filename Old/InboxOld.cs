using System;

namespace GTDApp.Models
{
    public class InboxOld
    {
        private static int _idCount = 1;
        private readonly string _inboxDefault = "Undefined Inbox";
        private string _item;
        private string _user;

        public InboxOld()
        {
            Id = _idCount++;
            CreateTime = DateTime.Now;
            ModifyTime = DateTime.Now;
            _user = "Unknown";
            _item = _inboxDefault;
        }

        public InboxOld(string item) : this()
        {
            _item = item ?? _inboxDefault;
        }

        public InboxOld(string item, string user) : this(item)
        {
            _user = user ?? "Unknown";
        }


        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }

        // Ensure item is defined
        public string Item
        {
            get => _item;
            set
            {
                _item = value ?? _inboxDefault;
                ModifyTime = DateTime.Now;
            }
        }

        public string User
        {
            get => _user;
            set
            {
                _user = value ?? "Unknown";
                ModifyTime = DateTime.Now;
            }
        }
    }
}
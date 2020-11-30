using System;

namespace GTDApp.Models
{
    public class Inbox
    {
        public Inbox()
        {
            Id = 0;
        }

        public Inbox(string item) : this()
        {
            Item = item;
        }

        public int Id { get; set; }
        public string Item { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }

    }
}
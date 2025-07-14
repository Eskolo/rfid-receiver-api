using System;
using System.Collections.Generic;

namespace rfid_receiver_api.Models;

public partial class Location
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Scan> Scans { get; set; } = new List<Scan>();
}

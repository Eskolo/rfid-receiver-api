namespace rfid_receiver_api.Models;

public partial class Location
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public virtual ICollection<Item> Items { get; set; } = [];

    public virtual ICollection<Scan> Scans { get; set; } = [];
}

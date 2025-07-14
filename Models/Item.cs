namespace rfid_receiver_api.Models;

public partial class Item
{
    public required string Id { get; set; }

    public required string Name { get; set; } = null!;

    public int LocationId { get; set; }

    public bool IsPresent { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual ICollection<Scan> Scans { get; set; } = [];
}
namespace rfid_receiver_api.Models;

public partial class Scan
{
    public long Id { get; set; }

    public required DateTime ScannedAt { get; set; }

    public required int LocationId { get; set; }

    public required string ItemId { get; set; }

    public int Rssi { get; set; }

    public virtual Item? Item { get; set; }

    public virtual Location? Location { get; set; }
}
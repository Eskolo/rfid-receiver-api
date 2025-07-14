using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;

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

[Table("scan")]
public class SupaScan : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("scanned_at")]
    public DateTime ScannedAt { get; set; }

    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("item_id")]
    public string ItemId { get; set; } = string.Empty;

    [Column("rssi")]
    public int Rssi { get; set; }
}

namespace rfid_receiver_api.Models;

using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;

public partial class Item
{
    public required string Id { get; set; }

    public required string Name { get; set; } = null!;

    public int LocationId { get; set; }

    public bool IsPresent { get; set; }

    [ForeignKey("LocationId")]
    public virtual Location LocationNavigation { get; set; } = null!;

    public virtual ICollection<Scan> Scans { get; set; } = new List<Scan>();
}

[Table("item")]
public partial class SupaItem : BaseModel
{
    [PrimaryKey("id")]
    public string Id { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("is_present")]
    public bool IsPresent { get; set; }
}

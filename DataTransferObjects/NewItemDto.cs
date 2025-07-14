namespace rfid_receiver_api.DataTransferObjects;

public class NewItemDto
{
    public required string TagHexId { get; set; }
    public required int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsInside { get; set; } = true;
}
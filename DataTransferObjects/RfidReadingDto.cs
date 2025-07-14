namespace rfid_receiver_api.DataTransferObjects;

public class RfidReadingDto
{
    public required string TagHexId { get; set; }
    public required int LocationId { get; set; }
    public required DateTime Timestamp { get; set; }
    public int SiganlStren { get; set; }
}
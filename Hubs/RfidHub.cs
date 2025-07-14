namespace rfid_receiver_api.Hubs;

using Microsoft.AspNetCore.SignalR;

public class RfidHub : Hub
{
    public const string NEW_SCAN_RECEIVED = "receiveMessage";
    public const string ITEM_STATUS_UPDATE = "updateStatus";
}
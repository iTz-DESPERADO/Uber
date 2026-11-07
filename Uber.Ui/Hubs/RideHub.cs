using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class RideHub : Hub
{
    // Client calls this right after page loads to join a ride group
    public Task JoinRideGroup(string rideGroupId)
    {
        System.Console.WriteLine($"Client {Context.ConnectionId} joining ride group: {rideGroupId}");
        return Groups.AddToGroupAsync(Context.ConnectionId, rideGroupId);
    }

    // Driver calls this to join their driver-specific group
    public Task JoinDriverGroup(string driverId)
    {
        System.Console.WriteLine($"Driver {Context.ConnectionId} joining driver group: driver-{driverId}");
        return Groups.AddToGroupAsync(Context.ConnectionId, $"driver-{driverId}");
    }

    public Task LeaveRideGroup(string rideGroupId)
    {
        System.Console.WriteLine($"Client {Context.ConnectionId} leaving ride group: {rideGroupId}");
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, rideGroupId);
    }

    // Test method for debugging
    public Task<string> TestConnection()
    {
        System.Console.WriteLine($"Test connection called by {Context.ConnectionId}");
        return Task.FromResult($"Connection test successful! Connection ID: {Context.ConnectionId}");
    }

    // Driver will call these from the Driver dashboard UI
    public async Task AcceptRide(string rideGroupId, int rideDbId)
    {
        System.Console.WriteLine($"AcceptRide called: rideGroupId={rideGroupId}, rideDbId={rideDbId}");
        // tell the user
        await Clients.Group(rideGroupId).SendAsync("RideAccepted", rideDbId);
    }

    public async Task RejectRide(string rideGroupId, int rideDbId)
    {
        System.Console.WriteLine($"RejectRide called: rideGroupId={rideGroupId}, rideDbId={rideDbId}");
        await Clients.Group(rideGroupId).SendAsync("RideRejected", rideDbId);
    }

    // Driver calls this when they arrive at pickup location
    public async Task DriverArrived(string rideGroupId, int rideDbId)
    {
        System.Console.WriteLine($"DriverArrived called: rideGroupId={rideGroupId}, rideDbId={rideDbId}");
        await Clients.Group(rideGroupId).SendAsync("DriverArrived", rideDbId);
    }

    // User calls this to rate the driver
    public async Task RateDriver(string rideGroupId, int rideDbId, int rating)
    {
        System.Console.WriteLine($"RateDriver called: rideGroupId={rideGroupId}, rideDbId={rideDbId}, rating={rating}");
        await Clients.Group(rideGroupId).SendAsync("DriverRated", rideDbId, rating);
    }

    // Driver calls this to rate the user
    public async Task RateUser(string rideGroupId, int rideDbId, int rating)
    {
        System.Console.WriteLine($"RateUser called: rideGroupId={rideGroupId}, rideDbId={rideDbId}, rating={rating}");
        await Clients.Group(rideGroupId).SendAsync("UserRated", rideDbId, rating);
    }
}

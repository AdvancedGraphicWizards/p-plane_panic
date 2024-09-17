using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public static class RelayManager {
    
    // Allocate a new relay server with a maximum number of participants.
    public static async Task<string> CreateRelay(int maxParticipants) {

        // Initialize all Unity Services subscribed to Core, and sign in anonymously to use Relay.
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try {
            // Create a new relay allocation with a maximum number of participants.
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxParticipants);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Set the relay server data for the network manager and start the host.
            RelayServerData relayServerData = new RelayServerData(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartServer();
            return joinCode;
        } catch (RelayServiceException e) {
            Debug.LogError("Failed to create relay: " + e.Message);
        }
        return null;
    }

    // Join a relay server using a join code.
    public static async void JoinRelay(string joinCode) {

        // Initialize all Unity Services subscribed to Core, and sign in anonymously to use Relay.
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try {
            // If the network manager is already running, shut it down before joining a new relay.
            NetworkManager.Singleton.Shutdown();

            // Get the allocation from the join code and set the relay server data for the network manager.
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e) {
            Debug.LogError("Failed to join relay: " + e.Message);
        }
    }
}

using System.Buffers;
using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace InstaPoker.Server;

public sealed class UserManager {

    private readonly object gate = new();
    private readonly List<ClientConnection> connections = [];
    private volatile bool shuttingDown;
    private readonly ArrayPool<ClientConnection> connectionsArrayPool;
    private readonly RoomManager roomManager;
    private readonly ILogger<UserManager> logger;

    public UserManager(RoomManager roomManager, ILogger<UserManager> logger) {
        this.roomManager = roomManager;
        this.logger = logger;
        connectionsArrayPool = ArrayPool<ClientConnection>.Create();
    }
    
    public ClientConnectionsSnapshot RentSnapshot() {
        ClientConnection[] arr;
        int count;
        lock (gate) {
            count = connections.Count;
            arr = connectionsArrayPool.Rent(Math.Max(count, 1));
            for (int i = 0; i < count; i++) {
                arr[i] = connections[i];
            }
        }
        ClientConnectionsSnapshot snapshot = new(connectionsArrayPool, arr, count);
        return snapshot;
    }

    public void AddNewConnection(ClientConnection conn) {
        lock (gate) {
            connections.Add(conn);
        }
    }

    public void RemoveConnection(ClientConnection conn, bool notifyRooms = true) {
        lock (gate) {
            connections.Remove(conn);
        }

        // during shutdown the rooms are going away anyway, so there is nobody to notify
        if (notifyRooms && !shuttingDown) {
            roomManager.UnexpectedDisconnect(conn);
        }
    }

    /// <summary>
    /// Cancels every active connection. Called by the server when the host is shutting down.
    /// </summary>
    public void DisconnectAll() {
        ClientConnection[] snapshot;
        lock (gate) {
            shuttingDown = true;
            snapshot = connections.ToArray();
        }

        logger.LogInformation("Closing {count} connection(s)", snapshot.Length);
        foreach (ClientConnection conn in snapshot) {
            conn.Disconnect();
        }
    }
}

public struct ClientConnectionsSnapshot : IEnumerable<ClientConnection>, IDisposable {

    private readonly ArrayPool<ClientConnection> pool;
    private readonly int count;
    private readonly ClientConnection[] array;
    
    public ClientConnectionsSnapshot(ArrayPool<ClientConnection> pool, ClientConnection[] array, int count) {
        this.pool = pool;
        this.array = array;
        this.count = count;
    }

    public void Dispose() {
        pool.Return(array, clearArray: true);
    }

    public IEnumerator<ClientConnection> GetEnumerator() {
        for (int i = 0; i < count; i++) {
            yield return array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
} 

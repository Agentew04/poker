using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Network;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;

namespace InstaPoker.Client.Game;

/// <summary>
/// Class that issues draw commands for the debug interface using Dear ImGui.
/// </summary>
public class DebugWindow {
    
    public PokerGame Game { get; set; } = null!;
    
    /// <summary>
    /// Creates the interface.
    /// </summary>
    public void Render() {
        // ImGui.ShowDemoWindow();
        ImGui.SetNextWindowSize(new Vector2(500,500), ImGuiCond.Always);
        ImGui.Begin("Debug");
        if (ImGui.BeginTabBar("topBar")) {

            NetworkTab();
            GraphicsTab();

            ImGui.EndTabBar();
        }
        ImGui.End();
    }

    private void NetworkTab() {
        if (ImGui.BeginTabItem("Network")) {

            ImGui.Text("Connected: " + NetworkManager.IsConnected);
            ImGui.Text("Server: " + (NetworkManager.IsConnected
                ? $"{NetworkManager.ServerInfo.HostName}:{NetworkManager.ServerInfo.Port}"
                : string.Empty));

            ImGui.Text("Pending Requests");
            if (ImGui.BeginListBox("##pendingrequests")) {
                foreach (Message pending in NetworkManager.Handler?.PendingMessages ??
                                            [new CreateRoomRequest(), new CreateRoomResponse()]) {
                    ImGui.Text(pending.GetType().Name);
                }
                ImGui.EndListBox();
            }

            ImGui.EndTabItem();
        }
    }

    private void GraphicsTab() {
        if (ImGui.BeginTabItem("Graphics")) {
            ImGui.Text($"Fps: {Game.Fps:F2}");
            ImGui.EndTabItem();
        }
    }
}
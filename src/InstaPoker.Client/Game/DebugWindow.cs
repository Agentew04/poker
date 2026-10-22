using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Graphics;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Subsystems;
using InstaPoker.Client.Subsystems.Network;
using InstaPoker.Core.Messages;
using InstaPoker.Core.Messages.Requests;
using InstaPoker.Core.Messages.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace InstaPoker.Client.Game;

/// <summary>
/// Class that issues draw commands for the debug interface using Dear ImGui.
/// </summary>
public class DebugWindow {
    
    public PokerGame Game { get; set; } = null!;

    public static bool Show { get; set; } = false; // isso não deveria ser static mas fds
    
    private NetworkManager? networkManager;
    
    /// <summary>
    /// Creates the interface.
    /// </summary>
    public void Render() {
        if (!Show) {
            return;
        }

        networkManager ??= Service.Provider.GetRequiredService<NetworkManager>();
        
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

            ImGui.Text("Connected: " + networkManager.IsConnected);
            ImGui.Text("Server: " + (networkManager.IsConnected
                ? $"{networkManager.ServerInfo.HostName}:{networkManager.ServerInfo.Port}"
                : string.Empty));

            ImGui.Text("Pending Requests");
            if (ImGui.BeginListBox("##pendingrequests")) {
                foreach (Type pending in networkManager.PendingRequests) {
                    ImGui.Text(pending.Name);
                }
                ImGui.EndListBox();
            }

            ImGui.EndTabItem();
        }
    }

    private void GraphicsTab() {
        if (ImGui.BeginTabItem("Graphics")) {
            
            ImGui.Text($"Fps: {Game.Fps:F2}");
            
            ImGui.Text("Scene Graph: ");
            SceneObject root = Game.RootObject;
            sceneGraphOcurrences.Clear();
            TraverseSceneGraph(root);
            
            ImGui.EndTabItem();
        }
    }

    private Dictionary<string, int> sceneGraphOcurrences = [];
    
    private void TraverseSceneGraph(SceneObject obj) {
        if (ImGui.TreeNode($"{obj.Name} ({obj.GetType().Name})")) {
            if (ImGui.IsItemHovered()) {
                Highlighter.HighlightObject(obj);
            }
            // debug data about a UI item
            ImGui.Text($"Input (Mouse={obj.UseMouse};Keyb={obj.UseKeyboard};)");
            ImGui.Text($"Position: {obj.Position}");
            ImGui.Text($"Size: {obj.Size}");
            obj.OnDebugDraw();
            
            foreach (SceneObject child in obj.GetChildren()) {
                if (!sceneGraphOcurrences.TryGetValue(child.GetType().Name, out int value)) {
                    value = 0;
                    sceneGraphOcurrences[child.GetType().Name] = value;
                }
                sceneGraphOcurrences[child.GetType().Name] = ++value;
                ImGui.PushID(value);
                TraverseSceneGraph(child);
                ImGui.PopID();
            }
            ImGui.TreePop();
        }
        else {
            if (ImGui.IsItemHovered()) {
                Highlighter.HighlightObject(obj);
            }
        }
    }
}
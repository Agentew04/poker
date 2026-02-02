using System.Numerics;
using ImGuiNET;
using InstaPoker.Client.Graphics;
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
                foreach (Type pending in NetworkManager.Handler?.GetPendingRequests() ?? []) {
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
            SceneObject root = Game.RenderScreen;
            sceneGraphOcurrences.Clear();
            TraverseSceneGraph(root);
            
            ImGui.EndTabItem();
        }
    }

    private Dictionary<string, int> sceneGraphOcurrences = [];
    
    private void TraverseSceneGraph(SceneObject obj) {
        if (ImGui.TreeNode($"{obj.Name} ({obj.GetType().Name})")) {
            // debug data about a UI item
            ImGui.Text($"Use Mouse: {obj.UseMouse})");
            ImGui.Text($"Use Keyboard: {obj.UseKeyboard}");
            switch (obj) {
                case Button button:
                    ImGui.Text("Label: " + button.Label);
                    if (ImGui.Button("Press")) {
                        button.Press();
                    }
                    break;
                case LoadingLabel loading:
                    ImGui.Text("Text: " + loading.Text);
                    ImGui.Text("Show dots: " + loading.ShowDots);
                    break;
                case TextBox textbox:
                    ImGui.Text("Text: " + textbox.GetString());
                    ImGui.Text("Placeholder: " + textbox.Placeholder);
                    break;
                case Toast board:
                    ImGui.Text("Text: " + board.Text);
                    break;
                case Label label:
                    ImGui.Text("Text: " + label.Text);
                    break;
                case Fader fader:
                    if (ImGui.Button("Show")) {
                        fader.ShowFor(5);
                    }
                    break;
            }
            
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
    }
}
using InstaPoker.Client.Game.Screens;
using InstaPoker.Client.Graphics.Objects;
using InstaPoker.Client.Subsystems;
using InstaPoker.Client.Subsystems.Network;
using Xunit;

namespace InstaPoker.Tests;

public class SceneObjectInitializationTests {

    private static readonly object initGate = new();

    private static void EnsureServices() {
        lock (initGate) {
            Service.Init();
        }
    }

    private sealed class TestObject : SceneObject {
        [Service] public MainThreadDispatcher? Dispatcher = null;
        [ChildObject] public readonly TestChild Child = new();

        public bool WasInitialized { get; private set; }

        public TestObject() : base("test") { }

        public override bool UseMouse => false;
        public override bool UseKeyboard => false;

        public override void Initialize() {
            WasInitialized = true;
            base.Initialize();
        }
    }

    private sealed class TestChild : SceneObject {
        [Service] public MainThreadDispatcher? Dispatcher = null;

        public bool WasInitialized { get; private set; }

        public TestChild() : base("test child") { }

        public override bool UseMouse => false;
        public override bool UseKeyboard => false;

        public override void Initialize() {
            WasInitialized = true;
            base.Initialize();
        }
    }

    private sealed class TestScreen : SceneObject, IScreen<Screen> {
        [Service] public MainThreadDispatcher? Dispatcher = null;

        public Screen Key => Screen.MainMenu;
        public bool WasInitialized { get; private set; }
        public bool WasShown { get; private set; }

        public TestScreen() : base("test screen") { }

        public override bool UseMouse => false;
        public override bool UseKeyboard => false;

        public override void Initialize() {
            WasInitialized = true;
            base.Initialize();
        }

        public void OnShow() {
            WasShown = true;
        }
    }

    [Fact]
    public void InitializeWithServices_injects_the_object_and_its_child_fields() {
        EnsureServices();
        TestObject obj = new();

        obj.InitializeWithServices();

        Assert.NotNull(obj.Dispatcher);
        Assert.True(obj.WasInitialized);
        Assert.NotNull(obj.Child.Dispatcher);
        Assert.True(obj.Child.WasInitialized);
        Assert.True(obj.HasChild(obj.Child));
    }

    [Fact]
    public void AddScreen_injects_services_initializes_and_shows_the_screen() {
        EnsureServices();
        WindowFrame<Screen> frame = new("frame");
        frame.Initialize();
        TestScreen screen = new();

        frame.AddScreen(screen, isDefault: true);

        Assert.NotNull(screen.Dispatcher);
        Assert.True(screen.WasInitialized);
        Assert.True(screen.WasShown);
        Assert.True(frame.HasChild(screen));
        Assert.Equal(Screen.MainMenu, frame.ActiveKey);
    }
}

namespace InstaPoker.Client.Game.Screens;

public interface IScreen<out T> {
    void OnShow();
    T Key { get; }
}
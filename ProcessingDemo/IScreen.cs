namespace ProcessingDemo;

public interface IScreen
{
    public AppStatus Start();
    public void Tick();
    public (AppStatus status, IScreen screen) React(string text);
}

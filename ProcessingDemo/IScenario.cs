namespace ProcessingDemo;

public interface IScenario
{
    public void Progress(double progress);
    public Frame Frame { get; }
    public double StepSize { get; set; }
}
using System.Drawing;

namespace ProcessingDemo;

public class EventIngestionScenario : IScenario
{
    public double StepSize { get; set; } = ScenarioDefaults.StepSize; 
    public double MaxDuration { get; set; } = ScenarioDefaults.Duration;
    public double BatchSize { get; set; } = 0.1;
    public double BreakSize { get; set; } = 0.01;
    public double StartProbability = 0.7;
    public double CrashProbability = 0;

    private bool hadStarted { get; set; } = false;
    private bool hasCrashed { get; set; } = false;
    private ProgressBar progressBar = new ProgressBar();

    private double duration = 0;
    public void Progress(double progress)
    {
        if (progress == 0)
        {
            progressBar = new ProgressBar();
            duration = 0;
            hadStarted = false;
            hasCrashed = false;
            prevEndPoint = 0;
            prevStartPoint = 0;
            return;
        }

        UpdateProgress(progress);

        var header = GetHeader(progress);

        progressBar.SetProgress(progress);
        progressBar.SetHeader(header);


        progressBar.SetFooter(DateTime.Now.Date.AddSeconds(86400 * progress).ToString("HH:mm:ss")
            .GetRenderFragment(ConsoleColor.Green));
    }

    private RenderFragment[] GetHeader(double progress)
    {
        var header = "";
        var color = ConsoleColor.Green;
        {
            if (!hadStarted)
            {
                header = "System available.";
                if (hasCrashed)
                {
                    header = " Crashed. Stale Data. System available.";
                    color = ConsoleColor.Red;
                }
            }
            else
            {
                header = "Inserting Data. System Under Load. Stale Data.";
                color = ConsoleColor.Yellow;
            }
        }

        var d = duration/MaxDuration *100;
        if (d > 100) d = 100;
        header = $"Progress:{Math.Round(d)}% |"+header;
        return header.GetRenderFragment(color);
    }

    private double prevStartPoint = 0;
    private double prevEndPoint = 0;

    private void UpdateProgress(double progress)
    {
        if (!hadStarted && duration<MaxDuration)
        {
            if (Random.Shared.Next(1, 100) / 100d < StartProbability && progress - prevEndPoint > BreakSize)
            {
                progressBar.StartBatch();
                prevStartPoint = progress;
                hadStarted = true;
                hasCrashed = false;
            }
        }

        else if (hadStarted)
        {
            if (Random.Shared.Next(1, 100) / 100d < CrashProbability)
            {
                hasCrashed = true;
                progressBar.SetBatchColor(ConsoleColor.Red);
                progressBar.EndBatch();
                hadStarted = false; 
            }
            else if(progress - prevStartPoint >  BatchSize )
            {
               progressBar.EndBatch();
               hadStarted = false;
               prevEndPoint = progress;
               duration += progress - prevStartPoint;
            }
        }
    }

    public Frame Frame => progressBar.Build();
}
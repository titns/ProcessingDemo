using System.Drawing;

namespace ProcessingDemo;

public class StreamIngestionScenario : IScenario
{
    public double StepSize { get; set; } = ScenarioDefaults.StepSize;
    public double MaxDuration { get; set; } = ScenarioDefaults.Duration;
    public double WorkingHourStart { get; set; } = (double)8 / 24;
    public double WorkingHourEnd { get; set; } = (double)18 / 24;
    public double CrashProbability { get; set; } = 0.05;
    private bool hadStarted { get; set; } = false;
    private bool hasCrashed { get; set; } = false;
    private ProgressBar progressBar = new ProgressBar();

    public void Progress(double progress)
    {
        if (progress == 0)
        {
            progressBar = new ProgressBar();
            prevStartPoint = 0;
            duration = 0;
            hadStarted = false;
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
                if (hasCrashed && (duration < MaxDuration) &&  duration !=0 )
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

        var d = duration / MaxDuration * 100;
        if (d > 100) d = 100;
        header = $"Progress:{Math.Round(d)}% |" + header;
        return header.GetRenderFragment(color);
    }


    private double duration = 0;
    private double prevStartPoint = 0;

    private void UpdateProgress(double progress)
    {
        if (!hadStarted && duration < MaxDuration && progress >=WorkingHourStart)
        {
            progressBar.StartBatch();
            prevStartPoint = progress;
            hadStarted = true;
            hasCrashed = false;
        }
        else if (hadStarted)
        {
            if (Random.Shared.Next(1, 100) / 100d < CrashProbability)
            {
                hasCrashed = true;
                progressBar.SetBatchColor(ConsoleColor.Red);
            }
            else
            {
                duration += progress - prevStartPoint;
                // if (duration - prevStartPoint <= StepSize + 0.005)
                // {
                //     duration += StepSize;
                // }
                // else
                // {
                //     duration += progress - prevStartPoint - StepSize;
                // }
            }

            progressBar.EndBatch();
            hadStarted = false;
            if (duration<MaxDuration)
            {
                prevStartPoint = progress;
                progressBar.StartBatch();
                hadStarted = true;
            }
        }
    }

    public Frame Frame => progressBar.Build();
}
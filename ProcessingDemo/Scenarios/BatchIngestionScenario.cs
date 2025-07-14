using System.Drawing;

namespace ProcessingDemo;

public class BatchIngestionScenario : IScenario
{
    public double StepSize { get; set; } = ScenarioDefaults.StepSize;
    public double MaxDuration { get; set; } = ScenarioDefaults.Duration;
    public double StartTime { get; set; } = (double)0.025 / 24;
    public double WorkingHourStart { get; set; } = (double)8 / 24;
    public double WorkingHourEnd { get; set; } = (double)18 / 24;
    private bool hadStarted { get; set; } = false;
    private bool hadEnded { get; set; } = false;
    private ProgressBar progressBar = new ProgressBar();

    public void Progress(double progress)
    {
        if (progress == 0)
        {
            progressBar = new ProgressBar();
            hadStarted = false;
            hadEnded = false;
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
        if (progress < WorkingHourStart || progress > WorkingHourEnd)
        {
            if (!hadStarted)
            {
                header = "Non-Working Hours";
            }
            else
            {
                header = "Non-Working Hours. Inserting Data. System Unavailable.";
                color = ConsoleColor.Yellow;
            }
        }

        if (progress > WorkingHourStart)
        {
            if (hadStarted && !hadEnded)
            {
                header = "Working Hours. Inserting Data. System Unavailable.";
                color = ConsoleColor.Red;
            }
            else
            {
                header = "Working Hours. System Available.";
            }
        }

        if (progress < StartTime)
        {
            header = $"Progress:0% |" + header;
        }
        else if (progress > StartTime && progress -StartTime < MaxDuration)
        {
            header = $"Progress:{Math.Round((progress - StartTime)/MaxDuration * 100)}% |" + header;
        }
        else
        {
            header = $"Progress:100% |" + header;
        }

        return header.GetRenderFragment(color);
    }

    private double runTime = 0;

    private void UpdateProgress(double progress)
    {
        runTime = progress - StartTime;

        if (!hadStarted && progress > StartTime && runTime <= MaxDuration)
        {
            progressBar.SetProgress(StartTime);
            progressBar.StartBatch();
            hadStarted = true;
        }

        if (hadStarted && !hadEnded && runTime >= MaxDuration)
        {
            progressBar.SetProgress(progress);
            progressBar.EndBatch();
            hadEnded = true;
        }
    }

    public Frame Frame => progressBar.Build();
}
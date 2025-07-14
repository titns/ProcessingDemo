namespace ProcessingDemo;

public class ScenarioScreen : IScreen
{
    public IScenario scenario;
    private ScreenStatus currentStatus { get; set; } = ScreenStatus.started;
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private enum ScreenStatus
    {
        init,
        started,
        running,
        paused,
        finished
    }

    public ScenarioScreen(IScenario scenario)
    {
        this.scenario = scenario;
    }

    public AppStatus Start()
    {
        Console.Clear();
        Console.SetCursorPosition(0,0);
        RenderTopPart();
        Console.SetCursorPosition(0, 10);
        RenderBottomPart();
        var t = this;
        return AppStatus.WaitingForCharacter;
    }

    public void Tick()
    {
        if (currentStatus == ScreenStatus.running)
        {
            progress += scenario.StepSize;
            scenario.Progress(progress);
            Render();
        }

        if (progress >= 1)
        {
            currentStatus = ScreenStatus.finished;
        }
    }

    public (ProcessingDemo.AppStatus status, IScreen screen) React(string text)
    {
        switch (text)
        {
            case "r":
                Run();
                break;
            case "p":
                Pause();
                break;
            case "s":
                Step();
                break;
            case "c":
                Continue();
                break;
            case "a":
                Reset();
                break;
            case "f":
                if (Math.Abs(scenario.StepSize - 0.02) < 0.002)
                {
                    scenario.StepSize = ScenarioDefaults.StepSize;
                }
                else
                {
                    scenario.StepSize = 0.02;
                }

                break;
            case "e":
                cancellationTokenSource.Cancel();
                return (AppStatus.Completed, this);
        }

        return (AppStatus.WaitingForCharacter, this);
    }

    private void Reset()
    {
        progress = 0;
        currentStatus = ScreenStatus.started;
        scenario.Progress(0);
        var currentStepSize = scenario.StepSize;
        scenario.StepSize = 0.001;
        Step();
        scenario.StepSize = currentStepSize;
    }

    private void Pause()
    {
        if (currentStatus == ScreenStatus.running)
        {
            currentStatus = ScreenStatus.paused;
        }
    }


    private double progress = 0;

    private void Run()
    {
        if (currentStatus != ScreenStatus.finished)
        {
            currentStatus = ScreenStatus.running;
        }

        Render();
    }

    private void Step()
    {
        if (currentStatus == ScreenStatus.paused && currentStatus != ScreenStatus.finished ||
            currentStatus == ScreenStatus.started)
        {
            progress += scenario.StepSize;
            scenario.Progress(progress);
            Render();
        }

        if (progress >= 1)
        {
            currentStatus = ScreenStatus.finished;
        }
    }

    private void Render()
    {
        Console.SetCursorPosition(0,3);
        scenario.Frame.Render();
    }

    private void Continue()
    {
        if (currentStatus != ScreenStatus.paused)
        {
            return;
        }

        currentStatus = ScreenStatus.running;
        Render();
    }


    public void RenderTopPart()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Select operation: r=Run,p=Pause,s=Step,c=Continue,a=reset,f=fast run, e=Exit");
        Console.WriteLine();
    }

    private static char[] sizes = new char[10] { '⣀', '⣄', '⣄', '⣆', '⣇', '⣇', '⣧', '⣧', '⣷', '⣿' };

    private static Dictionary<int, char> chars = new Dictionary<int, char>()
    {
        { -1, '⊢' },
        { -2, '⊣' },
        { -3, '⧺' }
    };

    public void RenderBottomPart()
    {
        var frame = new Frame()
        {
            RenderLines =
            [
                new RenderFragment("Legend:"),
                new RenderFragment("⣿ - progress"),
                new RenderFragment("Nothing being done; ",ConsoleColor.DarkGray)
                    .Append(new RenderFragment("Currently running ingestion; ", ConsoleColor.Yellow))
                    .Append(new RenderFragment("Data Ingestion batch complete; "))
                    .Append(new RenderFragment("Failed Data Ingestion batch", ConsoleColor.Red )),
                new RenderFragment("⊢ - start of batch"),
                new RenderFragment("⊣ - end of batch"),
                new RenderFragment("⧺ - end of batch immediately followed by a start"),
            ]
        };
        frame.Render();
    }
}
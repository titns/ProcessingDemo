namespace ProcessingDemo;

public class SelectionScreen : IScreen
{
    private SelectionScreenStatus currentStatus = SelectionScreenStatus.started;

    private enum SelectionScreenStatus
    {
        started,
        selecting,
        finished
    }


    public void Tick()
    {
    }

    public (AppStatus status, IScreen screen) React(string text)
    {
        if (currentStatus == SelectionScreenStatus.selecting)
        {
            var result = SelectScenario(text);
            currentStatus = result.status;
            if (result.scenario != default)
            {
                var scr = new ScenarioScreen(result.scenario);
                return (scr.Start(), new ScenarioScreen(result.scenario));
            }
            else
            {
                return (AppStatus.WaitingForLine, this);
            }
        }

        return (AppStatus.WaitingForLine, this);
    }

    public AppStatus Start()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Select scenario:");
        Console.WriteLine("1: Batch Ingestion: Standard");
        Console.WriteLine("2: Batch Ingestion: Long Running");
        Console.WriteLine("3: Batch Ingestion: Never Ends");
        Console.WriteLine("4: Event Ingestion: Normal");
        Console.WriteLine("5: Event Ingestion: With Crashes");
        Console.WriteLine("6: Stream Ingestion");
        Console.Write("Your Selection:");
        Console.WriteLine();
        currentStatus = SelectionScreenStatus.selecting;
        return AppStatus.WaitingForCharacter;
    }
    private (SelectionScreenStatus status, IScenario? scenario) SelectScenario(string text)
    {
        if (!Int32.TryParse(text, out int result))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Please enter a valid integer");
            return (SelectionScreenStatus.selecting, null);
        }

        switch (result)
        {
            case 1:
                return (SelectionScreenStatus.finished, new BatchIngestionScenario(){ MaxDuration = 7.0/24});
                break;
            case 2:
                return (SelectionScreenStatus.finished, new BatchIngestionScenario() { MaxDuration = ScenarioDefaults.Duration});
                break;
            case 3:
                return (SelectionScreenStatus.finished, new BatchIngestionScenario() { MaxDuration = 1});
                break;
            case 4:
                return (SelectionScreenStatus.finished, new EventIngestionScenario() { });
                break;
            case 5:
                return (SelectionScreenStatus.finished, new EventIngestionScenario() { CrashProbability = 0.085});
                break;
            case 6:
                return (SelectionScreenStatus.finished, new StreamIngestionScenario());
                break;
        }

        return (SelectionScreenStatus.selecting, null);
    }
}
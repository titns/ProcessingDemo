// See https://aka.ms/new-console-template for more information

using ProcessingDemo;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.ForegroundColor = ConsoleColor.White;

IScreen currentScreen = new SelectionScreen();
var currentStatus = currentScreen.Start();
var prevLine = String.Empty;
while (true)
{
    if (currentStatus == AppStatus.WaitingForCharacter)
    {
        var x = Console.KeyAvailable;
        if (x)
        {
            var key = Console.ReadKey(true);
            var result = currentScreen.React(key.KeyChar.ToString());
            currentScreen = result.screen;
            currentStatus = result.status;
        }
        else
        {
            System.Threading.Thread.Sleep(100);
            currentScreen.Tick();
            continue;
        }
    }

    if (currentStatus == AppStatus.WaitingForLine)
    {
        var x = Console.ReadLine();
        if (x == String.Empty)
        {
            x = prevLine;
        }
        else
        {
            prevLine = x;
        }

        var result = currentScreen.React(x);
        currentScreen = result.screen;
        currentStatus = result.status;
    }

    if (currentStatus == AppStatus.Completed)
    {
        currentScreen = new SelectionScreen();
        currentStatus = currentScreen.Start();
    }
}
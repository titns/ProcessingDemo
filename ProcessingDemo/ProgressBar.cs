using System.Drawing;

namespace ProcessingDemo;

public class BreakPoint
{
    public BreakPoint(bool start, double point, ConsoleColor color)
    {
        Start = start;
        Point = point;
        Color = color;
    }

    public bool Start { get; set; }
    public double Point { get; set; }
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
}

public class ProgressBar
{
    private double progress = 0;
    public int Max { get; set; } = 150;
    private List<BreakPoint> breaks = new();
    private RenderFragment[] statusHeader = [];
    private RenderFragment[] statusFooter = [];
    private RenderFragment status;
    private int[] charSet;

    public ProgressBar()
    {
        status = new RenderFragment(new char[Max], new ConsoleColor[Max]);
        charSet = new int[Max];
    }

    public void SetHeader(RenderFragment[] header)
    {
        this.statusHeader = header;
    }

    public void SetFooter(RenderFragment[] footer)
    {
        this.statusFooter = footer;
    }

    public void SetProgress(double value)
    {
        this.progress = value;
        if (this.progress >= 1)
        {
            this.progress = 1;
        }
        else if (this.progress <= 0)
        {
            this.progress = 0;
        }
    }

    public void StartBatch(ConsoleColor color = ConsoleColor.White)
    {
        if (this.breaks.Count == 0)
        {
            this.breaks.Add(new(true, this.progress, color));
        }

        var lastElement = this.breaks.Last();
        if (lastElement.Start)
        {
            lastElement.Point = this.progress;
        }
        else
        {
            this.breaks.Add(new(true, this.progress, color));
        }
    }

    public void SetBatchColor(ConsoleColor color = ConsoleColor.White)
    {
        if (this.breaks.Count == 0)
        {
            return;
        }

        var lastPoint = this.breaks.Last();
        if (lastPoint.Start)
        {
            lastPoint.Color = color;
        }
    }

    public void EndBatch(ConsoleColor color = ConsoleColor.White)
    {
        if (this.breaks.Count == 0)
        {
            return;
        }

        var lastElement = this.breaks.Last();
        if (!lastElement.Start)
        {
            lastElement.Point = this.progress;
        }
        else
        {
            this.breaks.Add(new(false, this.progress, color));
        }
    }

    private static char[] sizes = new char[10] { '⣀', '⣄', '⣄', '⣆', '⣇', '⣇', '⣧', '⣧', '⣷', '⣿' };

    private static char[] bracket = ['-','⊢','⊣'];//⧺
    private static Dictionary<int, char> chars = new Dictionary<int, char>()
    {
        { -1, '⊢' },
        { -2, '⊣' },
        { -3, '⧺' }
    };


    public Frame Build()
    {
        if (charSet.Length != this.Max)
        {
            charSet = new int[Max];
            this.status = new RenderFragment(new char[Max], new ConsoleColor[Max]);
        }

        int[] arr = charSet;
        ConsoleColor[] colors = this.status.ColorMask;

        var fill = progress.ToLocation(Max); 
        var leftover = progress%1;

        if (fill != Max)
        {
            arr[fill] = leftover.ToLocation(sizes.Length-1);
        }

        for (int i = 0; i < fill; i++)
        {
            arr[i] = sizes.Length - 1;
        }

        for (int i = fill + 1; i < Max; i++)
        {
            arr[i] = 0;
        }

        if (breaks.Count > 0)
        {
            var firstElement = breaks.First();
            var lastElement = breaks.Last();
            var start = firstElement.Point.ToLocation(Max);
            for (var i = 0; i < start; i++)
            {
                colors[i] = ConsoleColor.DarkGray;
            }

            var end = 0;
            for (var i = 1; i < breaks.Count; i++)
            {
                var nextElement = breaks[i];
                end = nextElement.Point.ToLocation(Max);
                if (!nextElement.Start)
                {
                    for (var j = start; j < end; j++)
                    {
                        colors[j] = breaks[i - 1].Color;
                    }
                }
                else
                {
                    for (var j = start; j < end; j++)
                    {
                        colors[j] = ConsoleColor.DarkGray;
                    }
                }

                start = end;
            }

            var currentColor = lastElement.Start ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
            for (var i = end; i < fill; i++)
            {
                colors[i] = currentColor;
            }

            for (var i = fill; i < Max; i++)
            {
                colors[i] = ConsoleColor.DarkGray;
            }

            for (var i = 0; i < breaks.Count; i++)
            {
                var b = breaks[i];
                var l = b.Point.ToLocation(Max);
                if (l == Max)
                {
                    continue;
                }

                colors[l] = ConsoleColor.Red;
                if (arr[l] >= 0)
                {
                    if (b.Start)
                    {
                        arr[l] = -1;
                    }
                    else
                    {
                        arr[l] = -2;
                    }
                }
                else
                {
                    arr[l] = -3;
                }
            }
        }


        char[] result = this.status.Chars;
        for (int i = 0; i < Max; i++)
        {
            var idx = arr[i];
            if (idx < 0)
            {
                result[i] = chars[idx];
                switch (idx)
                {
                    case -1:
                        break;
                    case -2:
                        break;
                    case -3:
                        break;
                }
            }
            else
            {
                result[i] = sizes[idx];
            }
        }

        result[result.Length - 1] = '|';

        var res = statusHeader.Concat(new[] { new RenderFragment(result, colors) }).Concat(statusFooter).ToList();

        return new Frame()
        {
            RenderLines = res
        };
    }
}
using System.Text;

namespace ProcessingDemo;

public struct Frame
{
    public List<RenderFragment> RenderLines;
}

public struct RenderFragment
{
    public char[] Chars;
    public ConsoleColor[] ColorMask;

    public RenderFragment(char[] chars, ConsoleColor[] colorMask)
    {
        this.Chars = chars;
        this.ColorMask = colorMask;
    }

    public RenderFragment(string text, ConsoleColor color = ConsoleColor.White)
    {
        this.Chars = text.ToCharArray();
        this.ColorMask = new ConsoleColor[this.Chars.Length];
        for (var i = 0; i < this.ColorMask.Length; i++)
        {
            this.ColorMask[i] = color;
        }
    }

    public RenderFragment Append(RenderFragment fragment)
    {
        this.ColorMask = this.ColorMask.Concat(fragment.ColorMask).ToArray();
        this.Chars = this.Chars.Concat(fragment.Chars).ToArray();
        return this;
    }
}

public static class Renderer
{
    public static void Render(this Frame frame, bool fullRerender = true)
    {
        foreach (var renderInfo in frame.RenderLines)
        {
            renderInfo.Render(fullRerender);
        }
    }

    private static char[] GetEmptyArray(int length)
    {
        char[] result = new char[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = ' ';
        }

        return result;
    }

    public static void Render(this RenderFragment renderFragment, bool fullRerender = true)
    {
        if (renderFragment.Chars.Length == 0 && fullRerender)
        {
            var width = Console.BufferWidth;
            Console.Write(GetEmptyArray(width));
            return;
        }

        Console.ForegroundColor = renderFragment.ColorMask[0];
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < renderFragment.Chars.Length; i++)
        {
            if (renderFragment.ColorMask[i] != Console.ForegroundColor)
            {
                Console.Write(sb.ToString());
                sb.Clear();
                Console.ForegroundColor = renderFragment.ColorMask[i];
            }

            sb.Append(renderFragment.Chars[i]);
        }

        Console.Write(sb.ToString());
        var position = Console.GetCursorPosition();
        if (fullRerender)
        {
            Console.Write(GetEmptyArray(Console.BufferWidth - position.Left));
        }

        Console.SetCursorPosition(0, position.Top + 1);
    }

    public static RenderFragment[] GetRenderFragment(this string line, ConsoleColor color)
    {
        if (string.IsNullOrEmpty(line))
        {
            return [new RenderFragment([], [])];
        }

        var charray = line.ToCharArray();
        var stcol = new ConsoleColor[charray.Length];
        for (int j = 0; j < stcol.Length; j++)
        {
            stcol[j] = color;
        }

        return [new RenderFragment(charray, stcol)];
    }

    public static RenderFragment[] GetRenderFragment(string[] text, ConsoleColor color)
    {
        if (text == null || text.Length == 0)
        {
            return [];
        }

        var result = new RenderFragment[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            var line = text[i];
            if (string.IsNullOrEmpty(line))
            {
                result[i] = new RenderFragment([], []);
            }

            var charray = line.ToCharArray();
            var stcol = new ConsoleColor[charray.Length];
            for (int j = 0; j < stcol.Length; j++)
            {
                stcol[j] = color;
            }

            result[i] = new RenderFragment(charray, stcol);
        }

        return result;
    }

    public static int ToLocation(this double progress, int size,
        MidpointRounding midpointRounding = MidpointRounding.ToZero)
    {
        if (progress >= 1)
        {
            return size;
        }
        else if (progress <= 0)
        {
            return 0;
        }

        return (int)Math.Round(progress * size, 0, midpointRounding);
    }
}
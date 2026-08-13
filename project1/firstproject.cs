using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

class Program
{
    // Сегменты: a (верх), b (право-верх), c (право-низ),
    // d (низ), e (лево-низ), f (лево-верх), g (середина)
    static readonly Dictionary<char, bool[]> segments = new Dictionary<char, bool[]>
    {
        { '0', new[] { true,  true,  true,  true,  true,  true,  false } },
        { '1', new[] { false, true,  true,  false, false, false, false } },
        { '2', new[] { true,  true,  false, true,  true,  false, true  } },
        { '3', new[] { true,  true,  true,  true,  false, false, true  } },
        { '4', new[] { false, true,  true,  false, false, true,  true  } },
        { '5', new[] { true,  false, true,  true,  false, true,  true  } },
        { '6', new[] { true,  false, true,  true,  true,  true,  true  } },
        { '7', new[] { true,  true,  true,  false, false, false, false } },
        { '8', new[] { true,  true,  true,  true,  true,  true,  true  } },
        { '9', new[] { true,  true,  true,  true,  false, true,  true  } },
    };

    static readonly int vThick = 3;           // толщина (ширина) вертикальных сегментов
    static readonly int hThick = vThick - 1;  // толщина горизонтальных линий — на 1 меньше
    static readonly int vLen = 4;             // длина вертикальных сегментов
    static readonly int barLen = 6;           // длина средней части горизонтальных линий
    static readonly int gap = 4;              // отступ между цифрами

    static void Main()
    {
        // Если запущено не из отдельного окна PowerShell (например, из
        // интегрированного терминала VS Code) — перезапускаем программу
        // в новом окне PowerShell и завершаем текущий процесс.
        if (OperatingSystem.IsWindows() && TryRelaunchInPowerShell())
            return;

        Console.Write("Введите число: ");
        string? input = Console.ReadLine();

        if (string.IsNullOrEmpty(input) || !IsAllDigits(input))
        {
            Console.WriteLine("Пожалуйста, вводите только цифры (0-9).");
            return;
        }

        List<List<string>> digitDrawings = new List<List<string>>();
        foreach (char c in input)
            digitDrawings.Add(BuildDigit(segments[c]));

        int digitWidth = 2 * vThick + barLen;
        int totalWidth = input.Length * (digitWidth + gap);
        int totalHeight = digitDrawings[0].Count + 8;

        if (!TryResizeConsole(totalWidth, totalHeight))
            WarnIfTerminalTooNarrow(totalWidth);

        Console.WriteLine();

        int totalRows = digitDrawings[0].Count;
        for (int row = 0; row < totalRows; row++)
        {
            StringBuilder line = new StringBuilder();
            foreach (var drawing in digitDrawings)
            {
                line.Append(drawing[row]);
                line.Append(new string(' ', gap));
            }
            Console.WriteLine(line.ToString());
        }

        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    // Перезапускает эту же программу в новом окне PowerShell.
    // Возвращает true, если новое окно успешно открыто (тогда текущий
    // процесс можно завершать), и false, если что-то пошло не так —
    // в этом случае программа просто продолжит работать в текущем терминале.
    static bool TryRelaunchInPowerShell()
    {
        const string flag = "SEVEN_SEGMENT_RELAUNCHED";
        if (Environment.GetEnvironmentVariable(flag) == "1")
            return false; // уже перезапущены — не зацикливаемся

        try
        {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(dllPath))
                return false;

            string psCommand = $"$env:{flag}='1'; dotnet \"{dllPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoExit -Command \"{psCommand}\"",
                UseShellExecute = true
            };

            Process.Start(psi);
            return true;
        }
        catch
        {
            // Не получилось открыть PowerShell (например, его нет в PATH) —
            // продолжаем работу в текущем терминале.
            return false;
        }
    }

    // Пытаемся расширить окно консоли под нужную ширину/высоту.
    // Работает в настоящем окне PowerShell (мы туда и перезапускаемся
    // через TryRelaunchInPowerShell), но не работает в псевдотерминале
    // VS Code — там просто ничего не изменится, и вернём false.
    static bool TryResizeConsole(int width, int height)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return false;

            int maxWidth = Console.LargestWindowWidth;
            int maxHeight = Console.LargestWindowHeight;

            width = Math.Min(width, maxWidth);
            height = Math.Min(height, maxHeight);

            if (Console.BufferWidth < width)
                Console.BufferWidth = width;
            if (Console.WindowWidth < width)
                Console.WindowWidth = width;

            if (Console.BufferHeight < height)
                Console.BufferHeight = height;
            if (Console.WindowHeight < height)
                Console.WindowHeight = height;

            return Console.WindowWidth >= width;
        }
        catch
        {
            return false;
        }
    }

    // В терминале VS Code (это псевдотерминал) программно менять ширину
    // окна нельзя — Console.BufferWidth/WindowWidth там не работают.
    // Поэтому просто читаем текущую ширину и предупреждаем, если рисунок
    // в неё не влезает: пользователь может расширить панель терминала
    // или уменьшить размер шрифта (Ctrl + "-") и запустить снова.
    static void WarnIfTerminalTooNarrow(int neededWidth)
    {
        try
        {
            int currentWidth = Console.WindowWidth;
            if (currentWidth > 0 && currentWidth < neededWidth)
            {
                Console.WriteLine();
                Console.WriteLine($"Внимание: ширина терминала {currentWidth} символов, а рисунку нужно {neededWidth}.");
                Console.WriteLine("Расширьте панель терминала или уменьшите шрифт (Ctrl + \"-\"), иначе строки перенесутся.");
            }
        }
        catch
        {
            // Если ширину терминала узнать не удалось — просто продолжаем.
        }
    }

    static List<string> BuildDigit(bool[] seg)
    {
        bool a = seg[0], b = seg[1], c = seg[2], d = seg[3], e = seg[4], f = seg[5], g = seg[6];

        int width = 2 * vThick + barLen;
        List<string> rows = new List<string>();

        string barOn = new string('$', width);
        string barOff = new string(' ', width);
        string midGap = new string(' ', barLen);

        string sideOn = new string('$', vThick);
        string sideOff = new string(' ', vThick);

        for (int i = 0; i < hThick; i++)
            rows.Add(a ? barOn : barOff);

        for (int i = 0; i < vLen; i++)
            rows.Add((f ? sideOn : sideOff) + midGap + (b ? sideOn : sideOff));

        for (int i = 0; i < hThick; i++)
            rows.Add(g ? barOn : barOff);

        for (int i = 0; i < vLen; i++)
            rows.Add((e ? sideOn : sideOff) + midGap + (c ? sideOn : sideOff));

        for (int i = 0; i < hThick; i++)
            rows.Add(d ? barOn : barOff);

        return rows;
    }

    static bool IsAllDigits(string s)
    {
        foreach (char ch in s)
            if (!char.IsDigit(ch)) return false;
        return true;
    }
}
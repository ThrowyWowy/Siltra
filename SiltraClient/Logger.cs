namespace Siltra;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Siltra.Data;

public static class Logger
{
    [DllImport( "kernel32.dll", SetLastError = true )]
    public static extern bool SetConsoleMode( IntPtr hConsoleHandle, int mode );
    [DllImport( "kernel32.dll", SetLastError = true )]
    public static extern bool GetConsoleMode( IntPtr handle, out int mode );

    [DllImport( "kernel32.dll", SetLastError = true )]
    public static extern IntPtr GetStdHandle( int handle );
    private static List<ColorCode> ColorCodes = new();
    static Logger()
    {
        ColorCodes.Add(new('0', 16));
        ColorCodes.Add(new('1', 18));
        ColorCodes.Add(new('2', 34));
        ColorCodes.Add(new('3', 74));
        ColorCodes.Add(new('4', 124));
        ColorCodes.Add(new('5', 90));
        ColorCodes.Add(new('6', 172));
        ColorCodes.Add(new('7', 7));
        ColorCodes.Add(new('8', 8));
        ColorCodes.Add(new('9', 69));
        ColorCodes.Add(new('a', 82));
        ColorCodes.Add(new('b', 87));
        ColorCodes.Add(new('c', 9));
        ColorCodes.Add(new('d', 171));
        ColorCodes.Add(new('e', 190));
        ColorCodes.Add(new('f', 15));
        ColorCodes.Add(new('l', 1));
        ColorCodes.Add(new('m', 9));
        ColorCodes.Add(new('n', 4));
        ColorCodes.Add(new('o', 3));
        ColorCodes.Add(new('r', 15));

        if (OperatingSystem.IsWindows()) {
            IntPtr handle = GetStdHandle(-11);
            GetConsoleMode(handle, out int mode);
            SetConsoleMode(handle, mode | 0x4);
        }
    }

    public static void WriteLine(string line, bool format = true)
    {
        if (format) WriteFormatted(line);
        else WriteUnformatted(line);
    }

    private static void WriteFormatted(string line)
    {
        string newStr = string.Empty;
        line += "&r";

        for (int i = 0; i < line.Length; i++)
        {
            char first = line[i];
            if (first == '&')
            {
                char second = line[i+1];
                ColorCode code = ColorCodes.Find(e => e.Key == second)!;
                if (code == null)
                {
                    newStr += first;
                }
                else
                {
                    newStr += $"\x1b[38;5;{code.col}m";
                    i++;
                }
            }
            else
            {
                newStr += first;
            }
        }

        Console.WriteLine(newStr);
    }

    private static void WriteUnformatted(string line)
    {
        Console.WriteLine(line);
    }
}
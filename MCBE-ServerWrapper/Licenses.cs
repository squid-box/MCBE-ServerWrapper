namespace AhlSoft.BedrockServerWrapper;

using Spectre.Console;

/// <summary>
/// Utility class for usage license output.
/// </summary>
public static class Licenses
{
    private static readonly string[] NewtonSoftJson =
    {
        "The MIT License (MIT)",
        "",
        "Copyright (c) 2007 James Newton-King",
        "",
        "Permission is hereby granted, free of charge, to any person obtaining a copy of",
        "this software and associated documentation files (the \"Software\"), to deal in",
        "the Software without restriction, including without limitation the rights to",
        "use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of",
        "the Software, and to permit persons to whom the Software is furnished to do so,",
        "subject to the following conditions:",
        "",
        "The above copyright notice and this permission notice shall be included in all",
        "copies or substantial portions of the Software.",
        "",
        "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
        "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS",
        "FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR",
        "COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER",
        "IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN",
        "CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
    };

    private static readonly string[] Autofac = 
    {
        "The MIT License (MIT)",
        "",
        "Copyright © 2014 Autofac Project",
        "",
        "Permission is hereby granted, free of charge, to any person obtaining a copy",
        "of this software and associated documentation files (the \"Software\"), to deal",
        "in the Software without restriction, including without limitation the rights",
        "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell",
        "copies of the Software, and to permit persons to whom the Software is",
        "furnished to do so, subject to the following conditions:",
        "",
        "The above copyright notice and this permission notice shall be included in all",
        "copies or substantial portions of the Software.",
        "",
        "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
        "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,",
        "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE",
        "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER",
        "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,",
        "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE",
        "SOFTWARE."
    };

    private static readonly string[] Spectre =
    {
        "MIT License",
        "",
        "Copyright (c) 2020 Patrik Svensson, Phil Scott",
        "",
        "Permission is hereby granted, free of charge, to any person obtaining a copy",
        "of this software and associated documentation files (the \"Software\"), to deal",
        "in the Software without restriction, including without limitation the rights",
        "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell",
        "copies of the Software, and to permit persons to whom the Software is",
        "furnished to do so, subject to the following conditions:",
        "",
        "The above copyright notice and this permission notice shall be included in all",
        "copies or substantial portions of the Software.",
        "",
        "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
        "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,",
        "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE",
        "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER",
        "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,",
        "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE",
        "SOFTWARE."
    };

    /// <summary>
    /// Prints all licenses to Std.Out.
    /// </summary>
    public static void PrintLicenses()
    {
        AnsiConsole.Write(new Rule("License information for 3rd party libraries used:").RuleStyle(Style.Parse("bold lightseagreen")));

        AnsiConsole.WriteLine("");
        AnsiConsole.Write(new Rule("Newtonsoft.Json").RuleStyle(Style.Parse("grey")));
        foreach (var line in NewtonSoftJson)
        {
            AnsiConsole.WriteLine(line);
        }

        AnsiConsole.WriteLine("");
        AnsiConsole.Write(new Rule("Autofac").RuleStyle(Style.Parse("grey")));
        foreach (var line in Autofac)
        {
            AnsiConsole.WriteLine(line);
        }

        AnsiConsole.WriteLine("");
        AnsiConsole.Write(new Rule("Spectre.Console").RuleStyle(Style.Parse("grey")));
        foreach (var line in Spectre)
        {
            AnsiConsole.WriteLine(line);
        }

        AnsiConsole.WriteLine("");
        AnsiConsole.Write(new Rule().RuleStyle(Style.Parse("bold lightseagreen")));
    }
}

using System;
using Microsoft.UI.Xaml;

namespace TaskbarGroups;

public class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Microsoft.UI.Xaml.Application.Start(_ => new App());
    }
}

using System;
using Drafty;
class Program
{
    static void Main(string[] args)
    {
        Counter counter = new();
        counter.Separator = " | ";
        counter.Run();
    }
}
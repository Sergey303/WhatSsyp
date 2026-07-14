using System.Runtime.CompilerServices;

Random rnd = new();
System.Diagnostics.Stopwatch sw = new();

Console.WriteLine("The ultimate math test by Losos.");
float avr = 0;
for (int i = 0; i < 10; i++)
{
    avr += MathTest();
}
Console.WriteLine($"Good job! Your average time is {Math.Round(avr / 10, 3)} seconds!");


float MathTest()
{
    sw.Reset();
    int a = rnd.Next(100);
    int b = rnd.Next(100);
    while (a + b >= 100)
    {
        a = rnd.Next(100);
        b = rnd.Next(100);
    }
    Console.WriteLine("Ready?");
    Console.ReadLine();
    Console.WriteLine($"{a} + {b} = ?");
    sw.Start();
    string c = Console.ReadLine() ?? string.Empty;
    sw.Stop();
    float time = sw.ElapsedMilliseconds / 1000.0f;
    if (c == (a + b).ToString())
    {
        Console.WriteLine($"Right. Time: {time}s");
        return time;
    }
    else
    {
        Console.WriteLine("WRONG!!! LOSER!!!!!!  +20 SECONDS");
        return time + 20;
    }
    
}
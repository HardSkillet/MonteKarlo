using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using static System.Math;

namespace MonteKarlo;

public class Vector
{ 
    public double X { get; set; }
    public double Y { get; set; }
    public double Result { get; set; }

    public Vector(double result, double x, double y)
    { 
        Result = result;
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"Result: {Result}, x: {X}, y: {Y}";
    }
}

public class IndependentNormallyDistributedStandardValue
{
    public double First { get; private set; }
    public double Second { get; private set; }
    private static Random Random = new Random();

    public IndependentNormallyDistributedStandardValue()
    {
        var gammaOne = Random.NextDouble();
        var gammaTwo = Random.NextDouble();
        var temp = Sqrt(-2*Log(gammaOne, E));

        First = temp*Cos(2*PI*gammaTwo);
        Second = temp*Sin(2*PI*gammaTwo);
    }

    public override string ToString() 
    {
        return $"{First}; {Second}";
    }
}

public class MonteKarlo
{
    public const double A = 64;
    public const double Alpha = 0.1;
    public const int N = 2;
    public const double E = 2.7182818284590451;
    public const double PI = 3.1415926535897931;
    public List<Vector> FirstIteration = new List<Vector>();
    public List<Vector> SecondIteration = new List<Vector>();
    public Vector Gradient = new(0, 0, 0);
    private Random Random = new Random();

    public double GenerateX() =>
        Random.NextDouble() * 2;

    public double GenerateY() =>
        Random.NextDouble() * 4 - 2;

    public static double FunctionValue(double x, double y)
    {
        var sumFirst = x*x + y*y;
        var sumSecond = x + y;
        
        var result = A - (sumFirst - Alpha*Alpha) * (sumFirst - Alpha*Alpha) + Alpha * sumSecond;
        
        return result;
    }

    public void GenerateFirst()
    {
        var list = new List<Vector>();

        for (int i = 0; i < 1000; i++)
        {
            var x = GenerateX();
            var y = GenerateY();
            var result = MonteKarlo.FunctionValue(x, y);
            list.Add(new Vector(result, x, y));
        }
        list.Sort(Comparison);
        list.RemoveRange(11, 989);

        FirstIteration = list;
    }

    public void GenerateSecond()
    {
        SecondIteration.Add(FirstIteration[0]);

        for (int k = 0; k < 1000; k++)
        {
            var INDSV = new List<IndependentNormallyDistributedStandardValue>();
            for (int i = 0; i < 5; i++)
            {
                INDSV.Add(new());
            }

            Vector sum = new(0, 0, 0);
            for (int i = 0; i < 10; i++)
            {
                double n;
                if (i % 2 == 0)
                    n = INDSV[i / 2].First;
                else
                    n = INDSV[i / 2].Second;

                sum.X += n * (FirstIteration[i+1].X - FirstIteration[0].X);
                sum.Y += n * (FirstIteration[i+1].Y - FirstIteration[0].Y);
            }
            sum.X /= 10;
            sum.Y /= 10;
            sum.X += FirstIteration[0].X;
            sum.Y += FirstIteration[0].Y;
            sum.Result = FunctionValue(sum.X, sum.Y);
            SecondIteration.Add(sum);

        }
        SecondIteration.Sort(Comparison);
        SecondIteration.RemoveRange(11, 990);
    }
    public static int Comparison(Vector x, Vector y) 
    {
        if (x.Result < y.Result)
            return 1;

        if(x.Result == y.Result)
            return 0;

        return -1;
    }

    public void GenerateGradient() 
    
    {
        double h = 0.001;
        Vector s = new Vector(0, (-2*SecondIteration[0].X + 0.1)*h, (-2*SecondIteration[0].Y+0.1)*h);
        s.X += SecondIteration[0].X;
        s.Y += SecondIteration[0].Y;
        s.Result = FunctionValue(s.X, s.Y);

        Gradient = s;
    }

    public void Swap()
    {
        FirstIteration = SecondIteration;
        SecondIteration = new();
    }
}

public static class Program 
{

    public static void Main() 
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

        var t = Log(10, 2.7182818284590451);
        Console.WriteLine("Простейший случайный поиск");

        var list = new List<Vector>();

        var MK = new MonteKarlo();

        MK.GenerateFirst();
        int j = 0;
        foreach (var f in MK.FirstIteration)
        {
            if (j == 0)
                Console.WriteLine("Max - " + f.ToString());
            else 
                Console.WriteLine(j + ". " + f.ToString());
            j++;
        }
        Console.WriteLine();
        Console.WriteLine("Первая итерация");
        MK.GenerateSecond();
        j = 0;
        foreach (var f in MK.SecondIteration)
        {
            if (j == 0)
                Console.WriteLine("Max - " + f.ToString());
            else
                Console.WriteLine(j + ". " + f.ToString());
            j++;
        }
        MK.Swap();

        Console.WriteLine();
        Console.WriteLine("Вторая итерация");
        MK.GenerateSecond();
        j = 0;
        foreach (var f in MK.SecondIteration)
        {
            if (j == 0)
                Console.WriteLine("Max - " + f.ToString());
            else
                Console.WriteLine(j + ". " + f.ToString());
            j++;
        }

        Console.WriteLine();
        Console.WriteLine("По градиенту");
        MK.GenerateGradient();
        Console.WriteLine(MK.Gradient.ToString());

        Console.ReadKey();
    }
}


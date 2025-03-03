
using static LALib.Lalib;
using System.Security.Cryptography;
using Type = double;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
namespace LA
{
    internal class Program
    {
        // TODO:
            //- ways to differentiate
                //- find a way to take the derivative of a function symbolically
                //- or stick to the numerical approach (diff(xs) / dx)
                //- or use the numerical differentiation formulas
            // - integrate raylib to draw the approximations for different methods, and various graphs like error, ...
        static void Main()
        {

        }
    }
}

namespace LALib
{
    public static class Lalib
    {
        public static Type Factorial(int value) => (Enumerable.Range(1, value).Aggregate(1, (p, item) => p * item));
        public static Type eps = 1e-6D;
        public static Type cos(Type x, int NthDerivative)
        {
            int which = NthDerivative % 4;
            if (which == 0)
                return Math.Cos(x);
            else if (which == 1)
                return -Math.Sin(x);
            else if (which == 2)
                return -Math.Cos(x);
            else if (which == 3)
                return Math.Sin(x);
            throw new Exception("UNREACHABLE");
        }
        public static Type AbsoluteError(Type x, Type x_star)
        {
            return Math.Abs(x - x_star);
        }
        public static Type RelativeError(Type x, Type x_star)
        {
            if (Math.Abs(x) < eps)
                throw new Exception($"Value too small : {x}");
            return AbsoluteError(x, x_star) / Math.Abs(x);
        }
        public class TaylorPolynomial
        {
            public int degree;
            public Type center;
            public List<Type> coefficients;
            public TaylorPolynomial(int degree, Type center, Func<Type, int, Type> function)
            {
                this.degree = degree;
                this.center = center;
                coefficients = [];
                for (int i = 0; i <= degree; i++)
                {
                    coefficients.Add(function(center, i) / Factorial(i));
                }
            }
            public Type At(Type x)
            {
                Type ret = 0;
                for (int i = 0; i < coefficients.Count; i++)
                {
                    ret += coefficients[i] * Math.Pow(x - center, i);
                }
                return ret;
            }
            public Type Error(Type x)
            {
                return Math.Pow(x - center, degree + 1) / Factorial(degree + 1);
            }
        }
        public static class Root
        {
            public static Type Bisection(Func<Type, int, Type> function, Type a, Type b, int iterations, Type error = 0)
            {
                Type a1 = a;
                Type b1 = b;
                Type func_a1 = function(a1, 0);
                Type func_b1 = function(b1, 0);
                if (func_a1 == 0)
                    return a1;
                if (func_b1 == 0)
                    return b1;
                if (func_a1 * func_b1 > 0)
                    throw new Exception("Bisection is not applicable");
                Type c = 0;
                if (error != 0)
                {
                    iterations = (int)(Math.Log2((b1 - a1) / error)) + 1;
                }
                while (iterations-- > 0)
                {
                    c = (a1 + b1) / (Type)2;
                    func_a1 = function(a1, 0);
                    func_b1 = function(b1, 0);
                    Type func_c = function(c, 0);
                    if (func_a1 * func_c < 0)
                        b1 = c;
                    else if (func_b1 * func_c < 0)
                        a1 = c;
                }
                return c;
            }
            public static Type NewtonMethod(Type initial_x0, Func<Type, int, Type> function, int iterations)
            {
                // x(n+1) = x(n) - (g(x(n)) / g'(x(n)))
                Type x = initial_x0;
                while (iterations-- > 0)
                {
                    x = x - (function(x, 0) / function(x, 1));
                }
                return x;
            }
            public static Type SecantMethod(Type initial_x0, Type initial_x1, Func<Type, int, Type> function, int iterations)
            {
                // x(n+1) = x(n) - ( g(x(n)) / ( (g(x(n)) - g(x(n-1))) / (x(n) - x(n-1)) ) )
                Type x1 = initial_x0;
                Type x2 = initial_x1;
                while (iterations-- > 0)
                {
                    Type gprime = (function(x2, 0) - function(x1, 0));
                    Type x = x2 - ((function(x2, 0) * (x2 - x1)) / (gprime));
                    x1 = x2;
                    x2 = x;
                }
                return x2;
            }
            public static Type FalsePositionMethod(Type initial_x0, Type initial_x1, Func<Type, int, Type> function, int iterations)
            {
                Type x1 = initial_x0;
                Type x2 = initial_x1;
                Type final = 0;
                Type func_x1 = function(x1, 0);
                Type func_x2 = function(x2, 0);
                if (func_x1 == 0)
                    return x1;
                if (func_x2 == 0)
                    return x2;
                if (func_x1 * func_x2 > 0)
                    throw new Exception("false position method is not applicable");
                while (iterations-- > 0)
                {
                    Type gprime = (function(x2, 0) - function(x1, 0));
                    Type x = x2 - ((function(x2, 0) * (x2 - x1)) / (gprime));
                    Type func_x = function(x, 0);
                    func_x1 = function(x1, 0);
                    func_x2 = function(x2, 0);
                    if (func_x1 * func_x < 0)
                    {
                        x2 = x;
                        final = x2;
                    }
                    else if (func_x2 * func_x < 0)
                    {
                        x1 = x;
                        final = x1;
                    }
                }
                return final;
            }
        }
        public struct Pair<U, V>
        {
            public U x;
            public V y;
        }
        public static class InterpolatingPolynomials
        {
            public abstract class InterpolatingPolynomial
            {
                public List<Pair<Type, Type>> points = [];
                public abstract Type At(Type x);
            }
            public class Lagrange : InterpolatingPolynomial
            {
                public List<Type> denominators;
                public Lagrange(List<Pair<Type, Type>> pairs)
                {
                    points = pairs;
                    denominators = [];
                    for (int i = 0; i < points.Count; i++)
                    {
                        Type den = 1;
                        for (int j = 0; j < points.Count; j++)
                        {
                            if (i == j)
                                continue;
                            den *= points[i].x - points[j].x;
                        }
                        denominators.Add(den);
                    }
                }
                public override Type At(Type x)
                {
                    Type ret = 0;
                    for (int i = 0; i < points.Count; i++)
                    {
                        Type num = 1;
                        for (int j = 0; j < points.Count; j++)
                        {
                            if (i == j)
                                continue;
                            num *= x - points[j].x;
                        }
                        ret += num / denominators[i] * points[i].y;
                    }
                    return ret;
                }
            }
            public class NevillesMethod : InterpolatingPolynomial
            {
                public NevillesMethod(List<Pair<Type, Type>> pairs)
                {
                    points = pairs;
                }
                private Type At(Type x, int i, int j)
                {
                    if (j == 0)
                    {
                        return points[i].y;
                    }
                    Type foo = (x - points[i - j].x) * At(x, i, j - 1);
                    Type bar = (x - points[i].x) * At(x, i - 1, j - 1);
                    return (foo - bar) / (points[i].x - points[i - j].x);
                }
                public override Type At(Type x)
                {
                    int i = points.Count - 1;
                    int j = points.Count - 1;
                    Type foo = (x - points[i - j].x) * At(x, i, j - 1);
                    Type bar = (x - points[i].x) * At(x, i - 1, j - 1);
                    return (foo - bar) / (points[i].x - points[i - j].x);
                }
            }
            public class NewtonsDDM : InterpolatingPolynomial
            {
                public NewtonsDDM(List<Pair<Type, Type>> pairs)
                {
                    points = pairs;
                }
                public Type KthDD(int k, int i)
                {
                    if (k == 0)
                    {
                        return points[i].y;
                    }
                    return (KthDD(k - 1, i + 1) - KthDD(k - 1, i)) / (points[i + k].x - points[i].x);
                }
                public override Type At(Type x)
                {
                    Type ret = 0;
                    for (int i = 0; i < points.Count; i++)
                    {
                        Type factor = 1;
                        for (int j = 0; j < i; j++)
                        {
                            factor *= x - points[j].x;
                        }
                        ret += KthDD(i, 0) * factor;
                    }
                    return ret;
                }
            }
        }
        public static class NumericalDifferentiation
        {
            public static List<Type> BasicMethod(List<Type> function, Type dx)
            {
                List<Type> result = [];
                for (int i = 0; i < function.Count - 1; i++)
                {
                    result.Add((function[i + 1] - function[i]) / dx);
                }
                return result;
            }
            public static void VisualizeBasicMethod()
            {
                List<Type> function = [];
                Type dx = 0.05;
                int c = 1;
                for (Type i = 0; i < 2 * Math.PI; i += dx)
                {
                    function.Add(cos(i, 0));
                }
                List<Type> dir_function = NumericalDifferentiation.BasicMethod(function, dx);
                for (int i = 0; i < function.Count / c; i++)
                {
                    for (Type j = 0; j <= 1 + function[i]; j += dx)
                        Console.Write(" ");
                    Console.WriteLine("*");
                }
                Console.WriteLine();
                Console.WriteLine();
                for (int i = 0; i < dir_function.Count / c; i++)
                {
                    for (Type j = 0; j <= 1 + dir_function[i]; j += dx)
                        Console.Write(" ");
                    Console.WriteLine("*");
                }
            }
            public static Type ThreePointsFirstDerivativeForward(Func<Type, int, Type>function, Type x, Type h)
            {
                return (-3 * (function(x, 0)) + 4 * (function(x + h, 0)) - (function(x + 2 * h, 0))) / (2 * h);
            }
            public static Type ThreePointsFirstDerivativeCentral(Func<Type, int, Type>function, Type x, Type h)
            {
                return ((function(x + h, 0)) - (function(x - h, 0))) / (2 * h);
            }
            public static Type ThreePointsFirstDerivativeBackWard(Func<Type, int, Type>function, Type x, Type h)
            {
                return ((function(x - 2 * h, 0)) - 4 * (function(x - 1 * h, 0)) + 3 * (function(x, 0))) / (2 * h);
            }
            public static Type xlnx(Type x, int sdf) => x * Math.Log(x);
            public static Type ThreePointsSecondDerivativeCentral(Func<Type, int, Type>function, Type x, Type h)
            {
                return ((function(x - h, 0)) - 2 * (function(x, 0)) + (function(x + h, 0))) / (h * h);
            }
        }

        public static class NumericalIntegration
        {
            public static Type TrapezoidalRule(List<Pair<Type, Type>> points, Type h)
            {
                return (points[0].y + points[1].y) * (h / 2.0);
            }
            public static Type CompositeTrapezoidalRule(Func<Type, int, Type> function, Type a, Type b, Type h)
            {
                Type term = 0;
                int n = (int)((b - a) / h);
                for (int i = 1; i <= n - 1; i++)
                {
                    term += function(a + i * h, 0);
                }
                return (function(a, 0) + function(b, 0) + 2 * term) * (h / 2.0);
            }
        }



    }
}

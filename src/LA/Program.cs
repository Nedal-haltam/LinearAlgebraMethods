
using static LALib.Lalib;
using System.Security.Cryptography;
using Type = double;
using System.Reflection;
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
            NumericalDifferentiation.TestFirstDerivative();
            NumericalDifferentiation.TestSecondDerivative();
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
        public static void TestTaylorCosine()
        {
            for (int c = 0; c < 5; c++)
            {
                Console.WriteLine($"the new center is : {c}");
                Type center = c;
                TaylorPolynomial poly = new(15, c, cos);
                for (float i = c - 1; i <= c + 1; i += 0.1f)
                {
                    Type actual = cos(i, 0);
                    Type approx = poly.At(i);
                    Console.WriteLine($"actual : {actual} , approx : {approx}");
                    Console.WriteLine(AbsoluteError(actual, approx));
                }
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
            public static void TestBiseciton()
            {
                Type a = 0.1;
                Type b = 2.5;
                int iterations = 10;
                Type c = Bisection(cos, a, b, iterations);
                Console.WriteLine("without specifying an error");
                Console.WriteLine($"actual : {Math.PI / 2} , approx : {c}");
                Console.WriteLine($"absolute error : {AbsoluteError(Math.PI / 2, c)}");
                c = Bisection(cos, a, b, iterations, 0.001);
                Console.WriteLine("with specified error of 0.001");
                Console.WriteLine($"actual : {Math.PI / 2} , approx : {c}");
                Console.WriteLine($"absolute error : {AbsoluteError(Math.PI / 2, c)}");
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
            public static void TestNewtonMethod()
            {
                Console.WriteLine($"actual : {Math.PI / 2} , approx : {NewtonMethod(1, cos, 3)}");
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
            public static void TestSecantMethod()
            {
                Console.WriteLine($"actual : {Math.PI / 2} , approx : {SecantMethod(0.8, 1, cos, 3)}");
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
            public static void TestFalsePositionMethod()
            {
                Console.WriteLine($"actual : {Math.PI / 2} , approx : {FalsePositionMethod(1, 2, cos, 3)}");
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
            public static void TestLagrange()
            {
                List<Pair<Type, Type>> pts = [];
                for (int i = -2; i <= 2; i++)
                {
                    pts.Add(new() { x = i, y = cos(i, 0) });
                }
                Lagrange lagrange = new(pts);
                Type x = 0.5;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {lagrange.At(x)}");
                x = 0.75;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {lagrange.At(x)}");
                x = 1.5;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {lagrange.At(x)}");
                x = 1.9;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {lagrange.At(x)}");
                x = 1;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {lagrange.At(x)}");
                x = 2;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {lagrange.At(x)}");
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
            public static void TestNevillesMethod()
            {
                List<Pair<Type, Type>> pts = [];
                for (int i = -2; i <= 2; i++)
                {
                    pts.Add(new() { x = i, y = cos(i, 0) });
                }
                InterpolatingPolynomials.NevillesMethod nevilles = new(pts);
                Type x = 0.5;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {nevilles.At(x)}");
                x = 0.75;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {nevilles.At(x)}");
                x = 1.5;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {nevilles.At(x)}");
                x = 1.9;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {nevilles.At(x)}");
                x = 1;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {nevilles.At(x)}");
                x = 2;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {nevilles.At(x)}");
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
            public static void TestNewtonsDDM()
            {
                List<Pair<Type, Type>> pts = [];
                for (int i = -2; i <= 2; i++)
                {
                    pts.Add(new() { x = i, y = cos(i, 0) });
                }
                InterpolatingPolynomials.NewtonsDDM newtonddm = new(pts);
                Type x = 0.5;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {newtonddm.At(x)}");
                x = 0.75;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {newtonddm.At(x)}");
                x = 1.5;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {newtonddm.At(x)}");
                x = 1.9;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {newtonddm.At(x)}");
                x = 1;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {newtonddm.At(x)}");
                x = 2;
                Console.WriteLine($"actual : {cos(x, 0)}, approx : {newtonddm.At(x)}");
            }
        }
        public static class NumericalDifferentiation
        {// TODO: change the interface such that we only provide the point the `h` and it figures out the rest
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
            public static void TestFirstDerivative()
            {
                List<Pair<Type, Type>> pts = [];
                pts.Add(new() { x = 8.1, y = 16.94410 });
                pts.Add(new() { x = 8.3, y = 17.56492 });
                pts.Add(new() { x = 8.5, y = 18.19056 });
                pts.Add(new() { x = 8.7, y = 18.82091 });
                // these were check from the lectures
                Console.WriteLine(NumericalDifferentiation.ThreePointsFirstDerivativeForward(xlnx, pts[0].x, 0.2));

                Console.WriteLine(NumericalDifferentiation.ThreePointsFirstDerivativeForward(xlnx, pts[1].x, 0.2));
                Console.WriteLine(NumericalDifferentiation.ThreePointsFirstDerivativeCentral(xlnx, pts[1].x, 0.2));

                Console.WriteLine(NumericalDifferentiation.ThreePointsFirstDerivativeCentral(xlnx, pts[2].x, 0.2));
                Console.WriteLine(NumericalDifferentiation.ThreePointsFirstDerivativeBackWard(xlnx, pts[2].x, 0.2));

                Console.WriteLine(NumericalDifferentiation.ThreePointsFirstDerivativeBackWard(xlnx, pts[3].x, 0.2));
            }
            public static Type ThreePointsSecondDerivativeCentral(Func<Type, int, Type>function, Type x, Type h)
            {
                return ((function(x - h, 0)) - 2 * (function(x, 0)) + (function(x + h, 0))) / (h * h);
            }
            public static void TestSecondDerivative()
            {
                List<Pair<Type, Type>> pts = [];
                pts.Add(new() { x = 0.7, y = cos(0.7, 0) });
                pts.Add(new() { x = 0.8, y = cos(0.8, 0) });
                pts.Add(new() { x = 0.9, y = cos(0.9, 0) });

                Type approx = NumericalDifferentiation.ThreePointsSecondDerivativeCentral(cos, pts[1].x, 0.1);
                Type actual = cos(0.8, 2);
                Console.WriteLine($"actual : {actual} , approx : {approx}");
            }
        }

        public static class NumericalIntegration
        {
            public static Type TrapezoidalRule(List<Pair<Type, Type>> points, Type h)
            {
                return (points[0].y + points[1].y) * (h / 2);
            }
            public static void TestTrapezoidalRule()
            {
                List<Pair<Type, Type>> pts = [];
                pts.Add(new() { x = 1.0, y = Math.Sqrt(1 + Math.Pow(1.0, 2))});
                pts.Add(new() { x = 1.5, y = Math.Sqrt(1 + Math.Pow(1.5, 2))});

                Type approx = NumericalIntegration.TrapezoidalRule(pts, pts[1].x - pts[0].x);

                Console.WriteLine($"approx : {approx}");
            }
            public static Type CompositeTrapezoidalRule(Func<Type, int, Type> function, Type a, Type b, Type h)
            {
                throw new NotImplementedException();
            }
        }



    }
}

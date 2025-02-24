﻿

using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using Type = double;
namespace LA
{
    

    internal class Program
    {
        public static Type Factorial(int value) => (Enumerable.Range(1, value).Aggregate(1, (p, item) => p * item));
        static Type eps = 1e-6D;
        static Type AbsoluteError(Type x, Type x_star)
        {
            return Math.Abs(x - x_star);
        }
        static Type RelativeError(Type x, Type x_star)
        {
            if (Math.Abs(x) < eps)
                throw new Exception($"Value too small : {x}");
            return AbsoluteError(x, x_star) / Math.Abs(x);
        }

        public struct TaylorPolynomial
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

        static Type cos(Type x, int NthDerivative)
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
        static void TestTaylorCosine()
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

        static Type RootBisection(Func<Type, int, Type> function, Type a, Type b, int iterations, Type error = 0)
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
        static void TestRootBiseciton()
        {
            Type a = 0.1;
            Type b = 2.5;
            int iterations = 10;
            Type c = RootBisection(cos, a, b, iterations);
            Console.WriteLine("without specifying an error");
            Console.WriteLine($"actual : {Math.PI / 2} , approx : {c}");
            Console.WriteLine($"absolute error : {AbsoluteError(Math.PI / 2, c)}");
            c = RootBisection(cos, a, b, iterations, 0.001);
            Console.WriteLine("with specified error of 0.001");
            Console.WriteLine($"actual : {Math.PI / 2} , approx : {c}");
            Console.WriteLine($"absolute error : {AbsoluteError(Math.PI / 2, c)}");
        }
        static Type RootNewtonMethod(Type initial_x0, Func<Type, int, Type> function, int iterations)
        {
            // x(n+1) = x(n) - (g(x(n)) / g'(x(n)))
            Type x = initial_x0;
            while (iterations-- > 0)
            {
                x = x - (function(x, 0) / function(x, 1));
            }
            return x;
        }
        static void TestRootNewtonMethod()
        {
            Console.WriteLine($"actual : {Math.PI / 2} , approx : {RootNewtonMethod(1, cos, 3)}");
        }
        static Type RootSecantMethod(Type initial_x0, Type initial_x1, Func<Type, int, Type> function, int iterations)
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
        static void TestRootSecantMethod()
        {
            Console.WriteLine($"actual : {Math.PI / 2} , approx : {RootSecantMethod(0.8, 1, cos, 3)}");
        }
        static Type RootFalsePositionMethod(Type initial_x0, Type initial_x1, Func<Type, int, Type> function, int iterations)
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
            while(iterations-- > 0)
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
        static void TestRootFalsePositionMethod()
        {
            Console.WriteLine($"actual : {Math.PI / 2} , approx : {RootFalsePositionMethod(1, 2, cos, 3)}");
        }

        // TODO:
        // - find a way to take the derivative of a function symbolically or just stick to the numerical approach (diff(xs) / dx)
        // - rename it to Lalib
        static void Main(string[] args)
        {
            TestRootNewtonMethod();
            TestRootSecantMethod();
            TestRootFalsePositionMethod();
        }
    }
}



using System.Globalization;
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
        static void TaylorCosineTest()
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

        static Type Bisection(Func<Type, int, Type> function, Type a, Type b, int iterations)
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
                throw new Exception("Bisection is not applicabla");
            Type c = 0;
            while (iterations-- > 0)
            {
                func_a1 = function(a1, 0);
                func_b1 = function(b1, 0);
                c = (a1 + b1) / (Type)2;
                Type func_c = function(c, 0);
                if (func_a1 * func_c < 0)
                    b1 = c;
                else if (func_b1 * func_c < 0)
                    a1 = c;
            }
            return c;
        }
        // TODO: find a way to take the derivative of a function symbolically
        // or just stick to the numerical approach (diff(xs) / dx)
        static void Main(string[] args)
        {
            Type c = Bisection(cos, 0.1, 2.5, 20);
            Console.WriteLine($"actual : {Math.PI / 2} , approx : {c}");
            Console.WriteLine($"absolute error : {AbsoluteError(Math.PI / 2, c)}");
        }
    }
}

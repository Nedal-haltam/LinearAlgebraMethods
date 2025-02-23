

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
                TaylorPolynomial poly = new(25, c, cos);
                for (float i = c - 1; i <= c + 1; i += 0.1f)
                {
                    Console.WriteLine($"actual : {cos(i, 0)} , approx : {poly.At(i)}");
                }
            }
        }
        static void Main(string[] args)
        {
            TaylorCosineTest();
        }
    }
}

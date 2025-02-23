

using Type = decimal;
namespace LA
{
    internal class Program
    {
        static Type eps = 1e-6M;
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


        static void Main(string[] args)
        {

        }
    }
}

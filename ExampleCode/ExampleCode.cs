namespace ExampleCode
{
    /*
        Class Structure:

                C1
                ^
                |
                C2
                ^^
               /  \
            C3_1  C3_2

        Methods:

            C1   C2   C3_1   C3_2
            ==   ==   ====   ====
            M1   M4<----------M4
            M2<--M2  `-M4     M5
            M3<--------M3
    */


    public class C1
    {
        public int M1(int a)
        {
            if (a > 0)
            {
                return M2(a, 1);
            }
            else
            {
                return M3(a, true, "string");
            }
        }

        public virtual int M2(int a, int b)
        {
            return M3(a, true, b.ToString());
        }

        public virtual int M3(int a, bool b, string c)
        {
            return a;
        }
    }

    public class C2 : C1
    {
        public override int M2(int a, int b)
        {
            return a - b;
        }

        public virtual int M4(int a, int b)
        {
            return a * b;
        }
    }

    public class C3_1 : C2
    {
        public override int M3(int a, bool b, string c)
        {
            return -a;
        }

        public override int M4(int a, int b)
        {
            return (int)System.Math.Pow(a, b);
        }
    }

    public class C3_2 : C2
    {
        public override int M4(int a, int b)
        {
            return a / b;
        }

        public int M5(int a)
        {
            return 0;
        }
    }
}

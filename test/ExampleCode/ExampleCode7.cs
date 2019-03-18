namespace ExampleCode7
{
    /*
        Class Structure:

            Prog

            C1
            ^
            |
            C2
            ^
            |
            C3
    */

    public class Prog
    {
        public int N(int c, int s)
        {
            switch (c)
            {
                case 1: return new C1().M(c, s);
                case 2: return new C2().M(c, s);
                case 3: return new C3().M(c, s);
                default: return s;
            }
        }

        public int MyInc(int c, int s)
        {
            return N(c + 1, s);
        }
    }
    
    public class C1
    {
        public virtual int M(int c, int s)
        {
            return new Prog().MyInc(c, s + 1);
        }
    }

    public class C2 : C1
    {
        public override int M(int c, int s)
        {
            return new Prog().MyInc(c, s + 2);
        }
    }

    public class C3 : C2
    {
        public override int M(int c, int s)
        {
            return new Prog().MyInc(c, s + 3);
        }
    }
}

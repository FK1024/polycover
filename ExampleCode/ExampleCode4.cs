namespace ExampleCode4
{
    /*
        Class Structure:
        
            Prog

            A
            ^
            |
            B
            ^
            |
            C
    */

    public class Prog
    {
        public int M1(A a)
        {
            return a.D();
        }

        public int M2(B b)
        {
            return b.D();
        }
    }
    
    public class A
    {
        public virtual int D()
        {
            return 1;
        }
    }

    public class B : A
    {
        public override int D()
        {
            return 2;
        }
    }

    public class C : B
    {
        public override int D()
        {
            return 3;
        }
    }
}

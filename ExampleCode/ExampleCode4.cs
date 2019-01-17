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
        public int m1(A a)
        {
            return a.d();
        }

        public int m2(B b)
        {
            return b.d();
        }
    }
    
    public class A
    {
        public virtual int d()
        {
            return 1;
        }
    }

    public class B : A
    {
        public override int d()
        {
            return 2;
        }
    }

    public class C : B
    {
        public override int d()
        {
            return 3;
        }
    }
}

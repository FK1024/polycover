namespace ExampleCode5
{
    /*
        Class Structure:
        
            A       C
            ^       ^
            |       |
            B       D
    */

    public class A
    {
        public virtual int M(bool b)
        {
            return N(b);
        }

        public virtual int N(bool b)
        {
            return O(b);
        }

        public int O(bool b)
        {
            if (b)
            {
                C c = new C();
                return c.P();
            }
            else
            {
                D d = new D();
                return d.P();
            }
        }
    }

    public class B : A
    {
        public override int N(bool b)
        {
            return O(b);
        }
    }

    public class C
    {
        public virtual int P()
        {
            return 1;
        }
    }

    public class D : C
    {
        public override int P()
        {
            return 2;
        }
    }
}

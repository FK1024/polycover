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
        public virtual int M()
        {
            return N();
        }

        public virtual int N()
        {
            C c = new C();
            return O(c);
        }

        public int O(C obj)
        {
            return obj.P();
        }
    }

    public class B : A
    {
        public override int N()
        {
            D d = new D();
            return O(d);
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

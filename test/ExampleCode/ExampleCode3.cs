namespace ExampleCode3
{
    /*
        Class Structure:

            <<interface>>
                     IA
                     ^^
                    /  \
                   A    D
                   ^
                   |
                   B
                   ^^
                  /  \
                 C1   C2
    */

    public interface IA
    {
        int M(IA a);
        int N();
    }

    public class A : IA
    {
        public int M(IA a)
        {
            B b = new B();
            int useless = b.N();
            int useless2 = b.N();
            int useless3 = a.N();
            int useless4 = b.N();
            return a.N();
        }

        public virtual int N()
        {
            return 1;
        }

        public virtual int O(int x)
        {
            return 42;
        }
    }

    public class B : A
    {
        public override int N()
        {
            return 2;
        }

        public int O(int x) // hides A.O() and gets inherited instead
        {
            return 43;
        }

        public int O(double x) // doesn't hide A.O() because parameter types don't match
        {
            return 44;
        }

        public double O(float x) // doesn't hide A.O() because return types don't match
        {
            return 45;
        }
    }

    public class C1 : B
    {
        public override int N()
        {
            return 3;
        }
    }

    public class C2 : B
    {

    }

    public class D : IA
    {
        public int M(IA a)
        {
            return 0;
        }

        public int N()
        {
            return 4;
        }
    }
}

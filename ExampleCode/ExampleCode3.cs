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
                 ^
                 |
                 C
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
            return a.N();
        }

        public virtual int N()
        {
            return 1;
        }
    }

    public class B : A
    {
        public override int N()
        {
            return 2;
        }
    }

    public class C : B
    {
        public override int N()
        {
            return 3;
        }
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

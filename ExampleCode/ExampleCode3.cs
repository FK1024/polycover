namespace ExampleCode3
{
    /*
        Class Structure:

            <<interface>>
                 IA
                 ^
                 |
                 A
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
}

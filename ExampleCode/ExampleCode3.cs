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
        int m(IA a);
        int n();
    }
    
    public class A : IA
    {
        public int m(IA a)
        {
            return a.n();
        }

        public virtual int n()
        {
            return 1;
        }
    }

    public class B : A
    {
        public override int n()
        {
            return 2;
        }
    }

    public class C : B
    {
        public override int n()
        {
            return 3;
        }
    }

namespace ExampleCode8
{
    /*
        Class Structure:

            <<interface>>
                   I1
                   ^^
                  /  \
                 C1   C3
                 ^
                 |
                 C2
    */

    public interface I1
    {
        int M2(I1 c);
        int M3(I1 c);
    }

    public class C1 : I1
    {
        public int M1(I1 c)
        {
            C3 c3 = new C3();
            c3.M2(c);
            M4(c);
            return M2(c);
        }

        public int M2(I1 c)
        {
            C2 c2 = new C2();
            c2.M3(c);
            M5(c);
            return c.M3(c);
        }

        public virtual int M3(I1 c)
        {
            return M5(c);
        }

        public int M4(I1 c)
        {
            M5(c);
            return M2(c);
        }

        public int M5(I1 c)
        {
            return M1(c);
        }

        public int M6()
        {
            return M3(new C2());
        }
    }

    public class C2 : C1
    {
        public override int M3(I1 c)
        {
            return 23;
        }
    }

    public class C3 : I1
    {
        public int M2(I1 c)
        {
            C2 c2 = new C2();
            c2.M3(c);
            return c.M3(c);
        }

        public int M3(I1 c)
        {
            C1 c1 = new C1();
            return c1.M1(c);
        }
    }
}

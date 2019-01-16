namespace ExampleCode2
{
    /*
        Class Structure:

            BaseClass
                ^
                |
            *DerivedAbstractClass*
                ^
                |
            DerivedConcreteClass
    */

    public class BaseClass
    {
        public virtual int Method1(int a)
        {
            return a;
        }
    }

    public abstract class DerivedAbstractClass : BaseClass
    {
        public override int Method1(int a)
        {
            return a + 1;
        }

        public abstract double Method2();
    }

    public class DerivedConcreteClass : DerivedAbstractClass
    {
        public override double Method2()
        {
            return 0;
        }

        public bool Method3(bool b)
        {
            return b;
        }
    }
}

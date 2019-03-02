using System.Diagnostics;
using System.IO;

namespace ExampleCode6
{
    /*
        Class Structure:

            A
            ^
            |
            B
    */

    public class A
    {
        public int N(bool b)
        {
            if (b)
            {
                new B().M();
                return new B().M();
            }
            else
            {
                return M();
            }
        }

        public virtual int M()
        {
            return 1;
        }
    }

    public class B : A
    {
        public override int M()
        {
            int ln = new StackFrame(1, true).GetFileLineNumber();
            if (ln == 21 || ln == 22)
            { }
            if (ln == 26)
            { }
            return 2;
        }
    }
}

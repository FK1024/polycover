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
            StackFrame callerStackFrame = new StackFrame(1, true);
            string callerFileName = callerStackFrame.GetFileName();
            int callerLineNumber = callerStackFrame.GetFileLineNumber();
            string thisFileName = Path.GetFullPath(@"..\..\..\ExampleCode\ExampleCode6.cs");
            
            if (callerFileName == thisFileName && callerLineNumber == 21)
            {
                
            }
            if (callerFileName == thisFileName && callerLineNumber == 25)
            {
                
            }

            return 2;
        }
    }
}

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace polycover
{
    public class InheritanceNode
    {
        private ClassDeclarationSyntax classDecl;
        private List<InheritanceNode> subClasses;


        public InheritanceNode(ClassDeclarationSyntax baseClass)
        {
            this.classDecl = baseClass;
            this.subClasses = new List<InheritanceNode>();
        }


        public InheritanceNode(ClassDeclarationSyntax baseClass, List<InheritanceNode> subClasses)
        {
            this.classDecl = baseClass;
            this.subClasses = subClasses;
        }

        public void SetSubClasses(List<InheritanceNode> subClasses)
        {
            this.subClasses = subClasses;
        }

        public ClassDeclarationSyntax GetBaseClass()
        {
            return this.classDecl;
        }

        public List<InheritanceNode> GetSubClasses()
        {
            return this.subClasses;
        }

        public override bool Equals(object obj)
        {
            InheritanceNode testObj = obj as InheritanceNode;

            return testObj.classDecl == this.classDecl
                && testObj.subClasses.SequenceEqual(this.subClasses);
        }

        public override int GetHashCode()
        {
            return (this.classDecl, this.subClasses).GetHashCode();
        }
    }
}

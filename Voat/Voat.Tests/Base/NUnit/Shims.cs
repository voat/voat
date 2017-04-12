using System;
using System.Collections;
using System.Threading.Tasks;

//This is a start of a port to NUnit, the idea was to originally support both NUnit and MSTest, 
//but MSTest is written so wacky it can not be cleanly abstracted without a lot of plumbing code,
//so we are ditching it entirely, and eventually this attribute shim will go away.
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public class TestClassAttribute : NUnit.Framework.TestFixtureAttribute
    {
    }
    public class TestInitializeAttribute : NUnit.Framework.SetUpAttribute
    {
    }
    public class TestMethodAttribute : NUnit.Framework.TestAttribute
    {
    }
    public class IgnoreAttribute : NUnit.Framework.IgnoreAttribute
    {
        public IgnoreAttribute(string reason = null) : base(reason) { }
    }

    public class TestCategoryAttribute : NUnit.Framework.CategoryAttribute
    {
        public TestCategoryAttribute(string name) : base(name) { }
    }

    [Obsolete("Not portable to NUnit", true)]
    public class ExpectedExceptionAttribute : Attribute
    {
        public ExpectedExceptionAttribute(Type expectedType) { }
    }

    public class AssemblyInitializeAttribute : Attribute
    {

    }
    public class AssemblyCleanupAttribute : NUnit.Framework.PostTestAttribute
    {

    }
    public class TestClassInitialize : NUnit.Framework.OneTimeSetUpAttribute
    {

    }

    [Obsolete("Not portable to NUnit", true)]
    public class ClassInitializeAttribute : Attribute
    {

    }

    //public class ExpectedExceptionAttribute : NUnit.Framework.ExpectedExceptionAttribute
    //{
    //    public ExpectedExceptionAttribute(Type exceptionType) : this(exceptionType, null)
    //    {
    //    }
    //    public ExpectedExceptionAttribute(Type exceptionType, string message) : base(exceptionType)
    //    {
    //        UserMessage = message;
    //    }
    //}
    public class TestContext : NUnit.Framework.TestContext
    {
        public TestContext(IDictionary dictionary) : base(new NUnit.Framework.Internal.TestExecutionContext())
        {
            //THIS IS WRONG
        }
    }

    public class Assert : NUnit.Framework.Assert
    {
        
    }
}


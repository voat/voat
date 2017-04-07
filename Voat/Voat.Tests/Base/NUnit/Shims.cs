using System;
using System.Collections;


#if NUNIT

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public class Placeholder { }
    public class TestClassAttribute : NUnit.Framework.TestFixtureAttribute
    {
    }
    public class TestInitializeAttribute : NUnit.Framework.SetUpAttribute
    {
    }
    public class TestMethodAttribute : NUnit.Framework.TestAttribute
    {
    }
    public class TestCleanupAttribute : NUnit.Framework.TearDownAttribute
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
    [Obsolete("Not portable to NUnit")]
    public class ExpectedExceptionAttribute : System.Attribute
    {
        public ExpectedExceptionAttribute(Type expectedType) { }
    }

    public class AssemblyInitializeAttribute : NUnit.Framework.PreTestAttribute
    {

    }
    public class AssemblyCleanupAttribute : NUnit.Framework.PostTestAttribute
    {

    }
    public class ClassInitializeAttribute : NUnit.Framework.OneTimeSetUpAttribute
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
        public static void IsInstanceOfType(object obj, Type type)
        {
            NUnit.Framework.Assert.IsInstanceOf(type, obj, null);
        }
        public static void IsInstanceOfType(object obj, Type type, string message)
        {
            NUnit.Framework.Assert.IsInstanceOf(type, obj, message);
        }
    }
}

#endif
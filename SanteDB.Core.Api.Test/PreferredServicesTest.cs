using NUnit.Framework;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Signing;
using SanteDB.Core.Services;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{

    // Foo Service
    public interface IFooService : IServiceImplementation
    {
        String Foo();
    }

    // Baz Service
    public interface IBazService : IServiceImplementation
    {
        String Baz();
    }

    // Foo Implementation is the second in the configuration file - but it is not preferred
    public class FooImplementation : IFooService
    {
        public string ServiceName => "Foo";
        public string Foo() => "Foo";
    }

    // Bar is third in the configuration file after foo but it is preferred
    // the IFooService on the constructor is injected with the FooImplementation
    [PreferredService(typeof(IFooService))]
    public class BarImplementation : IFooService
    {
        private readonly IFooService m_foo;
        public BarImplementation(IFooService foo)
        {
            this.m_foo = foo;
        }
        public string ServiceName => "Foo";

        public string Foo() => this.m_foo.Foo() + "Bar";
    }

    // Baz uses the Foo service - it appears first in the configuration file - it is injected with BarService
    public class BazImplementation : IBazService
    {
        private readonly IFooService m_fooService;

        public BazImplementation(IFooService fooService)
        {
            this.m_fooService = fooService;
        }
        public string ServiceName => "Foo";
        public string Baz() => this.m_fooService.Foo() + "Baz";
    }

    /// <summary>
    /// Tests for preferred services
    /// </summary>
    [TestFixture]
    public class PreferredServicesTest
    {


        /// <summary>
        /// Initializes the JWS test
        /// </summary>
        [OneTimeSetUp]
        public void Initialize()
        {
            TestApplicationContext.TestAssembly = typeof(JwsTest).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);
        }


        /// <summary>
        /// Test the creation of JWS content
        /// </summary>
        [Test]
        public void TestPreferredServiceIsPreferred()
        {


            // Bar Implementation replaces Foo
            Assert.IsInstanceOf<BarImplementation>(ApplicationServiceContext.Current.GetService<IFooService>());
            // FooImplementation should just be returned when I ask for it directly
            Assert.AreEqual("Foo", ApplicationServiceContext.Current.GetService<FooImplementation>().Foo());
            // IFooService should be BarImplementation injected with FooImplementation
            Assert.AreEqual("FooBar", ApplicationServiceContext.Current.GetService<IFooService>().Foo());
            // BazImplementation is for IBazService and has been injected with IFooService (which is BarImplementation)
            // which was injected with FooImplementation
            Assert.AreEqual("FooBarBaz", ApplicationServiceContext.Current.GetService<IBazService>().Baz());
            // Injecting 
        }
    }
}

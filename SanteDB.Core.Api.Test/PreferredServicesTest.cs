/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using NUnit.Framework;
using SanteDB.Core.Services;
using SanteDB.Core.TestFramework;
using System;

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
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

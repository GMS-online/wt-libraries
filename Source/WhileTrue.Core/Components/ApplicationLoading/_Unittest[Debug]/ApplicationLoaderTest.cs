﻿// ReSharper disable InconsistentNaming
using NUnit.Framework;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.UnitTesting;
using WhileTrue.Components.ApplicationLoading._Unittest.TestComponents;
using WhileTrue.Facades.ApplicationLoader;
using WhileTrue.Facades.SplashScreen;
using WhileTrue.Facades.SplashScreen._UnittestHelper;

namespace WhileTrue.Components.ApplicationLoading._Unittest
{
    [TestFixture]
    public class ApplicationLoaderTest
    {

        [Test]
        public void application_loader_shall_instanciate_all_modules_and_notify_the_splash_progress_accordingly()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<ApplicationLoader>();
            Repository.AddComponent<SplashScreenMock>();
            Repository.AddComponent<Test1>();
            Repository.AddComponent<Test2>();
            Repository.AddComponent<Test3>();


            using (ComponentContainer ComponentContainer = new ComponentContainer(Repository))
            {

                SplashScreenMock SplashScreen = (SplashScreenMock)ComponentContainer.ResolveInstance<ISplashScreen>();//resolve before because otherwise it is removed after application run and below that a new instance would be created

                ComponentContainer.ResolveInstance<IApplicationLoader>().Run();

                Assert.IsTrue(Test2.RunCalled);

                Assert.IsTrue(SplashScreen.ShowCalled);
                Assert.IsTrue(SplashScreen.HideCalled);

                Assert.AreEqual(2, SplashScreen.StatusTexts.Count);

                AutoIndex AutoIndex = new AutoIndex();
                Assert.AreEqual("Status: 1/2,Test2", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("Status: 2/2,Test1", SplashScreen.StatusTexts[AutoIndex]);
            }
        }
    }
}
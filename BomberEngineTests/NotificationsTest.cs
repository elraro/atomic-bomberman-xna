﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BomberEngine.Core.Events;

namespace BomberEngineTests
{
    [TestClass]
    public class NotificationsTest
    {
        private Notifications notifications;

        [TestMethod]
        public void TestPostImmediately0()
        {
            List<String> result = new List<String>();

            notifications = new Notifications();
            notifications.PostImmediately("name", result);

            Check(result);
        }

        [TestMethod]
        public void TestPostImmediately1()
        {
            List<String> result = new List<String>();

            notifications = new Notifications();
            notifications.Register("name1", Callback1);
            notifications.Register("name1", Callback2);
            notifications.Register("name1", Callback3);

            notifications.PostImmediately("name1", result);

            Check(result, "Callback1", "Callback2", "Callback3");
        }

        [TestMethod]
        public void TestPostImmediately2()
        {
            List<String> result = new List<String>();

            notifications = new Notifications();
            notifications.Register("name1", Callback1);
            notifications.Register("name1", Callback2);
            notifications.Register("name1", Callback3);

            notifications.Register("name2", Callback3);
            notifications.Register("name2", Callback2);
            notifications.Register("name2", Callback1);

            notifications.PostImmediately("name2", result);

            Check(result, "Callback3", "Callback2", "Callback1");
        }

        [TestMethod]
        public void TestPostImmediately3()
        {
            List<String> result = new List<String>();

            notifications = new Notifications();
            notifications.Register("name1", Callback1);
            notifications.Register("name1", Callback2);
            notifications.Register("name1", Callback3);

            notifications.PostImmediately("name2", result);

            Check(result);
        }

        [TestMethod]
        public void TestPostImmediately4()
        {
            List<String> result = new List<String>();

            notifications = new Notifications();
            notifications.Register("name1", Callback1);
            notifications.Register("name1", Callback2);
            notifications.Register("name1", Callback3);

            notifications.Remove(Callback1);
            
            notifications.PostImmediately("name1", result);

            Check(result, "Callback2", "Callback3");
        }

        [TestMethod]
        public void TestPostImmediately5()
        {
            List<String> result = new List<String>();

            notifications = new Notifications();
            notifications.Register("name1", Callback1);
            notifications.Register("name1", Callback2);
            notifications.Register("name1", Callback3);

            notifications.RemoveAll(this);

            notifications.PostImmediately("name1", result);

            Check(result);
        }

        [TestMethod]
        public void TestPostImmediately6()
        {
            List<String> result = new List<String>();

            Dummy dummy = new Dummy();

            notifications = new Notifications();
            notifications.Register("name1", Callback1);
            notifications.Register("name1", dummy.Callback1);
            notifications.Register("name1", Callback2);
            notifications.Register("name1", dummy.Callback2);
            notifications.Register("name1", Callback3);
            notifications.Register("name1", dummy.Callback3);

            notifications.RemoveAll(this);

            notifications.PostImmediately("name1", result);

            Check(result, "Dummy1", "Dummy2", "Dummy3");
        }

        private void Callback1(Notification notification)
        {
            List<String> result = notification.data as List<String>;
            result.Add("Callback1");
        }

        private void Callback2(Notification notification)
        {
            List<String> result = notification.data as List<String>;
            result.Add("Callback2");
        }

        private void Callback3(Notification notification)
        {
            List<String> result = notification.data as List<String>;
            result.Add("Callback3");
        }

        private void Check(List<String> result, params String[] values)
        {
            Assert.AreEqual(result.Count, values.Length);
            for (int i = 0; i < values.Length; ++i)
            {
                Assert.AreEqual(result[i], values[i]);
            }
        }
    }

    class Dummy
    {
        public void Callback1(Notification notification)
        {
            List<String> result = notification.data as List<String>;
            result.Add("Dummy1");
        }

        public void Callback2(Notification notification)
        {
            List<String> result = notification.data as List<String>;
            result.Add("Dummy2");
        }

        public void Callback3(Notification notification)
        {
            List<String> result = notification.data as List<String>;
            result.Add("Dummy3");
        }
    }
}

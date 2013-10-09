using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.ObjectModel
{
    [TestClass]
    public class PagedCollectionFixture
    {
        // *** Constructor Tests ***

        [TestMethod]
        public void PagedCollection_Constructor_Void_CollectionIsEmpty()
        {
            PagedList<int> list = new PagedList<int>();

            Assert.AreEqual(0, list.Count);
        }

        // *** Property Tests ***

        [TestMethod]
        public void PagedCollection_Indexer_SettingAndGettingReturnsSameElement_StartOfPage()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[10] = 42;

            Assert.AreEqual(42, list[10]);
        }

        [TestMethod]
        public void PagedCollection_Indexer_SettingAndGettingReturnsSameElement_MiddleOfPage()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[15] = 42;

            Assert.AreEqual(42, list[15]);
        }

        [TestMethod]
        public void PagedCollection_Indexer_SettingAndGettingReturnsSameElement_EndOfPage()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[19] = 42;

            Assert.AreEqual(42, list[19]);
        }

        [TestMethod]
        public void PagedCollection_Indexer_Getter_ReturnsPlaceholderBeforeSetElement()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[10] = 42;

            Assert.AreEqual(0, list[9]);
        }

        [TestMethod]
        public void PagedCollection_Indexer_Getter_ReturnsPlaceholderAfterSetElement()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[10] = 42;

            Assert.AreEqual(0, list[11]);
        }

        [TestMethod]
        public void PagedCollection_Indexer_Getter_ReturnsPlaceholderFarAfterSetElement()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(2000, 100);
            list[10] = 42;

            Assert.AreEqual(0, list[1990]);
        }

        [TestMethod]
        public void PagedCollection_IsReadOnly_IsFalse()
        {
            PagedList<int> list = new PagedList<int>();

            Assert.AreEqual(false, list.IsReadOnly);
        }

        [TestMethod]
        public void PagedCollection_PageCacheSize_IsInitiallyMaxValue()
        {
            PagedList<int> list = new PagedList<int>();

            Assert.AreEqual(int.MaxValue, list.PageCacheSize);
        }

        [TestMethod]
        public void PagedCollection_PageCacheSize_SetterSetsValue()
        {
            PagedList<int> list = new PagedList<int>();

            list.PageCacheSize = 5;

            Assert.AreEqual(5, list.PageCacheSize);
        }

        [TestMethod]
        public void PagedCollection_Add_AddsNewItemToList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);

            list.Add(123);

            Assert.AreEqual(23, list.Count);
            Assert.AreEqual(123, list[22]);
        }

        [TestMethod]
        public void PagedCollection_Clear_ClearsTheList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);

            list.Clear();

            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void PagedCollection_Contains_ReturnsTrueIfItemInList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            bool result = list.Contains(42);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void PagedCollection_Contains_ReturnsFalseIfNotInList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            bool result = list.Contains(15);

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void PagedCollection_CopyTo_CopiesToDestinationArray()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 3);
            list[2] = 8;
            list[10] = 42;

            int[] destination = new int[27];
            destination[1] = 1;
            destination[5] = 2;
            destination[24] = 3;

            list.CopyTo(destination, 2);

            CollectionAssert.AreEqual(new int[] { 0, 1, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0 }, destination);
        }

        [TestMethod]
        public void PagedCollection_IndexOf_ReturnsIndexOfItemInList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            int result = list.IndexOf(42);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void PagedCollection_IndexOf_ReturnsMinusOneIfNotInList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            int result = list.IndexOf(15);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void PagedCollection_Insert_InsertsItemIntoList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(95, 10);
            list[1] = 8;
            list[95] = 42;

            list.Insert(4, 123);

            Assert.AreEqual(21, list.Count);
            Assert.AreEqual(8, list[2]);
            Assert.AreEqual(123, list[5]);
            Assert.AreEqual(42, list[11]);
        }

        [TestMethod]
        public void PagedCollection_Remove_RemovesItemFromList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            list.Remove(10);

            Assert.AreEqual(19, list.Count);
            Assert.AreEqual(8, list[2]);
            Assert.AreEqual(42, list[9]);
        }

        [TestMethod]
        public void PagedCollection_Remove_ReturnsTrueForItemInList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            bool result = list.Remove(10);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void PagedCollection_Remove_ReturnsFalseForItemNotInList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            bool result = list.Remove(12);

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void PagedCollection_RemoveAt_RemovesItemFromList()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            list.RemoveAt(5);

            Assert.AreEqual(19, list.Count);
            Assert.AreEqual(8, list[2]);
            Assert.AreEqual(42, list[9]);
        }

        [TestMethod]
        public void PagedCollection_UpdateCount_UpdatesCountProperty()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);

            Assert.AreEqual(22, list.Count);
        }

        [TestMethod]
        public void PagedCollection_UpdateCount_UpdatesPageSizeProperty()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);

            Assert.AreEqual(10, list.PageSize);
        }

        [TestMethod]
        public void PagedCollection_UpdateCount_ChangingPageNumber_PreservesExistingValues()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);

            list[15] = 42;

            list.UpdateCount(50, 10);

            Assert.AreEqual(42, list[15]);
        }

        [TestMethod]
        public void PagedCollection_UpdateCount_ChangingPageSize_ClearsExistingValues()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 10);

            list[12] = 42;

            list.UpdateCount(22, 5);

            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(0, list[i]);
        }

        [TestMethod]
        public void PagedCollection_GetEnumerator_Generic_EnumeratesArray()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 3);
            list[2] = 8;
            list[10] = 42;

            List<int> result = new List<int>();

            IEnumerator<int> enumerator = ((IEnumerable<int>)list).GetEnumerator();

            while (enumerator.MoveNext())
                result.Add(enumerator.Current);

            CollectionAssert.AreEqual(new int[] { 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, result);
        }

        [TestMethod]
        public void PagedCollection_GetEnumerator_NonGeneric_EnumeratesArray()
        {
            PagedList<int> list = new PagedList<int>();
            list.UpdateCount(22, 3);
            list[2] = 8;
            list[10] = 42;

            List<int> result = new List<int>();

            IEnumerator enumerator = ((IEnumerable)list).GetEnumerator();

            while (enumerator.MoveNext())
                result.Add((int)enumerator.Current);

            CollectionAssert.AreEqual(new int[] { 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, result);
        }

        // *** Behaviour Tests ***

        [TestMethod]
        public void PagedCollection_WithPageCacheSize_SettingMultipleItemsInSinglePage_CachesAllValues()
        {
            PagedList<int> list = new PagedList<int>() { PageCacheSize = 3 };
            list.UpdateCount(50, 10);

            list[11] = 5;
            list[12] = 10;
            list[13] = 15;
            list[14] = 20;
            list[15] = 25;
            list[16] = 30;
            list[17] = 35;

            Assert.AreEqual(5, list[11]);
            Assert.AreEqual(10, list[12]);
            Assert.AreEqual(15, list[13]);
            Assert.AreEqual(20, list[14]);
            Assert.AreEqual(25, list[15]);
            Assert.AreEqual(30, list[16]);
            Assert.AreEqual(35, list[17]);
        }

        [TestMethod]
        public void PagedCollection_WithPageCacheSize_SettingValues_ClearsLeastRecentPages()
        {
            PagedList<int> list = new PagedList<int>() { PageCacheSize = 3 };
            list.UpdateCount(50, 10);

            list[15] = 5;
            list[25] = 10;
            list[35] = 15;
            list[45] = 20;
            list[10] = 25;

            Assert.AreEqual(25, list[10]);
            Assert.AreEqual(0, list[15]);
            Assert.AreEqual(0, list[25]);
            Assert.AreEqual(15, list[35]);
            Assert.AreEqual(20, list[45]);
        }

        [TestMethod]
        public void PagedCollection_WithPageCacheSize_GettingValues_PushesPageUpInRecentlyUsedPagesList()
        {
            PagedList<int> list = new PagedList<int>() { PageCacheSize = 3 };
            list.UpdateCount(50, 10);

            list[15] = 5;
            list[25] = 10;
            list[35] = 15;
            list[45] = 20;
            int item25 = list[25];
            list[10] = 25;

            Assert.AreEqual(25, list[10]);
            Assert.AreEqual(0, list[15]);
            Assert.AreEqual(10, list[25]);
            Assert.AreEqual(0, list[35]);
            Assert.AreEqual(20, list[45]);
        }
    }
}

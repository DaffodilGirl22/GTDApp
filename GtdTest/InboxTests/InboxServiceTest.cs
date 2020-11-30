using Xunit;
using GTDApp.Models;
using GTDApp.Services;
using System;
using GTDApp.Data;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class InboxServiceTest : DbTest
    {



        // "GetData" lists all expected cases, which are tested in turn
        [Theory]
        [MemberData(nameof(GetData))]
        public void GetAll_ReturnsAllInboxes(int id, string item, params object[] dates)
        {
            // Check any parameter dates are valid
            var inboxDates = new List<DateTime?>() { null, null };
            if (dates.Length > 0)
            {
                int idx = 0;
                try
                {
                    for (; idx < dates.Length; idx++)
                        inboxDates[idx] = DateTime.Parse(dates[idx].ToString());
                }
                catch (FormatException)
                {
                    Console.WriteLine("Unable to convert input date: {0}", dates[idx]);
                }
            }

            // Arrange
            var inboxServ = new InboxService(MakeInMemoryContext());

            // Act 
            var inboxList = inboxServ.GetAll().Result;

            // Assert
            Assert.Equal(IdList.Count(), inboxList.Count());

            var result = inboxList.Where(a => a.Id == id);
            var count = result.Count();

            // Expected: 1 inbox object with current case ID
            Assert.Equal(1, count);
            var actual = result.First();
            Assert.Equal(id, actual.Id);
            Assert.Equal(item, actual.Item);
            if (inboxDates[0] != null) Assert.Equal(inboxDates[0], actual.CreateTime);
            if (inboxDates[1] != null) Assert.Equal(inboxDates[1], actual.ModifyTime);
        }



        // "GetDataById" lists data for 1 valid inbox & 2 invalid inboxes
        [Theory]
        [MemberData(nameof(GetDataById))]
        public async void GetById_ReturnsCorrectInbox(int id, string item, string date, bool doesExist)
        {
            DateTime? createDate = null;
            // Parse Date
            try
            {
                createDate = DateTime.Parse(date.ToString());
            }
            catch (Exception)
            {
                Console.WriteLine("Error: converting input create date: {0}", date);
            }

            //Arrange
            var inboxServ = new InboxService(MakeInMemoryContext());

            // Act
            var actual = await inboxServ.GetById(id);

            // Assert
            if (doesExist)
            {
                Assert.NotNull(actual);
                Assert.Equal(id, actual.Id);
                Assert.Equal(item, actual.Item);
                Assert.Equal(createDate, actual.CreateTime);
            }
            else
            {
                Assert.Null(actual);
            }
        }



        // Ensure only valid inboxes are created and that their IDs
        // do not replicate any existing test inboxes.
        [Theory]
        [InlineData("Do Something Difficult", true)]
        [InlineData("Extra Thing", true)]
        [InlineData("", false)]
        public async void CreateInbox_ReturnsInbox(string item, bool ok)
        {
            // Arrange
            var inboxServ = new InboxService(MakeInMemoryContext());
            var newInbox = new Inbox() { Item = item };
            var existingIds = IdList;

            // Act
            var actual = await inboxServ.Create(newInbox);
            var now = DateTime.UtcNow;

            // Assert
            if (ok)
            {
                Assert.NotNull(actual);
                // Assert.All(existingIds, id => Assert.NotEqual(id, actual.Id));
                Assert.False(existingIds.Contains(actual.Id), "Error: Inbox Id already exists");
                Assert.NotEqual(0, actual.Id);
                Assert.Equal(item, actual.Item);
                Assert.True(CheckSecondTimeDiff(now, actual.CreateTime, 100));
                Assert.Null(actual.ModifyTime);
            }
            else
            {
                Assert.Null(actual);
            }
        }


        // The inbox service only requires the Inbox "item". The expectation is
        // that the inbox service or context will generate the other properties,
        // so the expectation is that other input properties will be ignored.
        [Theory]
        [InlineData(1, "New Task")]
        [InlineData(2, "Extra Thing", "01/05/2020 13:00:00", "14/06/2020 13:00:00")]
        [InlineData(3, "Do This", "21/08/2020 14:50:00")]
        public async void CreateIgnoresEverythingExceptItem(int id, string item,
                                                             params string[] dates)
        {
            // Check input parameter dates are valid
            var inboxDates = new List<DateTime?>() { null, null };
            if (dates.Length > 0)
            {
                int idx = 0;
                try
                {
                    for (; idx < dates.Length; idx++)
                        inboxDates[idx] = DateTime.Parse(dates[idx]);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Unable to convert string to date: {0}", dates[idx]);
                }
            }

            // Arrange
            var inboxServ = new InboxService(MakeInMemoryContext());
            var existingIds = IdList;

            var inbox = new Inbox() { Id = id, Item = item };
            inbox.CreateTime = inboxDates[0];
            inbox.ModifyTime = inboxDates[1];

            // Act
            var actual = await inboxServ.Create(inbox);
            var now = DateTime.UtcNow;

            // Assert
            Assert.NotNull(actual);
            if (existingIds.Contains(inbox.Id)) Assert.NotEqual(inbox.Id, actual.Id);
            Assert.False(existingIds.Contains(actual.Id), "Error: Inbox Id already exists");
            // Assert.All(existingIds, id => Assert.NotEqual(id, actual.Id));
            Assert.Equal(item, actual.Item);
            Assert.True(CheckSecondTimeDiff(now, actual.CreateTime, 100));
            Assert.Null(actual.ModifyTime);
        }


        // "GetUpdateData" supplies 4 valid & 2 invalid inbox updates
        [Theory]
        [MemberData(nameof(GetUpdateData))]
        public async void Update_ReturnsUpdatedInbox(int id, string item, params string[] dates)
        {
            // Check any parameter dates are valid
            var inboxDates = new List<DateTime?>() { null, null };
            if (dates.Length > 0)
            {
                int idx = 0;
                try
                {
                    for (; idx < dates.Length; idx++)
                        inboxDates[idx] = DateTime.Parse(dates[idx]);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Error: converting input date: {0}", dates[idx]);
                }
            }
          
            // Arrange
            var inboxServ = new InboxService(MakeInMemoryContext());

            var inbox = new Inbox() { Id = id, Item = item };
            if (inboxDates[0] != null) inbox.CreateTime = inboxDates[0];
            if (inboxDates[1] != null) inbox.ModifyTime = inboxDates[1];

            // Parse expected data
            Inbox expected = null;
            var res = InboxList.Where<Inbox>(x => x.Id == id).ToList();
            if (res.Count() == 1) expected = res.First();

            if (expected != null && !string.IsNullOrWhiteSpace(item))
            {
                expected.Item = item;
                expected.ModifyTime = DateTime.UtcNow;
            }

            // Act
            var updates = await inboxServ.Update(inbox);

            // Did data context get updated as expected
            var actual = await inboxServ.GetById(id);

            // Assert
            if (IdList.Contains(id))
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Item, actual.Item);
                Assert.Equal(expected.CreateTime, actual.CreateTime);
                Assert.True(CheckSecondTimeDiff(expected.ModifyTime, actual.ModifyTime, 100));
            }
            else
            {
                Assert.Null(updates);
                Assert.Null(actual);
            }
        }



        // "GetDeleteIds" supplies 2 valid & 2 invalid inbox Ids
        [Theory]
        [MemberData(nameof(GetDeleteIds))]
        public async void Test_Delete(int id)
        {
            // Arrange
            var inboxServ = new InboxService(MakeInMemoryContext());

            // Act
            var result = await inboxServ.Delete(id);

            // Assert
            if (IdList.Contains(id))
            {
                Assert.True(result);
            }
            else
            {
                Assert.False(result);
            }
        }



        /*// POPULATE the DATA CONTEXT  //*/
        protected override void SeedDatabase(DatabaseContext context)
        {
            InboxList = new List<Inbox>();
            foreach (var row in GetData())
            {
                Inbox inbox = new Inbox();
                try
                {
                    inbox.Id = (int)row[0];
                    inbox.Item = (string)row[1];
                    if (row.Length >= 3)
                        inbox.CreateTime = DateTime.Parse(row[2].ToString());
                    if (row.Length == 4)
                        inbox.ModifyTime = DateTime.Parse(row[3].ToString());
                }
                catch (Exception e)
                {
                    throw new Exception("Error:\n{0}", e);
                }
                context.Inbox.Add(inbox);
                InboxList.Add(inbox);
            }
            context.SaveChanges();
        }

        /*// TEST DATA SCENARIOS  //*/
        private static int[] IdList = new int[] { };
        private static List<Inbox> InboxList = new List<Inbox>();

        
        // Two main purposes:
        // 1) Supply data to populate data context;
        // 2) Supply test data for "GetAll".
        public static IEnumerable<object[]> GetData()
        {
            // <Inbox> data: "Id", "Item", "CreateDate", "ModifyDate"
            // Assumption DB data will always have:
            //    a "CreateDate", but may not have a "ModifyDate".
            var inboxData = new List<object[]>
            {
                new object[] { 1, "Task One", "02/05/2020 13:00:00","12/07/2020 14:00:00"},
                new object[] { 2, "Task 2", "21/04/2020 10:30:00" },
                new object[] { 3, "Task Three", "25/06/2020 10:30:00", "25/06/2020 10:30:00"},
                new object[] { 5, "Simple Task", "17/02/2020 16:40:00" },
            };

            // Initialise private IdList array
            IdList = new int[] { 1, 2, 3, 5 };

            return inboxData;
        }


        // Supplies test data for "Update"
        // Includes 6 cases: 3 valid and 3 invalid scenarios
        public static IEnumerable<object[]> GetUpdateData()
        {
            Random rnd = new Random();
            if (IdList.Count() < 3) throw new Exception("Error: insufficient test data");
            var ids = IdList.OrderBy(x => rnd.Next()).Take(4).ToList<int>();
            ids.Add(0);
            ids.Add(IdList.Count() + 3);

            var inboxData = new List<object[]>
            {
                new object[] { ids[0], "Complex Task"},
                new object[] { ids[1], ""},
                new object[] { ids[2], "Sort Out Stuff", "21/02/2020 10:30:00" },
                new object[] { ids[3], "Dig Up Tree", "25/09/2020 10:30:00", "25/10/2020 10:30:00"},
                new object[] { ids[4], "Extra Task", "17/03/2020 16:40:00" },
                new object[] { ids[5], "Impossible Job" }
            };

            return inboxData;
        }


        // Supplies test data for "GetById"
        // Includes 3 cases: 1 valid and 2 invalid scenarios
        public static IEnumerable<object[]> GetDataById()
        {
            var data = GetData();
            var itemList = new List<object[]>();

            // Select one correct ID
            var rand = new Random();
            var item = data.ElementAt(rand.Next(1, data.Count()));
            itemList.Add(new object[] { item[0], item[1], item[2], true });

            // Check list has one item
            if (itemList.Count() != 1)
                throw new Exception("Error: selecting a valid test inbox");

            // Add 2 non-existent Ids to check
            var now = DateTime.Now.ToString();
            itemList.Add(new object[] { 0, "Empty Task", now, false });
            itemList.Add(new object[] { IdList.Max() + 2, "Non Task", now, false });

            return itemList;
        }


        // Supplies test data for "Delete"
        // Includes 4 cases: 2 valid and 2 invalid Ids
        public static IEnumerable<object[]> GetDeleteIds()
        {
            var itemList = new List<object[]>();
            var ids = new List<int>() { 0, IdList.Max() + 5 };
            ids.AddRange(IdList.TakeLast(2));
            foreach (var id in ids) itemList.Add(new object[] { id });

            return itemList;
        }




            /*// TEST CLASS UTILITIES //*/

            // Check time difference between 2 specified times is not greater than
            // expected "seconds"
            private bool CheckSecondTimeDiff(DateTime? t1, DateTime? t2, int seconds)
        {
            if (t1 == null && t2 == t1) return true;
            else if (t1 == null || t2 == null) return false;

            DateTime d1 = t1.GetValueOrDefault(),
                     d2 = t2.GetValueOrDefault();
            return seconds > Math.Abs(d1.Subtract(d2).TotalSeconds);
        }


      



        /* Retrieve IDs from list of test inbox data
        public static int[] GetIdList()
        {
            var data = GetData();
            // Enumerable.Range(<start index>, <how many>)
            var idList = Enumerable.Range(0, data.Count())
                         .Select(x => (int)data.ElementAt(x)[0]).ToArray();
            return idList;
        }
        */

    }
}

using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using System.Collections.Generic;
using GTDApp.Controllers;
using GTDApp.Models;
using GTDApp.Services;
using System;

namespace Tests
{
    // Used to test the web service, not the database,
    // therefore MOQ data is used.
    public class InboxControllerTest
    {
        readonly private Mock<IInboxService> _inboxService;
        readonly private InboxController _inboxController;

        public InboxControllerTest()
        {
            _inboxService = new Mock<IInboxService>();
            _inboxController = CreateInboxController(_inboxService.Object);
        }

        [Fact]
        public void GetAllInboxesTest()
        {
            // Arrange
            List<Inbox> expected = new List<Inbox>(new Inbox[] {
                new Inbox { Id = 1, Item = "Task 1" },
                new Inbox { Id = 2, Item = "Task 2" },
                new Inbox { Id = 3, Item = "Task C"}
                });

            _inboxService.Setup(_ => _.GetAll()).ReturnsAsync(expected);

            // Act
            OkObjectResult result = _inboxController.Get().Result as OkObjectResult;

            // Assert
            // Will be null if result wasn't an OkObjectResult
            Assert.NotNull(result);
            Assert.Equal(expected, result.Value);
        }

        /*
         * I think that separating out tests by return type makes
         * the code cleaner and easier to follow
         * 
         * The next two methods are an example of how 
         * GetInboxByIdTest could be split into two test functions
         */

        [Theory]
        [InlineData(1, "Task A")]
        public void GetInboxByIdOk(int id, string item)
        {
            // Arrange
            Inbox expected = new Inbox() { Id = id, Item = item };
            _inboxService.Setup(r => r.GetById(id)).ReturnsAsync(expected);

            // Act
            var result = _inboxController.Get(id).Result;

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Inbox inbox = Assert.IsType<Inbox>(okResult.Value);

            Assert.Equal(id, inbox.Id);
            Assert.Equal(item, inbox.Item);
        }

        [Theory]
        [InlineData(4)]
        public void GetInboxByIdNotFound(int id)
        {
            // Arrange
            Inbox expected = null;
            _inboxService.Setup(r => r.GetById(id)).ReturnsAsync(expected);

            // Act
            var result = _inboxController.Get(id).Result;

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
        }

        [Theory]
        [InlineData(1, true, "Task A")]
        [InlineData(4, false)]               // Test where NO Inbox with "id" exists
        public void GetInboxByIdTest(int id, bool doesExist, params string[] item)
        {
            // Also expected result for Inbox with ID not existing
            Inbox expected = null;

            // Arrange
            if (doesExist)
            { 
                if (item.Length == 1)
                {
                    // Inbox has specified "Item"
                    expected = new Inbox(item[0]) { Id = id };
                }
                else
                    throw new Exception("Test Data Error: invalid <inline> data");
            }

            _inboxService.Setup(r => r.GetById(id)).ReturnsAsync((Inbox)expected);

            // Act
            // Note: without ".Result" a task is returned, rather than an "IActionResult"
            var result = _inboxController.Get(id).Result;

            // Assert
            if (!doesExist)
            {
                // No Inbox found with specified ID
                var notFoundResult = Assert.IsType<NotFoundResult>(result);
                Assert.NotNull(notFoundResult);
            }
            else
            {
                // Object with ID DOES exist
                var okResult = Assert.IsType<OkObjectResult>(result);
                Inbox inbox = Assert.IsType<Inbox>(okResult.Value);

                Assert.Equal(id, inbox.Id);
                Assert.Equal(item[0], inbox.Item);
            }
        }


        [Theory]
        [InlineData(1, "Task 1")]    // Add Inbox with specified Item
        [InlineData(3, "")]          // Add Inbox with empty Item
        [InlineData(5)]              // Add Inbox with specified Id only
        public void PostInboxTest(int id, params string[] item)
        {
            if (item.Length > 1)
                throw new Exception("Test Data Error: invalid test data");

            // ** Arrange
            bool itemValid = item.Length == 1 && !string.IsNullOrWhiteSpace(item[0]);

            // Input Inbox
            Inbox input = new Inbox() { Id = id };
            if (item.Length == 1) input.Item = item[0];

            // Expected inbox
            DateTime now = DateTime.Now;
            Inbox expectedInbox = new Inbox()
            {
                Id = id,
                CreateTime = now,
                ModifyTime = now
            };

            if (itemValid) expectedInbox.Item = item[0];
            else expectedInbox = null;

            // Create mock result to be used in "_inboxController" statement below
            _inboxService.Setup(i => i.Create(input)).ReturnsAsync((Inbox)expectedInbox);

            // ** Act
            IActionResult response = _inboxController.Post(input).Result;

            // ** Assert
            if (itemValid)
            {
                CreatedAtRouteResult result = Assert.IsType<CreatedAtRouteResult>(response);

                Assert.Equal("inbox", result.RouteName);
                Assert.Equal(id, result.RouteValues["id"]);

                Inbox inbox = Assert.IsType<Inbox>(result.Value);
                Assert.Equal(expectedInbox, inbox);
            }
            else
            {
                Assert.IsType<BadRequestResult>(response);
            }
        }


        [Theory]
        [InlineData(1, true, "Task One")]     // Update Inbox: "Task 1" => "Task One"
        [InlineData(3, true, "")]             // Update Inbox: "Task 3" => ""
        [InlineData(5, false, "New Task")]    // Attempt to Update non-existent inbox
        [InlineData(7, false)]                // Update non-existent inbox with no changes
        public void UpdateInboxTest(int id, bool doesExist, params string[] item)
        {
            if (item.Length > 1)
                throw new Exception("Test Data Error: invalid test data");

            // ** Arrange

            // Input Inbox
            Inbox input = new Inbox() { Id = id };
            if (item.Length == 1) input.Item = item[0];

            // Expected Inbox
            bool itemValid = item.Length == 1 && !string.IsNullOrWhiteSpace(item[0]);
            Inbox expected = null;
            if (doesExist && itemValid)
            {
                expected = new Inbox
                {
                    Id = id,
                    Item = item[0],
                    ModifyTime = DateTime.Now,
                    CreateTime = DateTime.Now.AddDays(-10)
                };
            }
           
            // Create mock result to be used in "_inboxController" statement below
            _inboxService.Setup(i => i.Update(input)).ReturnsAsync((Inbox)expected);

            // ** Act
            IActionResult response = _inboxController.Put(input).Result;

            // ** Assert
            if (itemValid && doesExist)
            {
                var result = Assert.IsType<OkObjectResult>(response);
                Inbox inbox = Assert.IsType<Inbox>(result.Value);
                Assert.Equal(expected, inbox);
                Assert.NotEqual(input.CreateTime, inbox.CreateTime);
                Assert.Equal(input.Item, inbox.Item);
                Assert.Equal(input.Id, inbox.Id);
            }
            else
            {
                Assert.IsType<BadRequestResult>(response);
            }
        }


        [Theory]
        [InlineData(12, true)]
        [InlineData(41, false)]
        public void DeleteInboxTest (int id, bool doesExist)
        {
            // Arrange
            bool expected = doesExist;
            _inboxService.Setup(r => r.Delete(id)).ReturnsAsync(expected);

            // Act
            var result = _inboxController.Delete(id).Result;

            // Assert
            if (doesExist)
            {
                // No Inbox found with specified ID
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.NotNull(noContentResult);
            }
            else
            {
                // No Inbox found with specified ID
                var notFoundResult = Assert.IsType<NotFoundResult>(result);
                Assert.NotNull(notFoundResult);
            }
        }

        private InboxController CreateInboxController(IInboxService inboxService)
        {
            Mock<IServiceContext> serviceContext = new Mock<IServiceContext>();
            serviceContext.SetupGet(i => i.InboxServ).Returns(inboxService);

            return new InboxController(serviceContext.Object);
        }

    }
}

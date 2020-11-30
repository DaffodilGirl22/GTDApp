using System.Collections.Generic;
using System.Linq;
using GTDApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace GTDApp.Controllers
{
    [Route("api/inbox")]
    [ApiController]
    public class InboxControllerOld : ControllerBase
    {
        static readonly List<Inbox> InboxList = new List<Inbox>();
        private static bool Init = false;

        public InboxControllerOld()
        {
            // Run every time there is Http Action
            if (!Init) Init = InitialiseInboxList();
        }


        // GET: "{URL host}/inbox"
        [HttpGet]
        public IEnumerable<Inbox> GetInbox()
        {
            return InboxList;
        }


        // GET: "{URL host}/inbox/{id}"
        [HttpGet("{id}")]
        public ActionResult GetInboxById(int id)
        {
            var idx = InboxList.IndexOf(InboxList.Where(i => i.Id == id).FirstOrDefault());
            if (idx < 0 || idx >= InboxList.Count()) return NotFound();
            return Ok(InboxList[idx]);
        }


        // Add new Inbox Item
        // POST: {URL host}/inbox"
        [HttpPost]
        public ActionResult CreateInbox(Inbox inboxItem)
        {
            if (string.IsNullOrEmpty(inboxItem.Item))
                return BadRequest("Error: Inbox Item is not defined");
            else
            {
                InboxList.Add(inboxItem);
                return Ok(inboxItem);
            }
        }


        // Replace Inbox Item
        // PUT {URL host}/inbox/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateInbox(int id, Inbox item)
        {
            var idx = InboxList.IndexOf(InboxList.Where(i => i.Id == id).FirstOrDefault());
            if (idx < 0 || idx >= InboxList.Count()) return NotFound();

            InboxList[idx].Item = item.Item;
            InboxList[idx].User = item.User;

            return Ok(InboxList[idx]);
        }


        // DELETE {URL host}/api/inbox/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteInbox(int id)
        {
            var idx = InboxList.IndexOf(InboxList.Where(i => i.Id == id).FirstOrDefault());
            if (idx < 0 || idx >= InboxList.Count()) return NotFound();

            var item = InboxList[idx];
            InboxList.RemoveAt(idx);
           
            return Ok(item);
        }


        private bool InitialiseInboxList()
        {
            InboxList.Add(new Inbox("Post Letter", "Gill"));
            InboxList.Add(new Inbox("Phone Fred", "Bob"));
            InboxList.Add(new Inbox("Clean Fridge"));
            InboxList.Add(new Inbox("Write up finances", "Accountant"));
            return true;
        }

        /* REST API protocol does not have a concept of "PATCH" therefore
         * this particular method is commented out.
         *
        */
        //// PATCH {URL host}/api/inbox/{id}
        //[HttpPatch("{id}")]
        //public ActionResult PatchInbox(int id, InboxItem item)
        //{
        //    var idx = Inbox.IndexOf(Inbox.Where(i => i.Id == id).FirstOrDefault());
        //    if (idx < 0 || idx >= Inbox.Count()) return NotFound();

        //    Inbox[idx].Item = item.Item ?? Inbox[idx].Item;
        //    Inbox[idx].User = item.User ?? Inbox[idx].User;
        //    return Ok();
        //}

    }
}

/*
 * Success:

    return Ok() ← Http status code 200
    return Created() ← Http status code 201
    return NoContent(); ← Http status code 204

    Client Error:

    return BadRequest(); ← Http status code 400
    return Unauthorized(); ← Http status code 401
    return NotFound(); ← Http status code 404
*/

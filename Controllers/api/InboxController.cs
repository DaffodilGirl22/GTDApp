using GTDApp.Models;
using GTDApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GTDApp.Controllers
{
    [Route("api/inbox")]
    [ApiController]
    public class InboxController : ControllerBase
    {
        private readonly IServiceContext _context;

        public InboxController(IServiceContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.InboxServ.GetAll());
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var inbox = await _context.InboxServ.GetById(id);
            if (inbox != null)
                return Ok(inbox);
            else
                return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> Post(Inbox inbox)
        {
            // TODO: can inbox be null?
            // TODO: can Create fail due to validation failure (empty Inbox text)?
            // TODO: do we need to handle DbUpdateException and return something Not Ok?
            var newInbox = await _context.InboxServ.Create(inbox);

            if (newInbox == null)
            {
                return BadRequest();
            }
            
            return CreatedAtRoute("inbox", new { id = newInbox.Id }, newInbox);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Inbox inbox)
        {
            // TODO: what if Update fails (id doesn't exist, empty text)
            // TODO: do we need to handle DbUpdateException and return something Not Ok?

            var updatedInbox = await _context.InboxServ.Update(inbox);
            if (updatedInbox == null)
            {
                return BadRequest();
            }
            return Ok(updatedInbox);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // TODO: do we need to handle DbUpdateException and return something Not Ok?
            var ok = await _context.InboxServ.Delete(id);
            if (ok) return NoContent();
            else return NotFound();
        }
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


using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using GTDApp.Data;
using GTDApp.Models;
using System.Threading.Tasks;

/**
 * Business logic for Inbox Items
 * Uses EF as the Repository layer
 * 
 * There is much internet debate about the utility of adding
 * own Repository pattern over EF. If we decide that is necessary
 * then we can easily add that layer here.
 * 
 * TODO: Validation prior to insert/update
 */

namespace GTDApp.Services
{
    
     public class InboxService : IInboxService
    { 
        private readonly DatabaseContext _context;
        public InboxService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Inbox>> GetAll()
        {
            try
            {
                return await _context
                    .Inbox
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public async Task<Inbox> GetById(int id)
        {
            try
            {
                Task<Inbox> inboxTask = _context.Inbox
                    .AsNoTracking()
                    .SingleAsync(i => i.Id == id);

                Inbox inbox = await inboxTask;

                return inbox;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }


        public async Task<Inbox> Create(Inbox inbox)
        {
            var success = false;
            var newInbox = new Inbox(inbox.Item);
            newInbox.CreateTime = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(inbox.Item))
            {
                _context.Inbox.Add(newInbox);
                success = await SaveDbChanges();
            }

            return success ? newInbox : null;
        }


        public async Task<Inbox> Update(Inbox inbox)
        {
            var success = false;
            var modInbox = new Inbox() { Id = inbox.Id };

            // Only modify the changed properties
            if (inbox.Id > 0 && !string.IsNullOrWhiteSpace(inbox.Item))
            {
                // Attach then modify to only update certain fields
                _context.Inbox.Attach(modInbox);
                modInbox.Item = inbox.Item;
                modInbox.ModifyTime = DateTime.Now;
                success = await SaveDbChanges();
            }

            return success ? modInbox : null;
        }

        public async Task<bool> Delete(int id)
        {
            var inbox = new Inbox() { Id = id };

            _context.Entry(inbox).State = EntityState.Deleted;

            return await SaveDbChanges();
        }


        private async Task<bool> SaveDbChanges()
        {
            bool ok = true;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ok = false;
            }
            return ok;
        }

    }
}

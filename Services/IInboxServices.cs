using System.Collections.Generic;
using System;
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
    public interface IInboxService
    {
        public Task<List<Inbox>> GetAll();

        public Task<Inbox> GetById(int id);

        public Task<Inbox> Create(Inbox inbox);

        public Task<Inbox> Update(Inbox inbox);

        public Task<bool> Delete(int id);

    }
}
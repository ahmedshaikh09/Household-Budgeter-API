using AutoMapper;
using AutoMapper.QueryableExtensions;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseholdBudgeter.Controllers
{
    [RoutePrefix("api/bankaccount")]
    [Authorize]
    public class BankAccountController : ApiController
    {
        private ApplicationDbContext Context;

        public BankAccountController()
        {
            Context = new ApplicationDbContext();
        }

        [Route("house-hold/{id}")]
        public IHttpActionResult Create(int id, BankAccountBindingModel formData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var houseHold = Context
                .HouseHolds
                .FirstOrDefault(p => p.Id == id);

            if (houseHold == null)
            {
                return NotFound();
            }

            var userId = User
                .Identity
                .GetUserId();

            if (userId != houseHold.OwnerId)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can create a Bank Account");
                return BadRequest(ModelState);
            }

            var bankAccount = Mapper.Map<BankAccount>(formData);
            bankAccount.HouseHoldId = id;
            bankAccount.Balance = 0;

            houseHold.BankAccounts.Add(bankAccount);
            Context.SaveChanges();

            var model = Mapper.Map<BankAccountViewModel>(bankAccount);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Put(int id, BankAccountBindingModel formData)
        {
            var bankAccount = Context
                .BankAccounts
                .FirstOrDefault(p => p.Id == id);

            if (bankAccount == null)
            {
                return NotFound();
            }

            var owner = bankAccount
                .HouseHold
                .OwnerId;

            var userId = User
                .Identity
                .GetUserId();

            if (userId != owner)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can edit a Bank Account");
                return BadRequest(ModelState);
            }

            Mapper.Map(formData, bankAccount);
            bankAccount.DateUpdated = DateTime.Now;

            Context.SaveChanges();

            var model = Mapper.Map<BankAccountViewModel>(bankAccount);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var bankAccount = Context
                  .BankAccounts
                  .FirstOrDefault(p => p.Id == id);

            if (bankAccount == null)
            {
                return NotFound();
            }

            var owner = bankAccount.HouseHold.OwnerId;

            var user = User.Identity.GetUserId();
            if (user != owner)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can delete a Bank Account");
                return BadRequest(ModelState);
            }

            Context.BankAccounts.Remove(bankAccount);
            Context.SaveChanges();

            return Ok();
        }

        [Route("house-hold/{id}/get-all")]
        public IHttpActionResult GetAll(int id)
        {
            var houseHold = Context
                .HouseHolds
                .FirstOrDefault(p => p.Id == id);

            if (houseHold == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = User.Identity.GetUserId();

            if (!houseHold.Members.Any(t => t.Id == user) && houseHold.OwnerId != user)
            {
                ModelState.AddModelError("Not a Member", "You are not a part of the requested HouseHold");
                return BadRequest(ModelState);
            }

            var model = Context
                .BankAccounts
                .Where(p => p.HouseHoldId == id)
                .ProjectTo<BankAccountViewModel>()
                .ToList();

            return Ok(model);
        }

        [Route("{id}/calculate")]
        [HttpGet]
        public IHttpActionResult Calculate(int id)
        {
            var bankAccount = Context
                  .BankAccounts
                  .FirstOrDefault(p => p.Id == id);

            if (bankAccount == null)
            {
                return NotFound();
            }

            var owner = bankAccount.HouseHold.OwnerId;

            var user = User.Identity.GetUserId();
            if (user != owner)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can calculate the bank balance");
                return BadRequest(ModelState);
            }

            bankAccount.Balance = 0;
            var total = bankAccount.Transactions.Where(p => p.Void == false).Sum(p => p.Amount);
            bankAccount.Balance = total;

            Context.SaveChanges();

            var model = bankAccount.Balance;

            return Ok(model);
        }
    }
}

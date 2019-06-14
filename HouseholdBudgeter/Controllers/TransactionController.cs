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
    [RoutePrefix("api/transaction")]
    [Authorize]
    public class TransactionController : ApiController
    {
        private ApplicationDbContext Context;

        public TransactionController()
        {
            Context = new ApplicationDbContext();
        }
        [HttpPost]
        [Route("create/{id}")]
        public IHttpActionResult Create(int id, TransactionBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Identity.GetUserId();

            var bankAccount = Context
                .BankAccounts
                .FirstOrDefault(p => p.Id == model.BankAccountId &&
                (p.HouseHold.OwnerId == userId ||
                p.HouseHold.Members.Any(t => t.Id == userId)));

            if (bankAccount == null)
            {
                ModelState.AddModelError("",
                    "Bank account doesn't exist or you don't belong to this household");
                return BadRequest(ModelState);
            }

            var category = Context
                .Categories
                .FirstOrDefault(p => p.Id == model.CategoryId &&
                p.HouseHoldId == bankAccount.HouseHoldId);

            if (category == null)
            {
                ModelState.AddModelError("", "Category doesn't exist in this household");
                return BadRequest(ModelState);
            }

            var transaction = Mapper.Map<Transaction>(model);
            transaction.CreatorId = userId;

            bankAccount.Balance += transaction.Amount;

            Context.Transactions.Add(transaction);
            Context.SaveChanges();

            var result = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(result);
        }

        [Route("edit/{id}")]
        public IHttpActionResult Edit(int id, EditTransactionBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Identity.GetUserId();

            var transaction = Context
                .Transactions
                .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.BankAccount.HouseHold.OwnerId != userId &&
                transaction.CreatorId != userId)
            {
                ModelState.AddModelError("",
                    "You are not allowed to edit this transaction");
                return BadRequest(ModelState);
            }

            var category = Context
                .Categories
                .FirstOrDefault(p => p.Id == model.CategoryId &&
                p.HouseHoldId == transaction.BankAccount.HouseHoldId);

            if (category == null)
            {
                ModelState.AddModelError("", "Category doesn't exist in this household");
                return BadRequest(ModelState);
            }

            if (!transaction.Void)
            {
                transaction.BankAccount.Balance -= transaction.Amount;
                transaction.BankAccount.Balance += model.Amount;
            }

            Mapper.Map(model, transaction);

            Context.SaveChanges();

            var result = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(result);
        }

        [HttpPost]
        [Route("delete/{id}")]
        public IHttpActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();

            var transaction = Context
                .Transactions
                .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.BankAccount.HouseHold.OwnerId != userId &&
                transaction.CreatorId != userId)
            {
                ModelState.AddModelError("",
                    "You are not allowed to delete this transaction");
                return BadRequest(ModelState);
            }

            if (!transaction.Void)
            {
                transaction.BankAccount.Balance -= transaction.Amount;
            }

            Context.Transactions.Remove(transaction);
            Context.SaveChanges();

            return Ok();
        }

        [Route("getAll/{id}")]
        public IHttpActionResult GetAll(int id)
        {
            var bankaccount = Context
                .BankAccounts
                .FirstOrDefault(p => p.Id == id);

            if (bankaccount == null)
            {
                ModelState.AddModelError("Bank Account Not found", "Bank Account id Provided does not exist in the household");
                return BadRequest(ModelState);
            }

            var userId = User
              .Identity
              .GetUserId();

            var member = bankaccount
                .HouseHold
                .Members
                .FirstOrDefault(p => p.Id == userId);

            if (member == null && bankaccount.HouseHold.OwnerId != userId)
            {
                ModelState.AddModelError("Not the member or creator", "Only the members or owner of the Household can create view all transactions of the Bank Account");
                return BadRequest(ModelState);
            }

            var model = Context
                .Transactions
                .Where(p => p.BankAccountId == id)
                .ProjectTo<TransactionViewModel>()
                .ToList();

            return Ok(model);
        }

        
        [HttpPost]
        [Route("void/{id}")]
        public IHttpActionResult Void(int id)
        {
            var userId = User.Identity.GetUserId();

            var transaction = Context
                .Transactions
                .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.BankAccount.HouseHold.OwnerId != userId &&
                transaction.CreatorId != userId)
            {
                ModelState.AddModelError("",
                    "You are not allowed to void this transaction");
                return BadRequest(ModelState);
            }

            if (transaction.Void)
            {
                ModelState.AddModelError("",
                    "This transaction has already been voided");
                return BadRequest(ModelState);
            }

            transaction.BankAccount.Balance -= transaction.Amount;
            transaction.Void = true;

            Context.SaveChanges();

            return Ok();
        }

        [Route("get/{id}")]
        public IHttpActionResult Get(int id)
        {
            var transaction = Context
                            .Transactions
                            .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            var model = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(model);
        }

       
    }
}


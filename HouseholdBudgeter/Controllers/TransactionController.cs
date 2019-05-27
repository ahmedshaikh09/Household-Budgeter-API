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
        [Route("bankaccount/{id}")]
        public IHttpActionResult Create(int id, TransactionBindingModel formData)
        {
            var bankaccount = Context
                .BankAccounts
                .FirstOrDefault(p => p.Id == id);

            if (bankaccount == null)
            {
                ModelState.AddModelError("Bank Account Not found", "Bank Account id Provided does not exist in the household");
                return BadRequest(ModelState);
            }

            var category = bankaccount.HouseHold.Categories
                .FirstOrDefault(p => p.Id == formData.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("Category Not found", "Category Id Provided does not exist in the household");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User
               .Identity
               .GetUserId();

            var member = bankaccount.HouseHold.Members.FirstOrDefault(p => p.Id == userId);

            if (member == null && bankaccount.HouseHold.OwnerId != userId)
            {
                ModelState.AddModelError("Not a member or owner", "Only the members or owner of the Household can create a transaction for the Bank Account");
                return BadRequest(ModelState);
            }

            var transaction = Mapper.Map<Transaction>(formData);
            transaction.BankAccountId = id;
            transaction.CreatorId = userId;

            if (transaction.Amount < 0)
            {
                bankaccount.Balance = transaction.Amount - bankaccount.Balance;
            }
            else
            {
                bankaccount.Balance = transaction.Amount + bankaccount.Balance;
            }

            bankaccount.Transactions.Add(transaction);
            Context.SaveChanges();

            var model = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Put(int id, TransactionBindingModel formData)
        {
            var transaction = Context
                .Transactions
                .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                ModelState.AddModelError("Transaction Not found", "Transaction id Provided does not exist");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User
               .Identity
               .GetUserId();

            var creator = transaction
                .Creator
                .Id;

            if (userId != creator && transaction.BankAccount.HouseHold.OwnerId != userId)
            {
                ModelState.AddModelError("Not the owner or creator", "Only the creator of the transaction or owner of the Household can edit");
                return BadRequest(ModelState);
            }

            Mapper.Map(formData, transaction);
            transaction.DateUpdated = DateTime.Now;


            transaction.BankAccount.Balance = transaction.Amount + transaction.BankAccount.Balance;


            Context.SaveChanges();

            var model = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var transaction = Context
                  .Transactions
                  .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                ModelState.AddModelError("Transaction Not found", "Transaction id Provided does not exist");
                return BadRequest(ModelState);
            }

            var owner = transaction
                .BankAccount
                .HouseHold
                .OwnerId;

            var userId = User
              .Identity
              .GetUserId();

            var creator = transaction
                .Creator
                .Id;

            if (userId != creator && owner != userId)
            {
                ModelState.AddModelError("Not the owner or creator", "Only the creator of the transaction or owner of the Household can delete");
                return BadRequest(ModelState);
            }

            transaction.BankAccount.Balance = transaction.BankAccount.Balance - transaction.Amount;
            Context.Transactions.Remove(transaction);
            Context.SaveChanges();

            return Ok();
        }

        [Route("bankaccount/{id}/get-all")]
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

        [Route("{id}/void")]
        public IHttpActionResult Void(int id)
        {
            var transaction = Context
                .Transactions
                .FirstOrDefault(p => p.Id == id);

            if (transaction == null)
            {
                ModelState.AddModelError("Transaction Not found", "Transaction id Provided does not exist");
                return BadRequest(ModelState);
            }

            var owner = transaction
                .BankAccount
                .HouseHold
                .OwnerId;

            var userId = User
              .Identity
              .GetUserId();

            var creator = transaction
                .Creator
                .Id;

            if (userId != creator && owner != userId)
            {
                ModelState.AddModelError("Not the owner or creator", "Only the creator of the transaction or owner of the Household can void it");
                return BadRequest(ModelState);
            }

            transaction.Void = true;
            transaction.BankAccount.Balance = transaction.Amount - transaction.BankAccount.Balance;
            Context.SaveChanges();

            return Ok();
        }
    }
}


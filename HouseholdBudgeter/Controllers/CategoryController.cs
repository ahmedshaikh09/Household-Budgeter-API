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
    [RoutePrefix("api/category")]
    [Authorize]
    public class CategoryController : ApiController
    {
        private ApplicationDbContext Context;

        public CategoryController()
        {
            Context = new ApplicationDbContext();
        }

        [Route("house-hold/{id}")]
        public IHttpActionResult Create(int id, CategoryBindingModel formData)
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
                ModelState.AddModelError("Not the Owner", "Only the owner create Categories");
                return BadRequest(ModelState);
            }

            var category = Mapper.Map<Category>(formData);
            category.HouseHoldId = id;

            houseHold.Categories.Add(category);
            Context.SaveChanges();

            var model = Mapper.Map<CategoryViewModel>(category);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Put(int id, CategoryBindingModel formData)
        {
            var category = Context
                .Categories
                .FirstOrDefault(p => p.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var owner = category
                .HouseHold
                .OwnerId;

            var userId = User
                .Identity
                .GetUserId();

            if (userId != owner)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can edit Categories");
                return BadRequest(ModelState);
            }

            Mapper.Map(formData, category);
            category.DateUpdated = DateTime.Now;

            Context.SaveChanges();

            var model = Mapper.Map<HouseHoldViewModel>(category);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var category = Context
                  .Categories
                  .FirstOrDefault(p => p.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var owner = category.HouseHold.OwnerId;

            var user = User.Identity.GetUserId();
            if (user != owner)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can delete a category");
                return BadRequest(ModelState);
            }

            Context.Categories.Remove(category);
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
                .Categories
                .Where(p => p.HouseHoldId == id)
                .ProjectTo<CategoryViewModel>()
                .ToList();

            return Ok(model);
        }
    }
}

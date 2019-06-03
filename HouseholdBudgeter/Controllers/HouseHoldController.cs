using AutoMapper;
using AutoMapper.QueryableExtensions;
using BugTracker.Models;
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
    [RoutePrefix("api/houseHold")]
    [Authorize]
    public class HouseHoldController : ApiController
    {
        private ApplicationDbContext Context;

        public HouseHoldController()
        {
            Context = new ApplicationDbContext();
        }

        [Route("get-all")]
        public IHttpActionResult GetAll()
        {
            var model = Context
                .HouseHolds
                .ProjectTo<HouseHoldViewModel>()
                .ToList();

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Get(int id)
        {
            var houseHold = Context
                            .HouseHolds
                            .FirstOrDefault(p => p.Id == id);

            if (houseHold == null)
            {
                return NotFound();
            }

            var model = Context
                .HouseHolds
                .Where(p => p.Id == houseHold.Id)
                .ProjectTo<HouseHoldViewModel>();

            return Ok(model);
        }

        [Route("create")]
        public IHttpActionResult Post(HouseHoldBindingModel formData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var houseHold = Mapper.Map<HouseHold>(formData);

            var userId = User
                .Identity
                .GetUserId();

            houseHold.OwnerId = userId;

            var user = Context
                .Users
                .FirstOrDefault(p => p.Id == userId);

            houseHold.Members.Add(user);

            Context.HouseHolds.Add(houseHold);
            Context.SaveChanges();

            var model = Mapper.Map<HouseHoldViewModel>(houseHold);

            return Ok(model);
        }

        [Route("edit/{id}")]
        public IHttpActionResult Put(int id, HouseHoldBindingModel formData)
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
            if (user != houseHold.OwnerId)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can do changes");
                return BadRequest(ModelState);
            }

            Mapper.Map(formData, houseHold);
            houseHold.DateUpdated = DateTime.Now;

            Context.SaveChanges();

            var model = Mapper.Map<HouseHoldViewModel>(houseHold);

            return Ok(model);
        }

        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            var houseHold = Context
                .HouseHolds
                .FirstOrDefault(p => p.Id == id);

            if (houseHold == null)
            {
                return NotFound();
            }

            var user = User
                .Identity
                .GetUserId();

            if (user != houseHold.OwnerId)
            {
                ModelState.AddModelError("Not the Owner", "Only the owner can delete a House Hold");
                return BadRequest(ModelState);
            }

            Context.HouseHolds.Remove(houseHold);
            Context.SaveChanges();

            return Ok();
        }

        [Route("{id}/invite")]
        public IHttpActionResult Invite(int id, MemberBindingModel model)
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
                ModelState.AddModelError("Not the Owner", "Only the owner can invite members");
                return BadRequest(ModelState);
            }

            var user = Context
                .Users
                .FirstOrDefault(p => p.Email == model.UserEmail);

            if (user == null)
            {
                ModelState.AddModelError("Not Found", "User does not exist");
                return BadRequest(ModelState);
            }

            houseHold.InvitedUsers.Add(user);
            Context.SaveChanges();

            EmailService emailInvitation = new EmailService();
            emailInvitation.Send(model.UserEmail, "You have been invited to a new House Hold.", "Invitation");

            return Ok();
        }

        [Route("{id}/join")]
        public IHttpActionResult Join(int id)
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

            var userId = User.Identity.GetUserId();

            if (houseHold.OwnerId == userId)
            {
                ModelState.AddModelError("Fail", "You are already the Owner of the requested House Hold");
                return BadRequest(ModelState);
            }

            if (!houseHold.InvitedUsers.Any(t => t.Id == userId))
            {
                ModelState.AddModelError("Not Invited", "You have not yet been invited by the Owner of the requested House Hold");
                return BadRequest(ModelState);
            }

            var member = Context
                .Users
                .FirstOrDefault(p => p.Id == userId);

            houseHold.Members.Add(member);
            houseHold.InvitedUsers.Remove(member);
            Context.SaveChanges();

            return Ok();

        }

        [Route("{id}/leave")]
        public IHttpActionResult Leave(int id)
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

            var userId = User
                .Identity
                .GetUserId();

            if (houseHold.OwnerId == userId)
            {
                ModelState.AddModelError("Owner can't leave", "Owner Can Only delete the house hold, not leave it.");
                return BadRequest(ModelState);
            }

            if (!houseHold.Members.Any(t => t.Id == userId))
            {
                ModelState.AddModelError("Not a Member", "You are not a member of the requested House Hold");
                return BadRequest(ModelState);
            }

            var member = Context.Users.FirstOrDefault(p => p.Id == userId);

            houseHold.Members.Remove(member);
            Context.SaveChanges();

            return Ok();
        }

        [Route("{id}/get-all-members")]
        public IHttpActionResult GetAllMembers(int id)
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

            var user = User
                .Identity
                .GetUserId();

            if (!houseHold.Members.Any(t => t.Id == user))
            {
                ModelState.AddModelError("Not a Member", "You are not a member of the requested HouseHold");
                return BadRequest(ModelState);
            }

            var members = houseHold.Members.Select(p => new MembersViewModel
            {
                memberEmail = p.Email,
            }).ToList();

            return Ok(members);
        }
    }
}
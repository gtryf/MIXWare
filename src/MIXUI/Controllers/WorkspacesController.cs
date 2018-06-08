﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIXUI.Dtos;
using MIXUI.Entities;
using MIXUI.Helpers;

namespace MIXUI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class WorkspacesController : Controller
    {
        private readonly DataContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public WorkspacesController(IMapper mapper, DataContext appDbContext, IAuthorizationService authorizationService)
        {
            _mapper = mapper;
            _appDbContext = appDbContext;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public IActionResult GetWorkspaces()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var workspaces = _appDbContext.Workspaces.AsNoTracking().Where(w => w.IdentityId == id);

            return Ok(_mapper.Map<ICollection<WorkspaceDto>>(workspaces));
        }

        [HttpGet("{id}", Name = "GetWorkspace")]
        public async Task<IActionResult> GetById(string id)
        {
            var workspace = await _appDbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }
            if ((await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Ok(_mapper.Map<WorkspaceDto>(workspace));
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostWorkspace([FromBody]CreateWorkspaceDto workspace)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var entity = _mapper.Map<Workspace>(workspace);
            entity.IdentityId = id;
            await _appDbContext.Workspaces.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtRoute("GetWorkspace", new { id = entity.Id }, _mapper.Map<WorkspaceDto>(entity));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkspace(string id)
        {
            var userId = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var workspace = await _appDbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }
            if ((await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                _appDbContext.Remove(workspace);
                await _appDbContext.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
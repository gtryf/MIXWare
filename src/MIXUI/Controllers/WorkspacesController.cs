using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public WorkspacesController(UserManager<AppUser> userManager, IMapper mapper, DataContext appDbContext)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkspaces()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var user = await _userManager.FindByIdAsync(id);

            return Ok(_mapper.Map<ICollection<WorkspaceDto>>(user.Workspaces));
        }

        [HttpGet("{id}", Name = "GetWorkspace")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var workspace = await _appDbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }
            if (workspace.IdentityId != userId)
            {
                return Unauthorized();
            }
            return Ok(_mapper.Map<WorkspaceDto>(workspace));
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
            await _appDbContext.Workspaces.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtRoute("GetWorkspace", new { id = entity.Id }, _mapper.Map<WorkspaceDto>(entity));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkspace(Guid id)
        {
            var userId = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var workspace = await _appDbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }
            if (workspace.IdentityId != userId)
            {
                return Unauthorized();
            }

            _appDbContext.Remove(workspace);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

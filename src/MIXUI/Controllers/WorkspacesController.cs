using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var workspaces = _appDbContext.Workspaces.AsNoTracking()
                .Include(w => w.Files)
                .Where(w => w.IdentityId == id)
                .OrderByDescending(w => w.CreatedUtc);

            return Ok(_mapper.Map<ICollection<ShortWorkspaceDto>>(workspaces));
        }

        [HttpGet("{id}", Name = "GetWorkspace")]
        public async Task<IActionResult> GetById(string id)
        {
            var workspace = await _appDbContext.Workspaces.AsNoTracking().Include(w => w.Files).SingleOrDefaultAsync(w => w.Id == id);
            if (workspace == null)
            {
                return NotFound();
            }
            if ((await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Ok(_mapper.Map<FullWorkspaceDto>(workspace));
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkspace([FromBody]CreateWorkspaceDto workspace)
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

            return CreatedAtRoute("GetWorkspace", new { id = entity.Id }, _mapper.Map<ShortWorkspaceDto>(entity));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkspace(string id, [FromBody]UpdateWorkspaceDto workspaceDto) {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var workspace = await _appDbContext.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

            _mapper.Map(workspaceDto, workspace);
            await _appDbContext.SaveChangesAsync();

            return Ok(_mapper.Map<ShortWorkspaceDto>(workspace));
        }

		[HttpGet("{workspaceId}/{fileId}", Name = "GetFile")]
		public async Task<IActionResult> GetFileById(string workspaceId, string fileId)
        {
            var workspace = await _appDbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

			var file = await _appDbContext.Files.FindAsync(fileId);
			if (file == null)
            {
                return NotFound();
            }
			if (file.WorkspaceId != workspaceId) 
			{
				return BadRequest(Errors.AddErrorToModelState("NotInWorkspace", "The specified file does not belong to this workspace", ModelState));
			}

			return Ok(_mapper.Map<FileDto>(file));
        }

        [HttpPost("{workspaceId}")]
		public async Task<ActionResult> CreateFile(string workspaceId, [FromBody]CreateFileDto data)
        {
            var workspace = await _appDbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }
            if (await _appDbContext.Files.AnyAsync(i => i.Name == data.Name && i.WorkspaceId == workspaceId))
            {
                return Conflict();
            }

            var entity = _mapper.Map<File>(data);
            entity.WorkspaceId = workspaceId;
            await _appDbContext.AddAsync(entity);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtRoute("GetFile", new { workspaceId = workspaceId, fileId = entity.Id }, _mapper.Map<CreatedFileDto>(entity));
		}

        [HttpPut("{workspaceId}/{fileId}")]
		public async Task<ActionResult> UpdateFile(string workspaceId, string fileId, [FromBody]CreateFileDto data)
        {
            var workspace = await _appDbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

			var file = await _appDbContext.Files.FindAsync(fileId);
			if (file == null)
            {
                return NotFound();
            }
			if (file.WorkspaceId != workspaceId) 
			{
				return BadRequest(Errors.AddErrorToModelState("NotInWorkspace", "The specified file does not belong to this workspace", ModelState));
			}
            if (!(await _authorizationService.AuthorizeAsync(User, file, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }
            if (await _appDbContext.Files.AnyAsync(i => i.Name == data.Name && i.WorkspaceId == workspaceId && i.Id != fileId))
            {
                return Conflict();
            }

            _mapper.Map(data, file);
			await _appDbContext.SaveChangesAsync();

            return Ok(_mapper.Map<FileDto>(file));
		}

        [HttpDelete("{workspaceId}/{fileId}")]
		public async Task<ActionResult> DeleteFile(string workspaceId, string fileId)
        {
            var workspace = await _appDbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

			var file = await _appDbContext.Files.FindAsync(fileId);
			if (file == null)
            {
                return NotFound();
            }
			if (file.WorkspaceId != workspaceId) 
			{
				return BadRequest(Errors.AddErrorToModelState("NotInWorkspace", "The specified file does not belong to this workspace", ModelState));
			}
            if (!(await _authorizationService.AuthorizeAsync(User, file, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

            _appDbContext.Remove(file);
			await _appDbContext.SaveChangesAsync();

            return NoContent();
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

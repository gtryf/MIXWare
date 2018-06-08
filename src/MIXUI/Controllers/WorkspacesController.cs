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

            return CreatedAtRoute("GetWorkspace", new { id = entity.Id }, _mapper.Map<WorkspaceDto>(entity));
        }

        [HttpGet("{workspaceId}/{storableId}", Name = "GetStorable")]
        public async Task<IActionResult> GetStorableById(string workspaceId, string storableId)
        {
            var userId = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var workspace = await _appDbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

            var folder = (await _appDbContext.Folders.Include(i => i.Children).ToListAsync()).SingleOrDefault(i => i.Id == storableId);
            var file = await _appDbContext.Files.FindAsync(storableId);
            if (folder == null && file == null)
            {
                return NotFound();
            }
            var s = (Storable)folder ?? file;
            bool found = false;
            while (s != null)
            {
                if (s.Id == workspace.Root.Id)
                {
                    found = true;
                    break;
                }
                s = await _appDbContext.Folders.FindAsync(s.ParentId);
            }
            if (!found)
            {
                return BadRequest(Errors.AddErrorToModelState("NotInWorkspace", "The specified storable is not in the workspace", ModelState));
            }

            if (file != null)
            {
                return Ok(new
                {
                    file.Id,
                    file.Name,
                    Data = Encoding.UTF8.GetString(file.Data),
                });
            }
            else
            {
                return Ok(new
                {
                    folder.Id,
                    folder.Name,
                    Children = folder.Children.Select(i => _mapper.Map<StorableDto>(i)),
                });
            }
        }

        [HttpPost("{workspaceId}/{parentId}")]
        public async Task<ActionResult> CreateStorable(string workspaceId, string parentId, [FromBody]CreateStorableDto data)
        {
            if (data.Type != "folder" && data.Type != "file")
            {
                return BadRequest(Errors.AddErrorToModelState("InvalidType", "Storable type must either be 'file' or 'folder'", ModelState));
            }
            var userId = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var workspace = await _appDbContext.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, workspace, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }
            var parent = await _appDbContext.Folders.FindAsync(parentId);
            if (parent == null)
            {
                return NotFound();
            }
            bool found = false;
            Storable s = parent;
            while (s != null)
            {
                if (s.Id == workspace.Root.Id)
                {
                    found = true;
                    break;
                }
                s = await _appDbContext.Folders.FindAsync(s.ParentId);
            }
            if (!found)
            {
                return BadRequest(Errors.AddErrorToModelState("NotInWorkspace", "The specified storable is not in the workspace", ModelState));
            }

            if (data.Type == "folder")
            {
                var entity = new Folder() { Name = data.Name, ParentId = parentId };
                await _appDbContext.AddAsync(entity);
                await _appDbContext.SaveChangesAsync();

                return CreatedAtRoute("GetStorable", new { workspaceId = workspaceId, storableId = entity.Id }, _mapper.Map<StorableDto>(entity));
            }
            else
            {
                var entity = new File() { Name = data.Name, Data = Encoding.UTF8.GetBytes(data.FileContents), ParentId = parentId };
                await _appDbContext.AddAsync(entity);
                await _appDbContext.SaveChangesAsync();

                return CreatedAtRoute("GetStorable", new { workspaceId = workspaceId, storableId = entity.Id }, _mapper.Map<StorableDto>(entity));
            }
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

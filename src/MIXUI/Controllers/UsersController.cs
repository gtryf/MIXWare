using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MIXUI.Dtos;
using MIXUI.Entities;
using MIXUI.Helpers;

namespace MIXUI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public UsersController(DataContext dataContext, UserManager<AppUser> userManager, IMapper mapper)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public IActionResult GetAll() => Ok(_userManager.Users.ToList().Select(_mapper.Map<UserDto>));

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!User.HasClaim(claim => claim.Type == Constants.Strings.JwtClaimIdentifiers.Id && claim.Value == id))
            {
                return StatusCode(403);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UserDto>(user));
        }
    }
}

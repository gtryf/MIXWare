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
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly DataContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public AccountsController(UserManager<AppUser> userManager, IMapper mapper, DataContext appDbContext, IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody]RegisterDto userInfo)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdentity = _mapper.Map<AppUser>(userInfo);
            userIdentity.IsEnabled = true;

            var result = await _userManager.CreateAsync(userIdentity, userInfo.Password);

            if (!result.Succeeded) return BadRequest(Errors.AddErrorsToModelState(result, ModelState));

            return CreatedAtAction("GetById", "Users", new { id = userIdentity.Id }, _mapper.Map<UserDto>(userIdentity));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetSelf()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if ((await _authorizationService.AuthorizeAsync(User, user, "SameUserPolicy")).Succeeded)
            {
                return Ok(_mapper.Map<UserDto>(user));
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if ((await _authorizationService.AuthorizeAsync(User, user, "SameUserPolicy")).Succeeded)
            {
                return Ok(_mapper.Map<UserDto>(user));
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}

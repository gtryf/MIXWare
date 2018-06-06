using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MIXUI.Dtos;
using MIXUI.Entities;
using MIXUI.Helpers;

namespace MIXUI.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly DataContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public AccountsController(UserManager<AppUser> userManager, IMapper mapper, DataContext appDbContext)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdentity = _mapper.Map<AppUser>(model);

            var result = await _userManager.CreateAsync(userIdentity, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

            return CreatedAtAction("GetById", "Users", new { id = userIdentity.Id }, _mapper.Map<UserDto>(userIdentity));
        }
    }
}

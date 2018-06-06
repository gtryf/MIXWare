using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MIXUI.Dtos;
using MIXUI.Entities;
using MIXUI.Helpers;
using MIXUI.Services;

namespace MIXUI.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /users
        ///     {
        ///         "username": "user",
        ///         "password": "p@ssw0rd"
        ///     }
        /// </remarks>
        /// <param name="credentials"></param>
        /// <returns>An authenticated token</returns>
        /// <response code="200">Returns the authenticated user and token</response>
        /// <response code="401">If credentials are invalid</response>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Authenticate([FromBody] LoginDto credentials)
        {
            var user = _userService.Authenticate(credentials.Username, credentials.Password);

            if (user == null)
                return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                user.Id,
                user.Username,
                Token = tokenString
            });
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /users
        ///     {
        ///         "username": "user",
        ///         "password": "p@ssw0rd"
        ///     }
        /// </remarks>
        /// <param name="userInfo"></param>
        /// <returns>A new user object</returns>
        /// <response code="201">If the user is successfully created</response>
        /// <response code="400">If input data is invalid</response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult Register([FromBody] RegisterDto userInfo)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userInfo);

            try
            {
                // save 
                var newUser = _userService.Create(user, userInfo.Password);
                return CreatedAtRoute("GetUser", new { id = newUser.Id }, _mapper.Map<UserDto>(user));
            }
            catch (ApiException ex)
            {
                // return error message if there was an exception
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Return all users in the system
        /// </summary>
        /// <remakrs>Must be called with a token with administrative claims</remakrs>
        /// <returns>A list of user objects</returns>
        /// <response code="200">Return all users</response>
        /// <response code="401">If the supplied token does not have administrative claims</response>
        [ProducesResponseType(200, Type = typeof(IList<UserDto>))]
        [ProducesResponseType(401)]
        [HttpGet]
        public IActionResult GetAll()
        {
            // TODO: Check authorization
            var users = _userService.GetAll();
            var userDtos = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDtos);
        }

        /// <summary>
        /// Return a user by their internal ID
        /// </summary>
        /// <remarks>
        /// To access the user information, the request must either be supplied a token which claims that their user ID matches the request ID, or that the requesting user is an administrator.
        /// </remarks>
        /// <param name="id">The requested user's internal ID</param>
        /// <returns>The user's information</returns>
        /// <response code="200">If the call is successful</response>
        /// <response code="401">If the calling token does not have access</response>
        [ProducesResponseType(200, Type = typeof(UserDto))]
        [ProducesResponseType(401)]
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetById(int id)
        {
            // TODO: Check authorization
            var user = _userService.GetById(id);
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <remarks>
        /// To access the user information, the request must either be supplied a token which claims that their user ID matches the request ID, or that the requesting user is an administrator. If the updated username is different, it's first checked against the usernames in the database.
        /// </remarks>
        /// <param name="id">The user's ID</param>
        /// <param name="updatedInfo">The new user information</param>
        /// <response code="204">If the update is successful</response>
        /// <response code="400">If the changed username is already taken</response>
        /// <response code="401">If the calling token does not have access</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult Update(int id, [FromBody] RegisterDto updatedInfo)
        {
            // TODO: Check authorization

            // map dto to entity and set id
            var user = _mapper.Map<User>(updatedInfo);
            user.Id = id;

            try
            {
                // save 
                _userService.Update(user, updatedInfo.Password);
                return NoContent();
            }
            catch (ApiException ex)
            {
                // return error message if there was an exception
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <remarks>
        /// To allow deletion, the request must either be supplied a token which claims that their user ID matches the request ID, or that the requesting user is an administrator. An administrative user cannot delete themselves.
        /// </remarks>
        /// <param name="id">The internal ID of the user to be deleted</param>
        /// <response code="204">If the deletion was successful</response>
        /// <response code="400">If an administrator attempts to delete themselves</response>
        /// <response code="401">If the calling token does not have access</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult Delete(int id)
        {
            // TODO: Check authorization

            _userService.Delete(id);
            return NoContent();
        }
    }
}

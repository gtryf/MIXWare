using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIXUI.Assembler;
using MIXUI.Dtos;
using MIXUI.Helpers;

namespace MIXUI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly DataContext _appDbContext;
        private readonly Func<string, IAssembler> _assemblerAccessor;
        private readonly Func<string, IPrettyPrinter> _prettyPrinterAccessor;

        public SubmissionsController(IAuthorizationService authorizationService, DataContext appDbContext, Func<string, IAssembler> assemblerAccessor, Func<string, IPrettyPrinter> prettyPrinterAccessor)
        {
            this._authorizationService = authorizationService;
            this._appDbContext = appDbContext;
            this._assemblerAccessor = assemblerAccessor;
            this._prettyPrinterAccessor = prettyPrinterAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubmission([FromBody]SubmissionDto data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var file = await _appDbContext.Files.Include(i => i.Workspace).SingleOrDefaultAsync(i => i.Id == data.FileId);
            if (file == null)
            {
                return NotFound();
            }

            if (!(await _authorizationService.AuthorizeAsync(User, file, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

            var assembler = _assemblerAccessor(data.Type);
            if (data.ProduceListing || data.ProduceSymbolTable)
            {
                data.PrettyPrinter = data.PrettyPrinter ?? "plain";
                assembler.PrettyPrinter = _prettyPrinterAccessor(data.PrettyPrinter);
            }

            var assembly = assembler.Assemble(file.Name, Encoding.UTF8.GetString(file.Data), data.ProduceSymbolTable, data.ProduceListing);
            var result = assembly.Match<IActionResult>(r =>
                {
                    return Ok(new
                    {
                        r.WordCount,
                        r.Listing,
                        r.SymbolTable,
                        Assembly = Convert.ToBase64String(r.Assembly),
                    });
                },
                r =>
                {
                    return BadRequest(new
                    {
                        r.Errors,
                        r.Warnings,
                    });
                });

            return result;
        }
    }
}

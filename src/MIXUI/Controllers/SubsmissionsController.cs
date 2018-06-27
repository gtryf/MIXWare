using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIXUI.Assembler;
using MIXUI.Dtos;
using MIXUI.Helpers;
using MIXUI.TaskQueues;

namespace MIXUI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly DataContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly Func<string, IAssembler> _assemblerAccessor;
        private readonly Func<string, IPrettyPrinter> _prettyPrinterAccessor;
        private readonly IBackgroundTaskQueue _queue;
        
        public SubmissionsController(
            IAuthorizationService authorizationService, DataContext appDbContext, IMapper mapper,
            Func<string, IAssembler> assemblerAccessor, Func<string, IPrettyPrinter> prettyPrinterAccessor,
            IBackgroundTaskQueue queue)
        {
            this._authorizationService = authorizationService;
            this._appDbContext = appDbContext;
            this._mapper = mapper;
            this._assemblerAccessor = assemblerAccessor;
            this._prettyPrinterAccessor = prettyPrinterAccessor;
            this._queue = queue;
        }

        [HttpGet("{id}", Name = "GetSubmission")]
        public async Task<IActionResult> GetSubmission(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var submission = await _appDbContext.Submissions.FindAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            if (!(await _authorizationService.AuthorizeAsync(User, submission, "SameUserPolicy")).Succeeded)
            {
                return Unauthorized();
            }

            return submission.Successful ? Ok(_mapper.Map<SuccessfulSubmissionDto>(submission)) : Ok(_mapper.Map<FailedSubmissionDto>(submission));
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubmission([FromBody]CreateSubmissionDto data)
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

            var submission = new Entities.Submission()
            {
                Status = Entities.SubmissionStatus.New,
                IdentityId = User.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
            };
            await _appDbContext.Submissions.AddAsync(submission);
            await _appDbContext.SaveChangesAsync();

            _queue.QueueBackgroundWorkItem(ProcessSubmission(data, file, assembler, submission));

            return AcceptedAtRoute("GetSubmission", new { id = submission.Id });
        }

        private Func<System.Threading.CancellationToken, IServiceScope, Task> ProcessSubmission(
            CreateSubmissionDto data, Entities.File file, IAssembler assembler, Entities.Submission submission)
        {
            return async (token, scope) =>
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                submission.Status = Entities.SubmissionStatus.Pending;
                dbContext.Submissions.Update(submission);
                await dbContext.SaveChangesAsync();

                var assembly = assembler.Assemble(file.Name, Encoding.UTF8.GetString(file.Data), data.ProduceSymbolTable, data.ProduceListing);
                await assembly.Match(async r =>
                {
                    var assemblyFile = new Entities.File
                    {
                        Name = data.Type == "card" ? file.Name + ".deck" : file.Name + ".mix",
                        Type = data.Type == "card" ? Entities.FileType.Deck : Entities.FileType.CompiledOutput,
                        Data = r.Assembly,
                        WorkspaceId = file.WorkspaceId,
                        CreatedUtc = DateTime.UtcNow,
                        UpdatedUtc = DateTime.UtcNow,
                    };
                    await dbContext.Files.AddAsync(assemblyFile);

                    Entities.File listingFile = null, symbolFile = null;
                    if (data.ProduceListing)
                    {
                        listingFile = new Entities.File
                        {
                            Name = file.Name + ".listing",
                            Type = Entities.FileType.Listing,
                            Data = Encoding.UTF8.GetBytes(r.Listing),
                            WorkspaceId = file.WorkspaceId,
                            CreatedUtc = DateTime.UtcNow,
                            UpdatedUtc = DateTime.UtcNow,
                        };
                        await dbContext.Files.AddAsync(listingFile);
                    }
                    if (data.ProduceSymbolTable)
                    {
                        symbolFile = new Entities.File
                        {
                            Name = file.Name + ".symbols",
                            Type = Entities.FileType.SymbolTable,
                            Data = Encoding.UTF8.GetBytes(r.SymbolTable),
                            WorkspaceId = file.WorkspaceId,
                            CreatedUtc = DateTime.UtcNow,
                            UpdatedUtc = DateTime.UtcNow,
                        };
                        await dbContext.Files.AddAsync(symbolFile);
                    }

                    await dbContext.SaveChangesAsync();

                    submission.Status = Entities.SubmissionStatus.Complete;
                    submission.Successful = true;
                    submission.AssemblyFileId = assemblyFile.Id;
                    if (data.ProduceListing)
                    {
                        submission.ListingFileId = listingFile.Id;
                    }
                    if (data.ProduceSymbolTable)
                    {
                        submission.SymbolFileId = symbolFile.Id;
                    }
                    dbContext.Submissions.Update(submission);
                    await dbContext.SaveChangesAsync();

                    return true;
                },
                    async r =>
                    {
                        submission.Status = Entities.SubmissionStatus.Complete;
                        submission.Successful = false;

                        submission.Errors = r.Errors.Select(e => new Entities.ErrorInfo
                        {
                            Column = e.Column,
                            Line = e.Line,
                            Text = e.Text,
                        });
                        submission.Warnings = r.Warnings.Select(w => new Entities.ErrorInfo
                        {
                            Column = w.Column,
                            Line = w.Line,
                            Text = w.Text,
                        });

                        dbContext.Submissions.Update(submission);
                        await dbContext.SaveChangesAsync();

                        return false;
                    });
            };
        }
    }
}

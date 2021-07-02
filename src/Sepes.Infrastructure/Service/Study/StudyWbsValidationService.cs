using Microsoft.Extensions.Logging;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudyWbsValidationService : IStudyWbsValidationService
    {
        readonly ILogger _logger;      
        readonly IWbsValidationService _wbsValidationService;
        readonly IDapperQueryService _dapperQueryService;

        public StudyWbsValidationService(ILogger<StudyWbsValidationService> logger, IWbsValidationService wbsValidationService, IDapperQueryService dapperQueryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));         
            _wbsValidationService = wbsValidationService ?? throw new ArgumentNullException(nameof(wbsValidationService));
            _dapperQueryService = dapperQueryService ?? throw new ArgumentNullException(nameof(dapperQueryService));
        }     

        public async Task ValidateForStudyCreate(Study study)
        {
            try
            {
                await PerformValidationAndUpdateStudy(study);               
            }
            catch (Exception ex)
            {
                _logger.LogError("Wbs validation failed", ex);              
            }           
        }

        public async Task ValidateForStudyUpdate(Study study)
        { 
            try
            {
                await PerformValidationAndUpdateStudy(study);               
            }
            catch (Exception ex)
            {
                _logger.LogError("Wbs validation failed", ex);
            }

            if (!study.WbsCodeValid && await HasSandboxOrDataset(study))
            {
                throw new InvalidWbsException($"Invalid WBS Code {study.WbsCode} for Study {study.Id}",
                    $"Invalid WBS code for Study. Study has active Datasets and/or Sandboxes. You cannot change to an invalid WBS code.");
            }
        }

        async Task<bool> HasSandboxOrDataset(Study study)
        {
            var hasSandboxTask = HasSandbox(study);
            var hasDatasetTask = HasDataset(study);

            await Task.WhenAny(hasSandboxTask, hasDatasetTask);

            if((hasSandboxTask.IsCompleted && hasSandboxTask.Result) || (hasDatasetTask.IsCompleted && hasDatasetTask.Result))
            {
                return true;
            }

            return false;
        }

        async Task<bool> HasSandbox(Study study)
        {
            var query = AnyQueryAsBool($"SELECT Id FROM dbo.[Sandboxes] WHERE [StudyId] = {study.Id} AND Deleted = 0");
            return await _dapperQueryService.RunDapperQuerySingleAsync<bool>(query);
        }

        async Task<bool> HasDataset(Study study)
        {
            var query = AnyQueryAsBool($"SELECT DatasetId FROM dbo.[StudyDatasets] sds LEFT JOIN dbo.[Datasets] ds on sds.DatasetId = ds.Id WHERE sds.[StudyId] = {study.Id} AND ds.Deleted = 0 AND ds.StudySpecific = 1");
            return await _dapperQueryService.RunDapperQuerySingleAsync<bool>(query);
        }       

        string AnyQueryAsBool(string innerQuery)
        {
            return $"SELECT CASE WHEN EXISTS({innerQuery}) THEN 1 ELSE 0 END";
        }

        public async Task ValidateForSandboxCreationOrThrow(Study study)
        {
            await ValidateForDependentEntityCreationOrThrow(study, "Sandbox");
        }

        public async Task ValidateForDatasetCreationOrThrow(Study study)
        {
            await ValidateForDependentEntityCreationOrThrow(study, "Dataset");
        }

        async Task ValidateForDependentEntityCreationOrThrow(Study study, string entityName)
        {
            if (String.IsNullOrWhiteSpace(study.WbsCode))
            {
                throw new InvalidWbsException($"Empty WBS Code for Study {study.Id}", $"Missing WBS code for Study. Study requires WBS code before {entityName} can be created.");
            }

            if (study.WbsCodeValid)
            {
                return;
            }

            await PerformValidationAndUpdateStudy(study);

            if (!study.WbsCodeValid)
            {
                throw new InvalidWbsException($"Invalid WBS Code {study.WbsCode} for Study {study.Id}", $"Invalid WBS code for Study. Study requires valid WBS code before {entityName} can be created.");
            }
        }        

        async Task PerformValidationAndUpdateStudy(Study study)
        {
            bool isValid;

            try
            {
                isValid = await _wbsValidationService.IsValid(study.WbsCode);
            }
            catch (Exception ex)
            {
                throw new InvalidWbsException($"WBS validation failed for Study {study.Id}, code: {study.WbsCode}", "Wbs validation failed.", ex);               
            }

            if (isValid)
            {
                study.WbsCodeValid = true;
                study.WbsCodeValidatedAt = DateTime.UtcNow;
            }
            else
            {
                study.WbsCodeValid = false;
                study.WbsCodeValidatedAt = default;              
            }
        }
    }
}
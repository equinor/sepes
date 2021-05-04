﻿using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Dto.VirtualMachine;
using Sepes.Infrastructure.Response.Sandbox;
using Sepes.RestApi.IntegrationTests.RequestHelpers;
using Sepes.RestApi.IntegrationTests.Setup;
using Sepes.RestApi.IntegrationTests.TestHelpers.AssertSets;
using Sepes.RestApi.IntegrationTests.TestHelpers.AssertSets.Dataset;
using Sepes.RestApi.IntegrationTests.TestHelpers.AssertSets.Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sepes.RestApi.IntegrationTests.Tests
{
    [Collection("Integration tests collection")]
    public class CreateStudyAndAllRelatedResourcesTest : ControllerTestBase
    {
        public CreateStudyAndAllRelatedResourcesTest(TestHostFixture testHostFixture)
            : base(testHostFixture)
        {

        }

        [Theory]
        [InlineData(true, false)]
        //[InlineData(false, true)]
        //[InlineData(true, true)]
        public async Task AddStudyAndSandboxAndVm_WithRequiredRole_ShouldSucceed(bool isAdmin, bool isSponsor)
        {
            await WithBasicSeeds();

            SetScenario(isEmployee: true, isAdmin: isAdmin, isSponsor: isSponsor);

            //CREATE STUDY
            var studyCreateConversation = await StudyCreator.CreateAndExpectSuccess(_restHelper);
            CreateStudyAsserts.ExpectSuccess(studyCreateConversation.Request, studyCreateConversation.Response);

            //CREATE STUDY SPECIFIC DATASET
            var datasetSeedResponse = await DatasetCreator.Create(_restHelper, studyCreateConversation.Response.Content.Id);
            var datasetCreateRequest = datasetSeedResponse.Request;
            var datasetResponseWrapper = datasetSeedResponse.Response;
            CreateDatasetAsserts.ExpectSuccess(datasetCreateRequest, datasetResponseWrapper);

            var createDatasetResponse = datasetResponseWrapper.Content;

            //CREATE SANDBOX
            var sandboxSeedResponse = await SandboxCreator.CreateAndExpectSuccess(_restHelper, studyCreateConversation.Response.Content.Id);
            var sandboxCreateRequest = sandboxSeedResponse.Request;
            var sandboxResponseWrapper = sandboxSeedResponse.Response;

            SandboxDetailsAsserts.NewlyCreatedExpectSuccess(sandboxCreateRequest, sandboxResponseWrapper);

            var sandboxResponse = sandboxResponseWrapper.Content;

            //ADD DATASET TO SANDBOX
            var addDatasetToSandboxResponse = await SandboxOperations.AddDataset(_restHelper, sandboxResponse.Id, createDatasetResponse.Id);
            var sandboxDatasetResponseWrapper = addDatasetToSandboxResponse.Response;
            AddDatasetToSandboxAsserts.ExpectSuccess(createDatasetResponse.Id, createDatasetResponse.Name, createDatasetResponse.Classification, "Open", sandboxDatasetResponseWrapper);

            //CREATE VM
            var virtualMachineSeedResponse = await VirtualMachineCreator.Create(_restHelper, sandboxResponse.Id);
            var virtualMachineCreateRequest = virtualMachineSeedResponse.Request;
            var virtualMachineResponseWrapper = virtualMachineSeedResponse.Response;

            CreateVirtualMachineAsserts.ExpectSuccess(virtualMachineCreateRequest, sandboxResponse.Region, virtualMachineResponseWrapper);

            //GET SANDBOX RESOURCE LIST AND ASSERT RESULT BEFORE CREATION
            var sandboxResourcesPreProvisioningResponseWrapper = await _restHelper.Get<List<SandboxResourceLight>>($"api/sandboxes/{sandboxResponse.Id}/resources");
            SandboxResourceListAsserts.BeforeProvisioning(sandboxResourcesPreProvisioningResponseWrapper, virtualMachineResponseWrapper.Content.Name);

            //GET SANDBOX VM LIST AND ASSERT RESULT BEFORE CREATION
            var virtualMachinesPreProvisioningResponseWrapper = await GenericReader.ReadAndAssertExpectSuccess<List<VmDto>>(_restHelper, GenericReader.SandboxVirtualMachines(sandboxResponse.Id));
            SandboxVirtualMachineAsserts.BeforeProvisioning(virtualMachinesPreProvisioningResponseWrapper.Response, virtualMachineResponseWrapper.Content.Name);

            //SETUP INFRASTRUCTURE BY RUNNING A METHOD ON THE API            
            var processWorkQueueResponse = await ProcessWorkQueue();

            //GET SANDBOX RESOURCE LIST AND ASSERT RESULT
            var sandboxResourcesResponseWrapper = await _restHelper.Get<List<SandboxResourceLight>>($"api/sandboxes/{sandboxResponse.Id}/resources");
            SandboxResourceListAsserts.AfterProvisioning(sandboxResourcesResponseWrapper, virtualMachineResponseWrapper.Content.Name);

            //TODO: GET SANDBOX VM LIST AND ASSERT RESULT
            var virtualMachinesAfterProvisioningResponseWrapper = await GenericReader.ReadAndAssertExpectSuccess<List<VmDto>>(_restHelper, GenericReader.SandboxVirtualMachines(sandboxResponse.Id));
            SandboxVirtualMachineAsserts.AfterProvisioning(virtualMachinesAfterProvisioningResponseWrapper.Response, virtualMachineResponseWrapper.Content.Name);


            //TODO: Add some participants

            await StudyParticipantAdderAndRemover.AddAndExpectSuccess(_restHelper, studyCreateConversation.Response.Content.Id, "admin",
                new ParticipantLookupDto { FullName = "John", DatabaseId = 1, EmailAddress = "john@email.com", ObjectId = "1", Source = "DB", UserName = "john" });

            var test = await _restHelper.Get<VmRuleDto>($"api/virtualmachines/{virtualMachineResponseWrapper.Content.Id}/extended");
            //SandboxResourceListAsserts.AfterProvisioning(sandboxResourcesResponseWrapper, virtualMachineResponseWrapper.Content.Name);

            var openInternetResponse = await SandboxOperations.OpenInternetForVm<VmRuleDto>(_restHelper, "1");


            SandboxVirtualMachineRuleAsserts.ExpectSuccess(openInternetResponse.Response.Content, test.Content);

            //TODO: OPEN INTERNET

            //TODO: MOVE TO NEXT PHASE

            //TODO: DELETE VM

            //TODO: RUN WORKER

            //TODO: ASSERT THAT VM DISSAPEARS

            //TRY TO DELETE STUDY, GET ERROR

            //DELETE STUDY

            //DELETE SANDBOX


        }
    }
}

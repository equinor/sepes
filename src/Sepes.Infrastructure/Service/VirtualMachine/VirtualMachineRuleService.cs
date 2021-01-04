﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Dto.VirtualMachine;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Service.Queries;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Sepes.Infrastructure.Service
{
    public class VirtualMachineRuleService : IVirtualMachineRuleService
    {
        readonly ILogger _logger;
        readonly SepesDbContext _db;
        readonly IUserService _userService;
        readonly ICloudResourceReadService _sandboxResourceService;
        readonly ICloudResourceOperationReadService _sandboxResourceOperationReadService;
        readonly ICloudResourceOperationCreateService _sandboxResourceOperationCreateService;
        readonly IProvisioningQueueService _workQueue;

        public VirtualMachineRuleService(ILogger<VirtualMachineService> logger,
            SepesDbContext db,
            IUserService userService,
            ICloudResourceReadService sandboxResourceService,
            ICloudResourceOperationReadService sandboxResourceOperationReadService,
            ICloudResourceOperationCreateService sandboxResourceOperationCreateService,
            IProvisioningQueueService workQueue)
        {
            _logger = logger;
            _db = db;
            _userService = userService;
            _sandboxResourceService = sandboxResourceService;
            _sandboxResourceOperationReadService = sandboxResourceOperationReadService;
            _sandboxResourceOperationCreateService = sandboxResourceOperationCreateService;
            _workQueue = workQueue;
        }

        async Task<CloudResource> GetVmResourceEntry(int vmId, UserOperation operation)
        {
            _ = await StudySingularQueries.GetStudyByResourceIdCheckAccessOrThrow(_db, _userService, vmId, operation);
            var vmResource = await _sandboxResourceService.GetByIdAsync(vmId);

            return vmResource;
        }

        public async Task<VmRuleDto> GetRuleById(int vmId, string ruleId, CancellationToken cancellationToken = default)
        {
            var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Read);

            //Get config string
            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(vm.ConfigString);

            if (vmSettings.Rules != null)
            {
                foreach (var curExistingRule in vmSettings.Rules)
                {
                    if (curExistingRule.Name == ruleId)
                    {
                        return curExistingRule;
                    }
                }
            }

            throw new NotFoundException($"Rule with id {ruleId} does not exist");
        }            

        public async Task<List<VmRuleDto>> SetRules(int vmId, List<VmRuleDto> updatedRuleSet, CancellationToken cancellationToken = default)
        {
            var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);
                       

            //Get config string
            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(vm.ConfigString);

            await ValidateRuleUpdateInputThrowIfNot(vm, vmSettings.Rules, updatedRuleSet);

            bool saveAfterwards = false;

            if (updatedRuleSet == null || updatedRuleSet != null && updatedRuleSet.Count == 0) //Easy, all rules should be deleted
            {
                vmSettings.Rules = null;
                saveAfterwards = true;
            }
            else
            { 
                var newRules = updatedRuleSet.Where(r => String.IsNullOrWhiteSpace(r.Name)).ToList();
                var rulesThatShouldExistAllready = updatedRuleSet.Where(r => !String.IsNullOrWhiteSpace(r.Name)).ToList();

                //Check that the new rules does not have a duplicate in existing rules
                foreach (var curNew in newRules)
                {
                    ThrowIfRuleExists(rulesThatShouldExistAllready, curNew);
                }

                foreach (var curRule in updatedRuleSet)
                {
                    if (curRule.Direction == RuleDirection.Inbound)
                    {
                        if (curRule.Action == RuleAction.Deny)
                        {
                            throw new ArgumentException("Inbound rules can only have Action: Allow");
                        }

                        if (String.IsNullOrWhiteSpace(curRule.Name))
                        {
                            curRule.Name = AzureResourceNameUtil.NsgRuleNameForVm(vmId);
                            //curRule.Priority = AzureVmUtil.GetNextVmRulePriority(updatedRuleSet, curRule.Direction);
                        }
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(curRule.Name) || !curRule.Name.Contains(AzureVmConstants.RulePresets.OPEN_CLOSE_INTERNET))
                        {
                            throw new ArgumentException("Custom outbound rules are not allowed");
                        }
                    }
                }

                vmSettings.Rules = updatedRuleSet;
                saveAfterwards = true;
            }

            if (saveAfterwards)
            {
                vm.ConfigString = CloudResourceConfigStringSerializer.Serialize(vmSettings);

                await _db.SaveChangesAsync();

                await CreateUpdateOperationAndAddQueueItem(vm, "Updated rules");
            }

            return updatedRuleSet != null ? updatedRuleSet : new List<VmRuleDto>();
        }

        async Task ValidateRuleUpdateInputThrowIfNot(CloudResource vm, List<VmRuleDto> existingRules, List<VmRuleDto> updatedRuleSet)
        {
            var validationErrors = new List<string>();

            var sandbox = await _db.Sandboxes.Include(sb => sb.PhaseHistory).FirstOrDefaultAsync(sb => sb.Id == vm.SandboxId);
            var curPhase = SandboxPhaseUtil.GetCurrentPhase(sandbox);           

            //VALIDATE OUTBOUND RULE, THERE SHOULD BE ONLY ONE
            
            var outboundRules = updatedRuleSet.Where(r => r.Direction == RuleDirection.Outbound).ToList();

            if (outboundRules.Count != 1)
            {
                validationErrors.Add($"Multiple outbound rule(s) provided");
                ValidationUtils.ThrowIfValidationErrors("Rule update not allowed", validationErrors);
            }

            var onlyOutboundRuleFromExisting = existingRules.SingleOrDefault(r=> r.Direction == RuleDirection.Outbound);
            var onlyOutboundRuleFromClient = outboundRules.SingleOrDefault();          

            if (onlyOutboundRuleFromExisting.Name != onlyOutboundRuleFromClient.Name)
            {
                validationErrors.Add($"Illegal outbound rule(s) provided");
                ValidationUtils.ThrowIfValidationErrors("Rule update not allowed", validationErrors);
            }

            //If Sandbox is not open, make sure outbound rule has not changed
            if (curPhase > SandboxPhase.Open)
            {               
                if (onlyOutboundRuleFromClient.Direction == RuleDirection.Outbound)
                {
                    if (onlyOutboundRuleFromClient.ToString() != onlyOutboundRuleFromExisting.ToString())
                    {
                        var currentUser = await _userService.GetCurrentUserAsync();

                        if (currentUser.Admin == false)
                        {
                            validationErrors.Add($"Only admin can updated outgoing rules when Sandbox is in phase {curPhase}");
                            ValidationUtils.ThrowIfValidationErrors("Rule update not allowed", validationErrors);
                        }                    
                    }                        
                }
            }

            //VALIDATE INBOUND RULES

            foreach (var curInboundRule in updatedRuleSet.Where(r => r.Direction == RuleDirection.Inbound).ToList())
            {
                if (curInboundRule.Direction > RuleDirection.Outbound)
                {
                    validationErrors.Add($"Invalid direction for rule {curInboundRule.Description}: {curInboundRule.Direction}");
                }

                if (String.IsNullOrWhiteSpace(curInboundRule.Ip))
                {
                    validationErrors.Add($"Missing ip for rule {curInboundRule.Description}");
                }

                if (curInboundRule.Port <= 0)
                {
                    validationErrors.Add($"Invalid port for rule {curInboundRule.Description}: {curInboundRule.Port}");
                }

                if (String.IsNullOrWhiteSpace(curInboundRule.Description))
                {
                    validationErrors.Add($"Missing Description for rule {curInboundRule.Description}");
                }
            }

            ValidationUtils.ThrowIfValidationErrors("Rule update not allowed", validationErrors);
        }       

        public async Task<bool> IsInternetVmRuleSetToDeny(int vmId)
        {
            var internetRule = await GetInternetRule(vmId);

            if(internetRule == null)
            {
                throw new NotFoundException($"Could not find internet rule for VM {vmId}");
            }

            return IsRuleSetToDeny(internetRule);          
        }

        public bool IsRuleSetToDeny(VmRuleDto rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            return rule.Action == RuleAction.Deny;
        }

        public async Task<VmRuleDto> GetInternetRule(int vmId)
        {
            var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

            //Get config string
            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(vm.ConfigString);

            if (vmSettings.Rules != null)
            {
                foreach (var curRule in vmSettings.Rules)
                {
                    if (curRule.Direction == RuleDirection.Outbound)
                    {
                        if (curRule.Name.Contains(AzureVmConstants.RulePresets.OPEN_CLOSE_INTERNET))
                        {
                            return curRule;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<VmRuleDto>> GetRules(int vmId, CancellationToken cancellationToken = default)
        {
            var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Read);

            //Get config string
            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(vm.ConfigString);

            return vmSettings.Rules != null ? vmSettings.Rules : new List<VmRuleDto>();
        }

        //TODO: Probably remove
        //public async Task<VmRuleDto> AddRule(int vmId, VmRuleDto input, CancellationToken cancellationToken = default)
        //{
        //    await ValidateRuleThrowIfInvalid(vmId, input);

        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

        //    //Get existing rules from VM settings
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    ThrowIfRuleExists(vmSettings, input);

        //    input.Name = AzureResourceNameUtil.NsgRuleNameForVm(vmId);

        //    if (vmSettings.Rules == null)
        //    {
        //        vmSettings.Rules = new List<VmRuleDto>();
        //    }

        //    vmSettings.Rules.Add(input);

        //    vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //    await _db.SaveChangesAsync();

        //    await CreateUpdateOperationAndAddQueueItem(vm, "Add rule");

        //    return input;
        //}

        //TODO: Probably remove
        //public async Task<VmRuleDto> UpdateRule(int vmId, VmRuleDto input, CancellationToken cancellationToken = default)
        //{
        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

        //    //Get config string
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    if (vmSettings.Rules != null)
        //    {

        //        VmRuleDto ruleToRemove = null;

        //        var rulesDictionary = vmSettings.Rules.ToDictionary(r => r.Name, r => r);

        //        if (rulesDictionary.TryGetValue(input.Name, out ruleToRemove))
        //        {
        //            vmSettings.Rules.Remove(ruleToRemove);

        //            ThrowIfRuleExists(vmSettings, input);

        //            vmSettings.Rules.Add(input);

        //            vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //            await _db.SaveChangesAsync();

        //            await CreateUpdateOperationAndAddQueueItem(vm, "Update rule");

        //            return input;
        //        }
        //    }

        //    throw new NotFoundException($"Rule with id {input.Name} does not exist");
        //}

        //public async Task CloseInternet(int vmId, CancellationToken cancellationToken = default)
        //{
        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

        //    //Get config string
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    if (vmSettings.Rules != null)
        //    {
        //        bool ruleIsChanged = false;

        //        foreach (var curRule in vmSettings.Rules)
        //        {
        //            if (curRule.Direction == RuleDirection.Outbound)
        //            {
        //                if (curRule.Name.Contains(AzureVmConstants.RulePresets.OPEN_CLOSE_INTERNET))
        //                {
        //                    if (curRule.Action == RuleAction.Allow)
        //                    {
        //                        ruleIsChanged = true;
        //                        curRule.Action = RuleAction.Deny;
        //                    }
        //                }
        //            }
        //        }

        //        if (ruleIsChanged)
        //        {
        //            vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //            await _db.SaveChangesAsync();

        //            await CreateUpdateOperationAndAddQueueItem(vm, "Update rule");
        //        }
        //    }
        //}
        //TODO: Probably remove
        //public async Task<VmRuleDto> AddRule(int vmId, VmRuleDto input, CancellationToken cancellationToken = default)
        //{
        //    await ValidateRuleThrowIfInvalid(vmId, input);

        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

        //    //Get existing rules from VM settings
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    ThrowIfRuleExists(vmSettings, input);

        //    input.Name = AzureResourceNameUtil.NsgRuleNameForVm(vmId);

        //    if (vmSettings.Rules == null)
        //    {
        //        vmSettings.Rules = new List<VmRuleDto>();
        //    }

        //    vmSettings.Rules.Add(input);

        //    vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //    await _db.SaveChangesAsync();

        //    await CreateUpdateOperationAndAddQueueItem(vm, "Add rule");

        //    return input;
        //}

        //TODO: Probably remove
        //public async Task<VmRuleDto> UpdateRule(int vmId, VmRuleDto input, CancellationToken cancellationToken = default)
        //{
        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

        //    //Get config string
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    if (vmSettings.Rules != null)
        //    {

        //        VmRuleDto ruleToRemove = null;

        //        var rulesDictionary = vmSettings.Rules.ToDictionary(r => r.Name, r => r);

        //        if (rulesDictionary.TryGetValue(input.Name, out ruleToRemove))
        //        {
        //            vmSettings.Rules.Remove(ruleToRemove);

        //            ThrowIfRuleExists(vmSettings, input);

        //            vmSettings.Rules.Add(input);

        //            vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //            await _db.SaveChangesAsync();

        //            await CreateUpdateOperationAndAddQueueItem(vm, "Update rule");

        //            return input;
        //        }
        //    }

        //    throw new NotFoundException($"Rule with id {input.Name} does not exist");
        //}

        //public async Task CloseInternet(int vmId, CancellationToken cancellationToken = default)
        //{
        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Crud_Sandbox);

        //    //Get config string
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    if (vmSettings.Rules != null)
        //    {
        //        bool ruleIsChanged = false;

        //        foreach (var curRule in vmSettings.Rules)
        //        {
        //            if (curRule.Direction == RuleDirection.Outbound)
        //            {
        //                if (curRule.Name.Contains(AzureVmConstants.RulePresets.OPEN_CLOSE_INTERNET))
        //                {
        //                    if (curRule.Action == RuleAction.Allow)
        //                    {
        //                        ruleIsChanged = true;
        //                        curRule.Action = RuleAction.Deny;
        //                    }
        //                }
        //            }
        //        }

        //        if (ruleIsChanged)
        //        {
        //            vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //            await _db.SaveChangesAsync();

        //            await CreateUpdateOperationAndAddQueueItem(vm, "Update rule");
        //        }
        //    }
        //}
        //TODO: Probably remove
        //public async Task<VmRuleDto> DeleteRule(int vmId, string ruleId, CancellationToken cancellationToken = default)
        //{
        //    var vm = await GetVmResourceEntry(vmId, UserOperation.Study_Read);

        //    //Get config string
        //    var vmSettings = SandboxResourceConfigStringSerializer.VmSettings(vm.ConfigString);

        //    if (vmSettings.Rules != null)
        //    {

        //        VmRuleDto ruleToRemove = null;

        //        var rulesDictionary = vmSettings.Rules.ToDictionary(r => r.Name, r => r);

        //        if (rulesDictionary.TryGetValue(ruleId, out ruleToRemove))
        //        {
        //            vmSettings.Rules.Remove(ruleToRemove);

        //            vm.ConfigString = SandboxResourceConfigStringSerializer.Serialize(vmSettings);

        //            await _db.SaveChangesAsync();

        //            await CreateUpdateOperationAndAddQueueItem(vm, "Delete rule");

        //            return ruleToRemove;
        //        }
        //    }

        //    throw new NotFoundException($"Rule with id {ruleId} does not exist");
        //}        

        void ThrowIfRuleExists(List<VmRuleDto> rules, VmRuleDto ruleToCompare)
        {
            if (rules != null)
            {
                foreach (var curExistingRule in rules)
                {
                    if (AzureVmUtil.IsSameRule(ruleToCompare, curExistingRule))
                    {
                        throw new Exception($"Same rule allready exists");
                    }
                }
            }
        }

        async Task CreateUpdateOperationAndAddQueueItem(CloudResource vm, string description)
        {
            if (await _sandboxResourceOperationReadService.HasUnstartedCreateOrUpdateOperation(vm.Id))
            {
                _logger.LogWarning($"Updating VM {vm.Id}: There is allready an unstarted VM Create or Update operation. Not creating additional");
            }
            else
            {
                //If un started update allready exist, no need to create update op?
                var vmUpdateOperation = await _sandboxResourceOperationCreateService.CreateUpdateOperationAsync(vm.Id);

                var queueParentItem = new ProvisioningQueueParentDto();
                queueParentItem.SandboxId = vm.SandboxId;
                queueParentItem.Description = $"Update VM state for Sandbox: {vm.SandboxId} ({description})";

                queueParentItem.Children.Add(new ProvisioningQueueChildDto() { ResourceOperationId = vmUpdateOperation.Id });

                await _workQueue.SendMessageAsync(queueParentItem);
            }
        }
    }
}
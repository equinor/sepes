using System;
using System.Collections.Generic;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto.VirtualMachine;
using Sepes.Common.Util;
using Sepes.Infrastructure.Model;

namespace Sepes.Infrastructure.Util
{
    public static class VmRuleUtils
    {
        public static bool InternetIsOpen(CloudResource vmResource)
        {
            var relevantRule = GetInternetRule(vmResource);

            if(relevantRule == null)
            {
                return false;
            }

            return relevantRule.Action == RuleAction.Allow;
        }

        public static VmRuleDto GetInternetRule(CloudResource vmResource)
        {
            if (!String.IsNullOrWhiteSpace(vmResource.ConfigString))
            {
                var vmSettings = CloudResourceConfigStringSerializer.VmSettings(vmResource.ConfigString);

                if (vmSettings != null && vmSettings.Rules != null)
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
            }          

            return null;
        }
        
        public static List<VmRuleDto> CreateInitialVmRules(int vmId)
        {
            return new List<VmRuleDto>() { new VmRuleDto() { Name = AzureResourceNameUtil.NsgRuleNameForVm(vmId, AzureVmConstants.RulePresets.OPEN_CLOSE_INTERNET), Description = "Control outbound internet access. Set to Allow or Deny as needed!", Action = RuleAction.Deny, Direction = RuleDirection.Outbound } };
        }
    }
}
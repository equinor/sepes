
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sepes.RestApi.Model;

namespace Sepes.RestApi.Services
{
    // This service manage changes to Pods. Make sure they are validated and the correct azure actions are performed.
    // It do not own the Pod state and need to talk to StudyService to keep its up to date.
    public interface IPodService
    {
        Task<Pod> CreateNewPod(string name, int userID);
        Task<string> GetPods(int studyID);
        Task Set(Pod newPod, Pod based);
    }

    public class PodService : IPodService
    {
        private readonly ISepesDb _database;
        private readonly IAzureService _azure;

        public PodService(ISepesDb database, IAzureService azure)
        {
            _database = database;
            _azure = azure;
        }

// old
        public async Task<Pod> CreateNewPod(string name, int studyID)
        {
            var pod = await _database.createPod(name, studyID);
            var resourceGroupName = await _azure.CreateResourceGroup(pod.resourceGroupName);
            await _azure.CreateNetwork(pod.networkName, pod.addressSpace, "subnet1");
            return pod;
        }
        public async Task createNsg(string securityGroupName, string resourceGroupName)
        {
            await _azure.CreateSecurityGroup(securityGroupName, resourceGroupName);
        }
        public async Task deleteNsg(string securityGroupName, string resourceGroupName){
            await _azure.DeleteSecurityGroup(securityGroupName, resourceGroupName);
        }
        public async Task applyNsg(string resourceGroupName, string securityGroupName, string subnetName, string networkName)
        {
            //throw new NotImplementedException();
            await _azure.ApplySecurityGroup(resourceGroupName ,securityGroupName, subnetName, networkName);

        }
        public async Task removeNsg(string resourceGroupName, string subnetName, string networkName)
        {
            await _azure.RemoveSecurityGroup(resourceGroupName, subnetName, networkName);
        }
        public Task<UInt16> deleteUnused()
        {
            throw new NotImplementedException();
            //needs to check for any policies that are not currently in use by any pods/belong to deleted pods.
        }
        public async Task<string> GetPods(int studyID)
        {
            return await _database.getPods(studyID);
        }

// new
        public async Task Set(Pod newPod, Pod based)
        {
            if (based == null)
            {
                Console.WriteLine("#### Creating Resource group and network");
                Task createRes = _azure.CreateResourceGroup(newPod.resourceGroupName);
                Task createNet = _azure.CreateNetwork(newPod.networkName, newPod.addressSpace, newPod.subnetName);
                await Task.WhenAll(createRes, createNet);
            }

            var tasks = new List<Task>();

            // Create network security group and add rules, apply to subnet
            bool mustCreateNewNSG = based == null || 
                !based.incoming.SequenceEqual(newPod.incoming) ||
                !based.outgoing.SequenceEqual(newPod.outgoing);
            if (mustCreateNewNSG) tasks.Add(ManageNetworkSecurityGroup(newPod, based));

            // Add users to resources
            tasks.Add(AddUsers(newPod, based));

            await Task.WhenAll(tasks);
        }

        private async Task AddUsers(Pod newPod, Pod based)
        {
            List<Task> addUsersTasks = new List<Task>();
            foreach (var user in newPod.users)
            {
                if (based == null || !based.users.Contains(user))
                {
                    Console.WriteLine("#### Adding user "+user.userEmail);
                    Task addUserToResGroup = _azure.AddUserToResourceGroup(user.userEmail, newPod.resourceGroupName);
                    Task addUserToNetwork = _azure.AddUserToNetwork(user.userEmail, newPod.networkName);
                    addUsersTasks.Add(addUserToResGroup);
                    addUsersTasks.Add(addUserToNetwork);
                }
            }
            await Task.WhenAll(addUsersTasks.ToArray());
        }

        private async Task ManageNetworkSecurityGroup(Pod newPod, Pod based)
        {
            string nsgNameDefault = newPod.networkSecurityGroupName;
            string nsgNameDef2 = nsgNameDefault+"2";

            // Generate network security network name
            var nsgNames = await _azure.GetNSGNames(newPod.resourceGroupName);
            string nsgName = nsgNameDefault;
            if (nsgNames.Contains(newPod.networkSecurityGroupName)) nsgName = nsgNameDef2;
            
            // Create nsg with generated nsg name
            bool mustCreateNewNSG = based == null || 
                !based.incoming.SequenceEqual(newPod.incoming) ||
                !based.outgoing.SequenceEqual(newPod.outgoing);
            
            if (mustCreateNewNSG) await _azure.CreateSecurityGroup(nsgName, newPod.resourceGroupName);

            // pod.allowAll != basePod.allowAll // Apply allow all

            // Set inbound and outbound rules
            var addRuleTasks = new List<Task>();
            if (based == null || !based.incoming.SequenceEqual(newPod.incoming))
            {
                Dictionary<ushort, string[]> inbound = GenerateRuleDictionary(newPod.incoming);
                int priority = 100;

                foreach (var port in inbound.Keys) {
                    await _azure.NsgAllowInboundPort(nsgName, newPod.resourceGroupName, "Port_" + port, priority++, inbound[port], (int) port);
                    Console.WriteLine("#### Added rule inbound "+port+" "+inbound[port].ElementAt(0));
                }
            }
            if (based == null || !based.outgoing.SequenceEqual(newPod.outgoing))
            {
                Dictionary<ushort, string[]> outbound = GenerateRuleDictionary(newPod.outgoing);
                int priority = 100;

                foreach (var port in outbound.Keys) {
                    await _azure.NsgAllowOutboundPort(nsgName, newPod.resourceGroupName, "Port_" + port, priority++, outbound[port], (int) port);
                    Console.WriteLine("#### Added rule outbound "+port+" "+outbound[port].ElementAt(0));
                }
            }

            // Apply network security group to subnet
            Console.WriteLine($"#### ApplySecurityGroup() {newPod.resourceGroupName}, {nsgName}, {newPod.subnetName}, {newPod.networkName}");
            await _azure.ApplySecurityGroup(newPod.resourceGroupName, nsgName, newPod.subnetName, newPod.networkName);

            // Delete old nsg
            if (nsgName == nsgNameDefault && nsgNames.Contains(nsgNameDef2)) {
                await _azure.DeleteSecurityGroup(nsgNameDef2, newPod.resourceGroupName);
            }
            else if (nsgName == nsgNameDef2 && nsgNames.Contains(nsgNameDefault)) {
                await _azure.DeleteSecurityGroup(nsgNameDefault, newPod.resourceGroupName);
            }
        }

        private Dictionary<ushort, string[]> GenerateRuleDictionary(IEnumerable<Rule> array)
        {
            var g = array.GroupBy(r => r.port, r => r.ip, (port, ips) => new { Key = port, Value = ips});
            var ruleDict = g.ToDictionary(r => r.Key, r => r.Value.ToArray());
            return ruleDict;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

namespace Sepes.RestApi.Model
{
    //Internal version of pod model
    public class Pod
    {
        public ushort id { get; }
        public string name { get; }
        public int studyId { get; }
        public readonly bool allowAll;
        public readonly ImmutableList<Rule> incoming;
        public readonly ImmutableList<Rule> outgoing;
        public readonly ImmutableList<User> users;
        public readonly ImmutableList<DataSet> locked;
        public readonly ImmutableList<DataSet> loaded;

        public string networkName => $"{studyId}-{name.Replace(" ", "-")}-Network";
        public string subnetName => $"{studyId}-{name.Replace(" ", "-")}-SubNet";
        public string resourceGroupName => $"{studyId}-{name.Replace(" ", "-")}-ResourceGroup";
        public string networkSecurityGroupName => $"{studyId}-{name.Replace(" ", "-")}-NetworkSecurityGroup";
        public string addressSpace => $"10.{1 + id / 256}.{id % 256}.0/24";


        public Pod(ushort id, string name, int studyId, bool allowAll, IEnumerable<Rule> incoming, IEnumerable<Rule> outgoing, 
                    IEnumerable<User> users, IEnumerable<DataSet> locked, IEnumerable<DataSet> loaded)
        {
            this.id = id;
            this.name = name;
            this.studyId = studyId;
            this.allowAll = allowAll;
            this.incoming = incoming.ToImmutableList();
            this.outgoing = outgoing.ToImmutableList();
            this.users = users.ToImmutableList();
            this.locked = locked.ToImmutableList();
            this.loaded = loaded.ToImmutableList();
        }

        public Pod(ushort id, string name, int studyId)
        {
            this.id = id;
            this.name = name;
            this.studyId = studyId;
            this.allowAll = false;
            this.incoming = new List<Rule>().ToImmutableList();
            this.outgoing = new List<Rule>().ToImmutableList();
            this.users = new List<User>().ToImmutableList();
            this.locked = new List<DataSet>().ToImmutableList();
            this.loaded = new List<DataSet>().ToImmutableList();
        }

        public override bool Equals(object obj)
        {
            return obj is Pod pod &&
                   id == pod.id &&
                   name == pod.name &&
                   studyId == pod.studyId &&
                   allowAll == pod.allowAll &&
                   Enumerable.SequenceEqual(incoming, pod.incoming) &&
                   Enumerable.SequenceEqual(outgoing, pod.outgoing) &&
                   Enumerable.SequenceEqual(users, pod.users) &&
                   Enumerable.SequenceEqual(locked, pod.locked) &&
                   Enumerable.SequenceEqual(loaded, pod.loaded);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(id);
            hash.Add(name);
            hash.Add(studyId);
            hash.Add(allowAll);
            hash.Add(incoming);
            hash.Add(outgoing);
            hash.Add(users);
            hash.Add(locked);
            hash.Add(loaded);

            return hash.ToHashCode();
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Sepes.RestApi.Model;

using System.Linq;

namespace Sepes.RestApi.Services {

    // The main job of the Study Service is to own and keep track Study and Pod state.
    // This service is the only one that can change the internal representation of Study and pod.
    // It's also responsible of saving new changes. 
    public interface IStudyService
    {
        // Get the list of studies based on a user.
        IEnumerable<Study> GetStudies(User user, bool archived);

        // Makes changes to the meta data of a study.
        // If based is null it means its a new study.
        // This call will only succeed if based is the same as the current version of that study.
        // This method will only update the metadata about a study and will not make changes to the azure resources.
        Task<Study> Save(Study newStudy, Study based);
    }

    public class StudyService: IStudyService {

        ISepesDb _db;
        IPodService _podService;
        HashSet<Study> _studies;
        ushort numberOfPods;


        public StudyService(ISepesDb dbService, IPodService podService) {
            _db = dbService;
            _podService = podService;

            _studies = new HashSet<Study>();
            numberOfPods = 0;
        }

        public IEnumerable<Study> GetStudies(User user, bool archived)
        {
            return _studies.Where(study => study.archived == archived);
        }

        public async Task<Study> Save(Study newStudy, Study based)
        {
            Study study = newStudy;

            if (based == null)
            {
                study = await _db.NewStudy(newStudy);
                _studies.Add(study);
            }
            else if (_studies.Contains(based))
            {
                study = UpdatePods(based, study);

                _studies.Remove(based);
                _studies.Add(study);
            }

            return study;
        }

        private Study UpdatePods(Study based, Study study)
        {
            foreach (var pod in study.pods)
            {
                if (!based.pods.Contains(pod))
                {
                    // Check if pod is new
                    if (!pod.id.HasValue)
                    {
                        // Update list of pod
                        // generate new pod with id
                        Pod newPod = pod.NewPodId(numberOfPods++);
                        var newPods = study.pods.Remove(pod).Add(newPod);
                        study = study.ReplacePods(newPods);

                        // Update Azure with Pod Service
                        // _podService.Set(newPod, null);
                    }
                    else
                    {
                        Pod basePod = based.pods.ToList().Find(basePod => basePod.id == pod.id);
                        // _podService.Set(pod, basePod);
                    }
                }
            }

            return study;
        }

        public void LoadStudies()
        {
            _studies = _db.GetAllStudies().Result.ToHashSet();
            numberOfPods = (ushort) _studies.Sum(study => study.pods.Count);
        }
    }

}

namespace Sepes.RestApi.Model
{
    public class DataSetDB
    {
        public string displayName { get; set; }
        public string opaPolicy { get; set; }
        public string azureReference { get; set; }

        public DataSet ToDataSet() => new DataSet(displayName, opaPolicy, azureReference);
    }
}

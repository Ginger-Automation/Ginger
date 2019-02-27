using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Amdocs.Ginger.CoreNET.RosLynLib.Refrences
{
    public class ValueExpressionReference
    {

        public string Category { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public string UseCase { get; set; }
        public List<string> Samples { get; set; }
        public string ReturnType { get; set; }
        public string ExpressionResult { get; set; }
        public string IconImageName { get; set; }

        public ValueExpressionReference()
        {
            Samples = new List<string>();

        }
    }

    public class VEReferenceList
    {
        public List<ValueExpressionReference> Refrences = new List<ValueExpressionReference>();

        public static VEReferenceList LoadFromJson(string JsonFilePath)
        {
            return JsonConvert.DeserializeObject<VEReferenceList>(System.IO.File.ReadAllText(JsonFilePath));
        }
        public void SavetoJson(string JsonFilePath)
        {
            string output = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(JsonFilePath, output);
        }
    }
}

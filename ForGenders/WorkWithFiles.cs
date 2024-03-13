using Newtonsoft.Json;

namespace ApiTests
{
    public class WorkWithFiles
    {

        public List<int> ReadIdsFromFile(string gender)
        {
            var fileName = $"{gender}_ids.txt";
            var json = File.ReadAllText(fileName);
            var idList = JsonConvert.DeserializeObject<List<int>>(json);
            return idList;
        }

        public void SaveIdsToFile(string gender, List<int> idList)
        {
            var fileName = $"{gender}_ids.txt";
            File.WriteAllText(fileName, JsonConvert.SerializeObject(idList));
            Console.WriteLine($"Saved IDs for {gender} to {fileName}");
        }
    }
}
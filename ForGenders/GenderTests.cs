using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ApiTests
{
    public class UserTests
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task TestUserGender(string gender)
        {
            var url = $"https://hr-challenge.dev.tapyou.com/api/test/users?gender={gender}";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ApiResponse>(responseBody);

            Assert.IsTrue(result.IsSuccess, "API call was not successful");

            SaveIdsToFile(gender, result.IdList);
        }

        private void SaveIdsToFile(string gender, List<int> idList)
        {
            var fileName = $"{gender}_ids.txt";
            File.WriteAllText(fileName, JsonConvert.SerializeObject(idList));
            Console.WriteLine($"Saved IDs for {gender} to {fileName}");
        }

        [TestCase("any")]
        [TestCase("male")]
        [TestCase("female")]
        public void TestUserDetails(string gender)
        {
            var ids = ReadIdsFromFile(gender);
            var errorMessages = new List<string>();

            foreach (var id in ids)
            {
                var errorMessage = TestUserDetail(id, gender).Result; // Получаем результат асинхронного метода
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    errorMessages.Add($"Error for ID {id}: {errorMessage}");
                }
            }

            if (errorMessages.Count > 0)
            {
                Assert.Fail($"Errors encountered:\n{string.Join("\n", errorMessages)}");
            }
        }

        private List<int> ReadIdsFromFile(string gender)
        {
            var fileName = $"{gender}_ids.txt";
            var json = File.ReadAllText(fileName);
            var idList = JsonConvert.DeserializeObject<List<int>>(json);
            return idList;
        }

        private async Task<string> TestUserDetail(int id, string expectedGender)
        {
            var url = $"https://hr-challenge.dev.tapyou.com/api/test/user/{id}";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Request failed with status code: {response.StatusCode}";
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UserApiResponse>(responseBody);

                if (!result.IsSuccess)
                {
                    return $"API call was not successful. Error: {result.ErrorMessage}";
                }
                if (result.User == null)
                {
                    return $"User with ID {id} was not found.";
                }

                if (id != result.User.Id)
                {
                    return $"Returned ID {result.User.Id} does not match the requested ID.";
                }

                if (expectedGender != "any" && expectedGender != result.User.Gender)
                {
                    return $"Gender does not match. Expected: {expectedGender}, but was: {result.User.Gender}";
                }
            }
            catch (HttpRequestException e)
            {
                return $"HttpRequestException. Message: {e.Message}";
            }
            catch (Exception e)
            {
                return $"Unexpected exception. Message: {e.Message}";
            }

            return null; // Возвращаем null, если тест прошел успешно без ошибок
        }

        public class ApiResponse
        {
            public bool IsSuccess { get; set; }
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
            public List<int> IdList { get; set; }
        }

        public class UserApiResponse
        {
            public bool IsSuccess { get; set; }
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
            public User User { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public int Age { get; set; }
            public string City { get; set; }
        }
    }
}

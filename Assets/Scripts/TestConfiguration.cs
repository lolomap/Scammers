using Newtonsoft.Json;
using UnityEngine;

public class TestConfiguration
{
    public static TestConfiguration Instance { get; private set; }

    public class Api
    {
        public string url;
        public string cert;
    }
    public class Links
    {
        public string website;
        public string personalData;
        public string personalDataConsent;
        public string userAgreement;
        public string advertisementConsent;
    }

    public class Credentials
    {
        public string login;
        public string password;
    }
    public class User
    {
        public string id;
        public string jwt_token;
    }

    public Api api;
    public Links links;

    public Credentials credentials;
    public User user;
        

    public static void Init()
    {
        if (Instance != null) return;

        string json = Resources.Load<TextAsset>("TestConfiguration").text;
        Instance = JsonConvert.DeserializeObject<TestConfiguration>(json);
    }
}
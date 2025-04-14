using Newtonsoft.Json;
using UnityEngine;

public class Configuration
{
    public static Configuration Instance { get; private set; }

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

    public Api api;
    public Links links;

    public static void Init()
    {
        if (Instance != null) return;

        string json = Resources.Load<TextAsset>("Configuration").text;
        Instance = JsonConvert.DeserializeObject<Configuration>(json);
        //Instance.api.cert = Resources.Load<TextAsset>(Instance.api.cert).text;
    }
}
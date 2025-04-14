#define USE_SSL
#undef USE_SSL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

#if USE_SSL
public class ApiCertHandler : CertificateHandler
{
    private static X509Certificate2 _rootCert;

    public ApiCertHandler()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
#if DEBUG
            GameManager.WaitCoroutine(LoadStreamingCert(TestConfiguration.Instance.api.cert));
#else
            GameManager.WaitCoroutine(loadStreamingCert(Configuration.Instance.api.cert));
#endif
        }
        else
        {
#if DEBUG
            _rootCert = new(Application.streamingAssetsPath + "/" + TestConfiguration.Instance.api.cert);
#else
            _rootCert = new(Application.streamingAssetsPath + "/" + Configuration.Instance.api.cert);
#endif
        }
    }

    private static IEnumerator LoadStreamingCert(string fileName)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        byte[] result = null;

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            //It just does not work with UnityWebRequest
            WWW www = new(filePath);
            yield return www;
            result = www.bytes;
        }

        _rootCert = new(result);
    }

    protected override bool ValidateCertificate(byte[] certificateData)
    {
        X509Chain chain = new();

        X509Certificate2 certificate = new(certificateData);

        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        chain.ChainPolicy.VerificationTime = DateTime.Now;
        chain.ChainPolicy.UrlRetrievalTimeout = new(0, 0, 0);

        // Adding known root
        chain.ChainPolicy.ExtraStore.Add(_rootCert);

        bool isChainValid = chain.Build(certificate);
        if (!isChainValid)
        {
            string[] errors = chain.ChainStatus
                .Select(x => $"{x.StatusInformation.Trim()} ({x.Status})")
                .ToArray();
            string certificateErrorsString = "Unknown errors.";

            if (errors.Length > 0)
            {
                certificateErrorsString = string.Join(", ", errors);
            }

            Debug.LogError  ("Trust chain did not complete to the known authority anchor. Errors: " + certificateErrorsString);
        }

        foreach (X509ChainElement element in chain.ChainElements)
        {
            foreach (X509ChainStatus status in element.ChainElementStatus)
            {
                if (status.Status == X509ChainStatusFlags.UntrustedRoot)
                {
                    // improvement: we could validate that the request matches an internal domain by using request.RequestUri in addition to the certicate validation

                    // Check that the root certificate matches one of the valid root certificates
                    if (_rootCert.RawData.SequenceEqual(element.Certificate.RawData))
                        continue; // Process the next status
                }

                return false;
            }
        }

        // Return true only if all certificates of the chain are valid
        return true;
    }
}
#endif

public class Request<T> where T : IResponseData
{
    public WebAPI.Action Action;
    public IRequestData Data;
    public string UserId = "";
    public string Token = "";

    public delegate void ResponseHandler(T response, long statusCode);
    public event ResponseHandler Success;

    public delegate void ErrorHandler(string error, long statusCode);
    public event ErrorHandler Error;

    public Request(WebAPI.Action action, IRequestData data = null)
    {
        Action = action;
        Data = data;
    }

    public void OnSuccess(T response, long statusCode) { Success?.Invoke(response, statusCode); }
    public void OnError(string error, long statusCode) { Error?.Invoke(error, statusCode); }
}

public interface IRequestData { }
public interface IResponseData { }

public class WebAPI
{
#if DEBUG
    private static readonly string _url = TestConfiguration.Instance.api.url;
#else
        private static readonly string _url = Configuration.Instance.api.url;
#endif



    public static IEnumerator<UnityWebRequestAsyncOperation> Get<T>(Request<T> action) where T : IResponseData
    {
        string destination = _url + ActionMappings[action.Action].Replace("{id}", action.UserId.ToString());
        using UnityWebRequest request = UnityWebRequest.Get(destination);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("accept", "application/json");
        if (action.Token != "")
            request.SetRequestHeader("Authorization", "Bearer " + action.Token);
        #if USE_SSL
request.certificateHandler = new ApiCertHandler();
#endif

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            action.OnSuccess(JsonConvert.DeserializeObject<T>(json), request.responseCode);
        }
        else
        {
            string json = request.downloadHandler.text;
            action.OnError(json, request.responseCode);
        }
    }

    public static IEnumerator<UnityWebRequestAsyncOperation> Put<T>(Request<T> action) where T : IResponseData
    {
        string destination = _url + ActionMappings[action.Action].Replace("{id}", action.UserId.ToString());
        string jsonData = JsonConvert.SerializeObject(action.Data,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        Debug.Log(jsonData);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = UnityWebRequest.Put(destination, data);
        //request.uploadHandler = new UploadHandlerRaw(data);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("accept", "application/json");
        if (action.Token != "")
            request.SetRequestHeader("Authorization", "Bearer " + action.Token);
        #if USE_SSL
request.certificateHandler = new ApiCertHandler();
#endif

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            action.OnSuccess(JsonConvert.DeserializeObject<T>(json), request.responseCode);
        }
        else
        {
            string json = request.downloadHandler.text;
            action.OnError(json, request.responseCode);
        }
    }

    public static IEnumerator<UnityWebRequestAsyncOperation> Post<T>(Request<T> action) where T : IResponseData
    {
        string destination = _url + ActionMappings[action.Action].Replace("{id}", action.UserId.ToString());
        string jsonData = JsonConvert.SerializeObject(action.Data,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        Debug.Log(jsonData);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = UnityWebRequest.PostWwwForm(destination, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("accept", "application/json");
        if (action.Token != "")
            request.SetRequestHeader("Authorization", "Bearer " + action.Token);
        #if USE_SSL
request.certificateHandler = new ApiCertHandler();
#endif

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            action.OnSuccess(JsonConvert.DeserializeObject<T>(json), request.responseCode);
        }
        else
        {
            string json = request.downloadHandler.text;
            action.OnError(json, request.responseCode);
        }
    }

        

    #region Schemas

    public class BlankResponse : IResponseData { }

    #endregion

    public enum Action
    {
        
    }
    private static readonly Dictionary<Action, string> ActionMappings = new()
    {
        /*{ Action.AuthRegister, "/auth/register" },*/
    };

    private static string SerializeDate(DateTime date)
    {
        string res = date.Year.ToString() + "-";
        if (date.Month < 10) res += "0";
        res += date.Month.ToString() + "-";
        if (date.Day < 10) res += "0";
        res += date.Day.ToString();
        return res;
    }
}
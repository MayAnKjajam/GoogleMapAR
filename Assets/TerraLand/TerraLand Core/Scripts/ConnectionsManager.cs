using UnityEngine;
using UnityEditor;
using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

//#if UNITY_EDITOR
//[InitializeOnLoad]
public static class ConnectionsManager
{
    public static void SetAsyncConnections()
    {
        ServicePointManager.UseNagleAlgorithm = true;
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.CheckCertificateRevocationList = true;
        ServicePointManager.DefaultConnectionLimit = 100;

        Configuration config = ConfigurationManager.OpenMachineConfiguration();
        System.Web.Configuration.ProcessModelSection processModelSection = (System.Web.Configuration.ProcessModelSection)config.GetSection("system.web/processModel");
        processModelSection.AutoConfig = false;
        processModelSection.MaxWorkerThreads = 100;
        processModelSection.MaxIOThreads = 100;
        processModelSection.MinWorkerThreads = 50;
        processModelSection.MaxWorkerThreads = 50;

        ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
    }

    static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;

        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);

                    if (!chainIsValid)
                        isOk = false;
                }
            }
        }

        return isOk;
    }
}
//#else
public static class ConnectionsManagerRuntime
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void SetAsyncConnections()
    {
        ServicePointManager.UseNagleAlgorithm = true;
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.CheckCertificateRevocationList = true;
        ServicePointManager.DefaultConnectionLimit = 100;

        Configuration config = ConfigurationManager.OpenMachineConfiguration();
        System.Web.Configuration.ProcessModelSection processModelSection = (System.Web.Configuration.ProcessModelSection)config.GetSection("system.web/processModel");
        processModelSection.AutoConfig = false;
        processModelSection.MaxWorkerThreads = 100;
        processModelSection.MaxIOThreads = 100;
        processModelSection.MinWorkerThreads = 50;
        processModelSection.MaxWorkerThreads = 50;

        ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
    }

    static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;

        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);

                    if (!chainIsValid)
                        isOk = false;
                }
            }
        }

        return isOk;
    }
}
//#endif


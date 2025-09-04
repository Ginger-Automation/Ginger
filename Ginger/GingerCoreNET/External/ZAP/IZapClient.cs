using OWASPZAPDotNetAPI;

namespace Amdocs.Ginger.CoreNET.External.ZAP
{
    public interface IZapClient
    {
        IApiResponse Version();
        IApiResponse RecordsToScan();
        IApiResponse GenerateReport(string title, string template, string theme, string description, string contextName, string sites, string sections, string includedconfidences, string includedrisks, string reportfilename, string reportfilenamepattern, string reportdir, string display);
        IApiResponse AccessUrl(string site, string recurse);
        IApiResponse Urls(string baseUrl);
        IApiResponse Scan(string url, string recurse, string inscopeonly,
                          string scanpolicyname, string method, string postdata, string contextId);
        IApiResponse ScanStatus(string scanId);
        IApiResponse AlertsSummary(string targetUrl);
    }

}

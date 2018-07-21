//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.ginger.SourceControl;

import com.amdocs.devops.ginger.GingerProjectHelper;
import com.amdocs.devops.ginger.SourceControl.*;
import com.sun.jna.platform.FileUtils;
import java.io.BufferedOutputStream;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.Authenticator;
import java.net.HttpURLConnection;
import java.net.InetSocketAddress;
import java.net.PasswordAuthentication;
import java.net.Proxy;
import java.net.URL;
import java.net.URLConnection;
import java.nio.channels.Channels;
import java.nio.channels.ReadableByteChannel;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Date;
import java.util.Iterator;
import java.util.List;
import java.util.logging.Logger;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;
import javax.net.ssl.HttpsURLConnection;
import org.apache.commons.codec.binary.Base64;
import org.apache.commons.codec.language.bm.Languages.SomeLanguages;
import org.apache.http.HttpEntity;
import org.apache.http.HttpHost;
import org.apache.http.HttpResponse;
import org.apache.http.HttpStatus;
import org.apache.http.auth.AuthScope;
import org.apache.http.auth.UsernamePasswordCredentials;
import org.apache.http.client.CredentialsProvider;
import org.apache.http.client.HttpClient;
import org.apache.http.client.config.RequestConfig;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.auth.BasicScheme;
import org.apache.http.impl.client.BasicCredentialsProvider;
import org.apache.http.impl.client.BasicResponseHandler;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
import org.apache.http.params.*;
import org.apache.http.protocol.BasicHttpContext;
import org.apache.http.protocol.HttpContext;
import org.apache.commons.*;
import org.tmatesoft.svn.core.ISVNDirEntryHandler;
import org.tmatesoft.svn.core.SVNDepth;
import org.tmatesoft.svn.core.SVNDirEntry;
import org.tmatesoft.svn.core.SVNException;
import org.tmatesoft.svn.core.SVNNodeKind;
import org.tmatesoft.svn.core.SVNURL;
import org.tmatesoft.svn.core.auth.BasicAuthenticationManager;
import org.tmatesoft.svn.core.auth.ISVNAuthenticationManager;
import org.tmatesoft.svn.core.internal.io.dav.DAVRepositoryFactory;
import org.tmatesoft.svn.core.io.SVNRepository;
import org.tmatesoft.svn.core.io.SVNRepositoryFactory;
import org.tmatesoft.svn.core.wc.ISVNOptions;
import org.tmatesoft.svn.core.wc.SVNClientManager;
import org.tmatesoft.svn.core.wc.SVNLogClient;
import org.tmatesoft.svn.core.wc.SVNRevision;
import org.tmatesoft.svn.core.wc.SVNStatus;
import org.tmatesoft.svn.core.wc.SVNStatusClient;
import org.tmatesoft.svn.core.wc.SVNWCUtil;
import org.tmatesoft.svn.core.wc2.SvnCheckout;
import org.tmatesoft.svn.core.wc2.SvnGetInfo;
import org.tmatesoft.svn.core.wc2.SvnOperationFactory;
import org.tmatesoft.svn.core.wc2.SvnRevert;
import org.tmatesoft.svn.core.wc2.SvnTarget;
import org.tmatesoft.svn.core.wc2.SvnUpdate;

public class GitHelper implements ISourceControlHelper
{
	// TODO maybe have a boolean flag wherever to checkout to java.io.tmpdir ?
	//SvnOperationFactory			svnOperationFactory		= new SvnOperationFactory( );
	//SVNURL						repositoryURL			= null;
	//ISVNOptions					options					= null;
	//ISVNAuthenticationManager	authManager				= null;
	boolean						isInitialized			= false;
	private String				connectionErrorMEssage	= "";
	List<String>				svnSolutions			= new ArrayList<String>();
	String						userName				= "";
	String						passWord				= "";
	String						proxyServer				= "";
	String						proxyPort				= "";
	HttpClient					httpClient 				= null;	
	String 						URL 					= ""; 
	List<String>				projectEnvs				= new ArrayList<String>();
	List<String>				runSetConfigs			= new ArrayList<String>();
	HttpHost					proxy 					= null;
	String 						solutionName   			= "";
	//HttpsHost					proxy1 					= new HttpsHost("genproxy.amdocs.com", 8080, "https");
	//Common 						commFunc				= new Common();
		
	public GitHelper()
	{		
		
	}

	public GitHelper( String url , String userName , String password , String ProxyServer, String ProxyPort )
	{
		this( );
		try
		{
			this.userName = userName;
			this.passWord = password;
			this.proxyServer = ProxyServer.replace("http://", "");
			this.proxyPort = ProxyPort;
					
			this.URL = url;
			if(!ProxyPort.isEmpty())
				proxy = new HttpHost(proxyServer,Integer.parseInt(ProxyPort),"http");
			
			CredentialsProvider credsProvider = new BasicCredentialsProvider( );
			if(userName!="" && password!="")
			{
				credsProvider.setCredentials( AuthScope.ANY , new UsernamePasswordCredentials( userName , password ) );	
			}
			
			httpClient = HttpClients.custom( ).setDefaultCredentialsProvider( credsProvider ).build( );				
			RequestConfig config = RequestConfig.custom()
	                .setProxy(proxy)
	                .build();
			
			HttpGet getMethod = new HttpGet( url );
			if(URL.contains("github") && proxy != null)
				getMethod.setConfig(config);
			getMethod.setHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");    
			
		        
			HttpResponse response = httpClient.execute( getMethod );
			
			if ( response.getStatusLine( ).getStatusCode( ) != HttpStatus.SC_OK )
			{
				connectionErrorMEssage = response.getStatusLine( ).getReasonPhrase( );
			}
			else
			{								
				isInitialized = true;
			}
		}
		catch ( Exception ex )
		{
			connectionErrorMEssage = ex.toString();
		}
	}

	public String getConnectionErrorMEssage()
	{
		return connectionErrorMEssage;
	}

	public boolean isConnectionValid()
	{
		return isInitialized;
	}

	public List<String> getSolutionsList()
	{
		return svnSolutions;
	}
	
	public List<String> getRunSetList()
	{
		return runSetConfigs;
	}
	
	public List<String> getEnvsList()
	{
		return projectEnvs;
	}
	
	public boolean isFilesystemUnderSVN( String path )
	{
		boolean result = false;	
		return result;
	}

	public void update( String fromUrl , String toFolder )
	{
	}
	
	public void fetchSolutionDetails(String solName)
	{		
		solutionName=solName;
		if(solName.endsWith(".git"))
			solutionName = solName.substring(0, solName.length()-4);
		if(URL.endsWith(".git"))
			URL = URL.substring(0, URL.length()-4);
		fetchSolutionEnvDetails();
		fetchSolutionRunSet();
	}
	
	public void fetchSolutionRunSet()
	{
		try
		{
			runSetConfigs.clear();
			RequestConfig config = RequestConfig.custom()
	                .setProxy(proxy)
	                .build();
			HttpGet getMethod= null;
			String solBrowse="";
			if(!URL.endsWith(solutionName))
			{
				solBrowse=solutionName + "/";
			}
			
			if(URL.contains("github"))
				 getMethod = new HttpGet( URL + "/tree/master/" + solBrowse + "RunSetConfigs" );
			else if(URL.contains("codecloud.web.att.com"))
				 getMethod = new HttpGet( URL + "/" + solBrowse+ "RunSetConfigs" );
			
			if(URL.contains("github") && proxy!=null)
				getMethod.setConfig(config);
			
			getMethod.setHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");
			HttpResponse response = httpClient.execute( getMethod );
			
			//getRunSet details
			
			if ( response.getStatusLine( ).getStatusCode( ) == HttpStatus.SC_OK )
			{
				String responseString = new BasicResponseHandler().handleResponse(response);
				System.out.println(responseString);		
				
				Pattern pattern = Pattern.compile("RunSetConfigs/(.+?).Ginger.RunSetConfig.xml");
				Matcher matcher = pattern.matcher(responseString);
				while (matcher.find()) {
				    System.out.println("group 1: " + matcher.group(1));
				    runSetConfigs.add(matcher.group(1).replace("%20", " "));
				}
			}		
		}		
		catch ( Exception ex )
		{
			connectionErrorMEssage = ex.toString();
		}
	}
	
	public void fetchSolutionEnvDetails()
	{
		try
		{			
			projectEnvs.clear();
			
			RequestConfig config = RequestConfig.custom()
	                .setProxy(proxy)
	                .build();
			HttpGet getMethod= null;
			
			String solBrowse="";
			if(!URL.endsWith(solutionName))
			{
				solBrowse=solutionName + "/";
			}
			
			if(URL.contains("github"))
				 getMethod = new HttpGet( URL + "/tree/master/" + solBrowse + "Environments" );
			else if(URL.contains("codecloud.web.att.com"))
				 getMethod = new HttpGet( URL + "/" + solBrowse+ "Environments" );
			
			if(URL.contains("github") && proxy !=null)
				getMethod.setConfig(config);
			getMethod.setHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");
			
			HttpResponse response = httpClient.execute( getMethod );
			
			if ( response.getStatusLine( ).getStatusCode( ) == HttpStatus.SC_OK )
			{
				String responseString = new BasicResponseHandler().handleResponse(response);
				System.out.println(responseString);	
				
				Pattern pattern = Pattern.compile("Environments/(.+?).Ginger.Environment.xml");
				Matcher matcher = pattern.matcher(responseString);
				while (matcher.find()) {
				    System.out.println("group 1: " + matcher.group(1));
				    projectEnvs.add(matcher.group(1).replace("%20", " "));
				}				
			}		
		}		
		catch ( Exception ex )
		{
			connectionErrorMEssage = ex.toString();
		}
	}
	public void checkOut( String fromUrl , String toFolder ) throws Exception
	{
	}
	
	public void updateAndRevert( String toFolder )
	{
	}


	public void getAllSolutions()
	{
		SVNRepository repository = null;
		String path = "";
	}
	
	public void getAllSolutionsByHTMLReq()
	{
		try{
			svnSolutions.clear();
			
			RequestConfig config = RequestConfig.custom()
	                .setProxy(proxy)
	                .build();
			
			HttpGet getMethod = new HttpGet( URL );
			if(URL.contains("github") && proxy != null)
				getMethod.setConfig(config);
			getMethod.setHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");
			HttpResponse response = httpClient.execute( getMethod );
			
			if ( response.getStatusLine( ).getStatusCode( ) != HttpStatus.SC_OK )
			{
				connectionErrorMEssage = response.getStatusLine( ).getReasonPhrase( );
			}
			else
			{								
				String responseString = new BasicResponseHandler().handleResponse(response);
				System.out.println(responseString);								
				if(responseString.contains("/Ginger.Solution.xml"))
				{					
					String solutionName = URL.substring(URL.lastIndexOf("/")+1);
					svnSolutions.add(solutionName);					
				}
				else
				{					
					String match="";					
					if(URL.contains("github"))
						match ="/tree/master/";
					else if(URL.contains("codecloud.web.att.com"))
						match ="/browse/";

					Pattern pattern = Pattern.compile(match + "(.+?)\"");
					Matcher matcher = pattern.matcher(responseString);
					while (matcher.find()) {
						svnSolutions.add(matcher.group(1));
					    System.out.println("group 1: " + matcher.group(1));
					   // runSetConfigs.add(matcher.group(1).replace("%20", " "));
					   // matcher = matcher.NextMatch();
					}					
				}
				String match="\"href\":\"" + URL + "\".*\"self\":.*\"href\":\"(.+?)\"";
				Pattern pattern = Pattern.compile(match);
				Matcher matcher = pattern.matcher(responseString);
				while (matcher.find()) {
					URL=matcher.group(1);				    
				}	
			}			
		}		
		catch (Exception ex){}       
	}
}

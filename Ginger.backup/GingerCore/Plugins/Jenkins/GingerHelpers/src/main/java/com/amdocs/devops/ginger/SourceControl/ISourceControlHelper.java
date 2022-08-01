//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.ginger.SourceControl;

import java.util.ArrayList;
import java.util.List;
import org.apache.http.HttpHost;

import org.apache.http.client.HttpClient;

public interface ISourceControlHelper
{
	// TODO maybe have a boolean flag wherever to checkout to java.io.tmpdir ?

	//SvnOperationFactory			svnOperationFactory		= new SvnOperationFactory( );
	//SVNURL						repositoryURL			= null;
	//ISVNOptions					options					= null;
	//ISVNAuthenticationManager	authManager				= null;
	boolean						isInitialized			= false;
	//String						connectionErrorMEssage	= "";
	List<String>				svnSolutions			= new ArrayList<String>();
	String						userName				= "";
	String						passWord				= "";
	String						proxyServer				= "";
	String						proxyPort				= "";
	HttpClient					httpClient 				= null;
	String 						URL 					= ""; 
	List<String>				projectEnvs				= new ArrayList<String>();
	List<String>				runSetConfigs			= new ArrayList<String>();
	//HttpHost					proxy 					= new HttpHost("genproxy.amdocs.com", 8080, "http");	
	

	public String getConnectionErrorMEssage();	

	public boolean isConnectionValid();	

	public List<String> getSolutionsList();
	
	public List<String> getRunSetList();	
	
	public List<String> getEnvsList();	
	
	public boolean isFilesystemUnderSVN( String path );	

	public void update( String fromUrl , String toFolder );
	
	public void fetchSolutionDetails(String solutionName);
	
	public void fetchSolutionRunSet();
	
	public void fetchSolutionEnvDetails();
	
	public void checkOut( String fromUrl , String toFolder )throws Exception;

	public void updateAndRevert( String toFolder );

	public void getAllSolutions();
	
	public void getAllSolutionsByHTMLReq();		
}

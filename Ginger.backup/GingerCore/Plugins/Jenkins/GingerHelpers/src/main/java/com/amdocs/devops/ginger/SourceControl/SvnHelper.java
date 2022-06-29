//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.ginger.SourceControl;

import com.amdocs.devops.ginger.GingerProjectHelper;
import com.amdocs.devops.ginger.SourceControl.*;
import java.io.BufferedReader;
import java.io.File;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.Authenticator;
import java.net.HttpURLConnection;
import java.net.PasswordAuthentication;
import java.net.URL;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Date;
import java.util.Iterator;
import java.util.List;
import java.util.UUID;
import java.util.logging.Logger;
import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.HttpStatus;
import org.apache.http.auth.AuthScope;
import org.apache.http.auth.UsernamePasswordCredentials;
import org.apache.http.client.CredentialsProvider;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.auth.BasicScheme;
import org.apache.http.impl.client.BasicCredentialsProvider;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
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

public class SvnHelper implements ISourceControlHelper
{
	// TODO maybe have a boolean flag wherever to checkout to java.io.tmpdir ?

	SvnOperationFactory			svnOperationFactory		= new SvnOperationFactory( );
	SVNURL						repositoryURL			= null;
	ISVNOptions					options					= null;
	ISVNAuthenticationManager	authManager				= null;
	boolean						isInitialized			= false;
	private String				connectionErrorMEssage	= "";
	List<String>				svnSolutions			= new ArrayList<String>();
	List<String>				projectEnvs				= new ArrayList<String>();
	List<String>				runSetConfigs			= new ArrayList<String>();
	String						userName				= "";
	String						passWord				= "";
	String						proxyServer				= "";
	String						proxyPort				= "";
	
	private GingerProjectHelper	gingerProjectHelper;
	
	public SvnHelper()
	{
		DAVRepositoryFactory.setup( );
		options = SVNWCUtil.createDefaultOptions( true );
	}

	public SvnHelper( String url , String userName , String password , String ProxyServer, String ProxyPort )
	{
		this( );
		try
		{

			this.userName = userName;
			this.passWord = password;
			this.proxyServer = ProxyServer;
			this.proxyPort = ProxyPort;
			
			if(url.toUpperCase().indexOf("/SVN/") ==-1  && url.toUpperCase().indexOf("/SVN") == -1)
				if(url.toUpperCase().endsWith("/"))
					url = url+ "svn/";
				else
					url = url+ "/svn/";
			
			if(!url.toUpperCase().endsWith("/"))
				url = url+ "/";
			
			authManager = BasicAuthenticationManager.newInstance( userName , password.toCharArray( ) );
			svnOperationFactory = new SvnOperationFactory( );
			svnOperationFactory.setAuthenticationManager( authManager );

			repositoryURL = SVNURL.parseURIEncoded( url );

			CredentialsProvider credsProvider = new BasicCredentialsProvider( );
			credsProvider.setCredentials( AuthScope.ANY , new UsernamePasswordCredentials( userName , password ) );

			HttpClient httpClient = HttpClients.custom( ).setDefaultCredentialsProvider( credsProvider ).build( );
			HttpGet getMethod = new HttpGet( url );
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
		catch ( SVNException e )
		{
			connectionErrorMEssage = e.getMessage( );
		}
		catch ( Exception ex )
		{
			connectionErrorMEssage = ex.getMessage( );
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
		SVNStatusClient svnStatusClient = new SVNStatusClient( svnOperationFactory );
		try
		{
			SVNStatus svnStatus = svnStatusClient.doStatus( new File( path ) , false );
			result = svnStatus.isVersioned( );

		}
		catch ( Exception e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}

		return result;
	}

	public void update( String fromUrl , String toFolder )
	{
	}

	public boolean isPathUnderSVN1( String path )
	{
		// TODO rename method , not clear
		boolean methodReturn = false;
		try
		{
			SVNRepository svnRepository = SVNRepositoryFactory.create( repositoryURL );
			svnRepository.setAuthenticationManager( authManager );
			svnRepository.testConnection( );

			SVNNodeKind svnNodeKind = svnRepository.checkPath( path  , -1 );
			if ( svnNodeKind != SVNNodeKind.NONE )
			{
				methodReturn = true;
			}

		}
		catch ( Exception ex )
		{
			Logger.getAnonymousLogger( ).info( ex.getMessage( ) );
		}
		return methodReturn;
	}

	public void checkOut( String fromUrl , String toFolder ) throws Exception
	{
		final SvnCheckout checkout = svnOperationFactory.createCheckout( );

		// TODO add check if path esists
		checkout.setSingleTarget( SvnTarget.fromFile( new File( toFolder ) ) );
		SvnTarget fromSvnURL = null;

		fromSvnURL = SvnTarget.fromURL( SVNURL.parseURIEncoded( repositoryURL.toDecodedString( ) + "/" + fromUrl ) );

		checkout.setSource( fromSvnURL );

		checkout.run( );
	}

	public void updateAndRevert( String toFolder )
	{
		try
		{
			final SvnRevert svnRevert = svnOperationFactory.createRevert( );
			svnRevert.addTarget( SvnTarget.fromFile( new File( toFolder ) ) );
			svnRevert.setDepth( SVNDepth.INFINITY );
			svnRevert.run( );

			final SvnUpdate svnUpdate = svnOperationFactory.createUpdate( );

			svnUpdate.addTarget( SvnTarget.fromFile( new File( toFolder ) ) );
			svnUpdate.setDepth( SVNDepth.INFINITY );

			long[] updatedToRevision = svnUpdate.run( );

			Logger.getAnonymousLogger( ).info( "Updated to latest revision:" + String.valueOf( updatedToRevision[0] ) );

		}
		catch ( SVNException e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}
	}

	private void initSVNClientForExampleOnly( File workspace )
	{

		final List<String> apiVersions = new ArrayList<String>( );
		try
		{

			SVNRevision revision = SVNRevision.HEAD;

			File solutionName = null;
			repositoryURL = SVNURL.parseURIEncoded( "http://cmitechint1srv.corp.amdocs.com:81/svn/" + solutionName );
			SVNURL url = SVNURL.create( "http" , "ATT:Ginger" , "cmitechint1srv" , 81 , "/svn/" + solutionName , false );
			File dstPath = new File( workspace.getParent( ) + "/" + solutionName );
			SVNRevision pegRevision = SVNRevision.create( new Date( ) );
			SVNRevision revision1 = SVNRevision.create( new Date( ) );
			svnOperationFactory.setAuthenticationManager( authManager );

			final SvnGetInfo svnInfo = svnOperationFactory.createGetInfo( );
			svnInfo.addTarget( SvnTarget.fromURL( SVNURL.parseURIEncoded( "http://cmitechint1srv.corp.amdocs.com:81/svn/" + solutionName ) ) );
			svnInfo.run( );
			final SvnCheckout checkout = svnOperationFactory.createCheckout( );
			checkout.setSingleTarget( SvnTarget.fromFile( dstPath ) );
			checkout.setSource( SvnTarget.fromURL( repositoryURL ) );

			final SvnUpdate svnUpdate = svnOperationFactory.createUpdate( );
			checkout.run( );
		}
		catch ( Exception e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}
	}
	
	public void getAllSolutions()
	{
		SVNRepository repository = null;
		String path = "";
		try
		{

			SVNLogClient svnClient = SVNClientManager.newInstance(null, authManager).getLogClient();

			  svnClient.doList(repositoryURL, SVNRevision.HEAD, SVNRevision.HEAD,true,false					  
					  , new ISVNDirEntryHandler() { 
			      			public void handleDirEntry(SVNDirEntry entry) throws SVNException { 
			      				svnSolutions.add(entry.getName()); 
			      } 
			  }); 
			  
			  
			repository = SVNRepositoryFactory.create(repositoryURL);
			repository.setAuthenticationManager( authManager );
			
			SVNNodeKind nodeKind = repository.checkPath("", -1);
            if (nodeKind == SVNNodeKind.NONE) {

            } else if (nodeKind == SVNNodeKind.FILE) {

            }
            else{
			System.out.println( "Repository Root: " + repository.getRepositoryRoot( true ) );
			System.out.println(  "Repository UUID: " + repository.getRepositoryUUID( true ) );
			
			
			Collection entries = repository.getDir( path, -1 , null , (Collection) null );
			Iterator iterator = entries.iterator( );
			           while ( iterator.hasNext( ) ) {
			               SVNDirEntry entry = ( SVNDirEntry ) iterator.next( );
			               svnSolutions.add(entry.getName());
			           }
            }
		}
		catch (SVNException svne)
		{
			
		}
	}
	public void fetchSolutionDetails(String solutionName)
	{	
		
		File tempCheckoutDir = new File( System.getProperty( "java.io.tmpdir" ) + "/GingerPlugin/" + UUID.randomUUID( ).toString( ).replaceAll( "-" , "" ) );

		try
		{
			checkOut( solutionName + "/Environments" , tempCheckoutDir.getAbsolutePath( )+"/Environments" );
			checkOut( solutionName + "/RunSetConfigs" , tempCheckoutDir.getAbsolutePath( )+"/RunSetConfigs" );
		}
		catch ( Exception e )
		{
			e.printStackTrace( );
		}

		gingerProjectHelper = new GingerProjectHelper( tempCheckoutDir.getAbsolutePath( ) );
		
		fetchSolutionRunSet();
		fetchSolutionEnvDetails();
	}
		
	public void fetchSolutionRunSet()
	{	
		runSetConfigs = this.gingerProjectHelper.getRunSetConfigs( );
	}
	
	public void fetchSolutionEnvDetails()
	{	
		projectEnvs = this.gingerProjectHelper.getProjectEnvironments();
	}
	
	public void getAllSolutionsByHTMLReq()
	{
		svnSolutions.clear();	
		CloseableHttpClient httpclient = null;
		try
		{
	    	String url = repositoryURL.toString();
	    	
			CredentialsProvider credsProvider = new BasicCredentialsProvider();
	        credsProvider.setCredentials(
	                new AuthScope(null, -1),
	                new UsernamePasswordCredentials(userName, passWord));
	        httpclient = HttpClients.custom()
	                .setDefaultCredentialsProvider(credsProvider)
	                .build();
	        try 
	        {
	            HttpGet httpget = new HttpGet(url);
	
	            System.out.println("Executing request " + httpget.getRequestLine());
	            CloseableHttpResponse response = httpclient.execute(httpget);
	            HttpEntity responseEntity = response.getEntity();
	            
	            try 
	            {
	            	
	    			BufferedReader in = new BufferedReader(
	    			        new InputStreamReader(responseEntity.getContent()));
	    			String inputLine;
	    			StringBuffer responseFromUrl = new StringBuffer();
	    	
	    			while ((inputLine = in.readLine()) != null) 
	    			{
	    				if(inputLine.contains("dir name"))
	    				{
	    					int sIndex = inputLine.indexOf("dir name");
	    					int eIndex = inputLine.indexOf(" href") - 1;
	    					String solutionName = inputLine.substring(sIndex+10, eIndex);
	    					svnSolutions.add(solutionName);
	    				}
	    				responseFromUrl.append(inputLine);
	    			}
	    			in.close();
	            } 
	            finally 
	            {
	                response.close();
	            }
	        }
	        catch(Exception ex)
	        {
	        	
	        }
		}
		catch (Exception ex){}
        finally 
        {
            try {
				httpclient.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
        }
	}
}

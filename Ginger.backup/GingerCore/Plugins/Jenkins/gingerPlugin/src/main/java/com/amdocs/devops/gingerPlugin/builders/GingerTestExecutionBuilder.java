//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.gingerPlugin.builders;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.Serializable;
import java.util.List;
import javax.servlet.ServletException;
import org.kohsuke.stapler.AncestorInPath;
import org.kohsuke.stapler.DataBoundConstructor;
import org.kohsuke.stapler.QueryParameter;
import org.kohsuke.stapler.StaplerRequest;
import com.amdocs.devops.ginger.SourceControl.*;
import com.amdocs.devops.gingerPlugin.tasks.GingerRemoteTask;
import hudson.AbortException;
import hudson.EnvVars;
import hudson.Extension;
import hudson.FilePath;
import hudson.Launcher;
import hudson.model.AbstractProject;
import hudson.model.AutoCompletionCandidates;
import hudson.model.ModelObject;
import hudson.model.Run;
import hudson.model.TaskListener;
import hudson.remoting.Callable;
import hudson.tasks.BuildStepDescriptor;
import hudson.tasks.Builder;
import hudson.util.FormValidation;
import jenkins.tasks.SimpleBuildStep;
import net.sf.json.JSONObject;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.InetSocketAddress;
import java.net.Proxy;
import java.net.URL;
import java.net.URLConnection;
import java.nio.channels.Channels;
import java.nio.channels.ReadableByteChannel;

public class GingerTestExecutionBuilder extends Builder implements SimpleBuildStep,Serializable
{
	private static final long	serialVersionUID	= 1L;
	private String				gingerInstallationPath;
	private String				remoteSolutionPath;
	private String				runSetName			= null;
	private String				targetEnvCode		= null;
	private String				solutionName		= null;
	private String				scType				= null;
	private String				scUrl				= null;
	private String				scUsername			= null;
	private String				scPassword			= null;
	private String				scProxyServer		= null;
	private String				scProxyPort			= null;
	public String				tmpSolutionName		= null;
	public String				tmpTargetEnvCode	= null;
	public String				tmpRunSetName		= null;
	
	private static final String USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
	
	@DataBoundConstructor
	public GingerTestExecutionBuilder( String gingerInstallationPath , String remoteSolutionPath, String runSetName , String targetEnvCode , String solutionName , String scType, String scUrl , String scUsername , String scPassword , String scProxyServer , String scProxyPort )
	{
		super( );
		this.gingerInstallationPath = gingerInstallationPath;
		this.remoteSolutionPath = remoteSolutionPath;
		this.runSetName = runSetName;
		this.targetEnvCode = targetEnvCode;
		this.solutionName = solutionName;
		this.scType = scType;
		this.scUrl = scUrl;
		this.scUsername = scUsername;
		this.scPassword = scPassword;
		this.scProxyServer = scProxyServer;
		this.scProxyPort = scProxyPort;

	}

	@Override
	public DescriptorImpl getDescriptor()
	{
		DescriptorImpl descriptorImpl = ( DescriptorImpl ) super.getDescriptor( );
		return descriptorImpl;
	}

	public String getScType()
	{
		return scType;
	}

	public void setScType( String scType )
	{
		this.scType = scType;
	}
	
	public String getScUrl()
	{
		return scUrl;
	}

	public void setScUrl( String scUrl )
	{
		this.scUrl = scUrl;
	}

	public String getScUsername()
	{
		return scUsername;
	}

	public void setScUsername( String scUsername )
	{
		this.scUsername = scUsername;
	}

	public String getScPassword()
	{
		return scPassword;
	}

	public void setScPassword( String scPassword )
	{
		this.scPassword = scPassword;
	}

	public String getScProxyServer()
	{
		return scProxyServer;
	}

	public void setScProxyServer( String scProxyServer )
	{
		this.scProxyServer = scProxyServer;
	}

	public String getScProxyPort()
	{
		return scProxyPort;
	}

	public void setScProxyPort( String scProxyPort )
	{
		this.scProxyPort = scProxyPort;
	}
	
	public void setGingerInstallationPath( String gingerInstallationPath )
	{
		this.gingerInstallationPath = gingerInstallationPath;
	}
	
	public void setRemoteSolutionPath( String remoteSolutionPath )
	{
		this.remoteSolutionPath = remoteSolutionPath;
	}
	
	public void setSolutionName( String solutionName )
	{
		this.solutionName = solutionName;
	}

	public String getGingerInstallationPath()
	{
		return gingerInstallationPath;
	}

	public String getRemoteSolutionPath()
	{
		return remoteSolutionPath;
	}
	
	public String getSolutionName()
	{
		return solutionName;
	}

	public String getRunSetName()
	{
		return runSetName;
	}

	public void setRunSetName( String runSetName )
	{
		this.runSetName = runSetName;
	}

	public String getTargetEnvCode()
	{
		return targetEnvCode;
	}

	public void setTargetEnvCode( String targetEnvCode )
	{
		this.targetEnvCode = targetEnvCode;
	}

	@Override
	public void perform( Run< ? , ? > build , final FilePath workspace , final Launcher launcher , final TaskListener listener ) throws AbortException
	{
		String retCode = "-1";

		try
		{
			EnvVars envVars = new EnvVars();
			envVars = build.getEnvironment(listener);
			UpdateParametersValues(envVars);
			String remoteFilePath = null;
			String solFolder="";
			if(getScType().equalsIgnoreCase("GIT") && !tmpSolutionName.endsWith(".git"))
			{
				solFolder =getScUrl().substring(getScUrl().lastIndexOf("/")+1) + "\\" ;
			}
			System.out.println("solFolder::" + solFolder);
			if(!getRemoteSolutionPath().isEmpty())
			{
				remoteFilePath =  getRemoteSolutionPath() +  "\\" + solFolder + tmpSolutionName;		
			}
			else
			{
				remoteFilePath =  workspace.getRemote( ) + "\\" + solFolder + tmpSolutionName;
			}
			System.out.println("remoteFilePath::" + remoteFilePath);					
			Callable<String,IOException> task = new GingerRemoteTask( workspace , listener , getGingerInstallationPath( ) , getScType() ,getScUrl() , getScUsername(), getScPassword(), getRemoteSolutionPath(),getScProxyServer(),getScProxyPort(),tmpSolutionName, remoteFilePath , tmpRunSetName , tmpTargetEnvCode);			
			retCode = launcher.getChannel( ).call( task );		
		}
		catch ( IOException e )
		{
			e.printStackTrace( listener.getLogger( ) );
		}

		catch ( Exception e )
		{
			e.printStackTrace( listener.getLogger( ) );
		}
		finally{
		}
		
		if(!retCode.equals("0"))
		{
			throw new AbortException(retCode);
		}
		// TODO thorws here .
	}
	
	private void RevertParametrsValues()
	{
		if(tmpSolutionName!=null)
		{
			setSolutionName(tmpSolutionName);
		}
		
		if(tmpRunSetName!=null)
		{
			setRunSetName(tmpRunSetName);
		}
		if(tmpTargetEnvCode!=null)
		{
			setTargetEnvCode(tmpTargetEnvCode);
		}
	}

	private void UpdateParametersValues(EnvVars env)
	{
		String SolutionName = getSolutionName();
		String targetEnvCode = getTargetEnvCode( );
		String runsetName = getRunSetName( );
		
		if(SolutionName.startsWith("$"))
		{
			if(env.containsKey(SolutionName.substring(1)))
			{
				tmpSolutionName=env.get(SolutionName.substring(1));				
			}
		}
		else
		{
			tmpSolutionName=SolutionName;
		}
		
		if(targetEnvCode.startsWith("$"))
		{
			if(env.containsKey(targetEnvCode.substring(1)))
			{
				tmpTargetEnvCode=env.get(targetEnvCode.substring(1));	
			}
		}
		else
		{
			tmpTargetEnvCode=targetEnvCode;
		}
		
		if(runsetName.startsWith("$"))
		{
			if(env.containsKey(runsetName.substring(1)))
			{
				tmpRunSetName=env.get(runsetName.substring(1));
			}
		}
		else
		{
			tmpRunSetName=runsetName;
		}
	}
	@Extension
	public static final class DescriptorImpl extends BuildStepDescriptor<Builder>
	{
		private ISourceControlHelper scHelper;
		private boolean				isConnectionTested	= false;
		private boolean				isSolutionParsed	= false;
		
		public DescriptorImpl()
		{
			load( );
		}

		public boolean isApplicable( Class< ? extends AbstractProject> aClass )
		{
			// this step can be used within all job types
			return true;
		}

		public String getDisplayName()
		{
			return "Configure Ginger execution parameters";
		}

		@Override
		public boolean configure( StaplerRequest req , JSONObject formData ) throws FormException
		{

			save( );
			return super.configure( req , formData );
		}

		public FormValidation doCheckGingerInstallationPath( @QueryParameter String value , @AncestorInPath AbstractProject project , @AncestorInPath ModelObject context ) throws IOException , ServletException
		{
			if ( value.length( ) == 0 )
				return FormValidation.error( "Write something here , cannot be blank" );

			if ( value.contains( "$" ) )
			{
				return FormValidation.ok( "Value is parametrized , cannot evaluate at this moment." );
			}

			if ( !value.endsWith( "Ginger.exe" ) )
			{
				return FormValidation.error( "Expected full path including Ginger.exe" );
			}

			return FormValidation.ok( );
		}
		
		private void sendGet() throws Exception {
			
			Proxy proxy = new Proxy(Proxy.Type.HTTP, new InetSocketAddress("genproxy.amdocs.com", 8080/*port*/));
			String url = "http://www.google.com/search?q=mkyong";

			URL obj = new URL(url);
			HttpURLConnection con = (HttpURLConnection) obj.openConnection(proxy);

			// optional default is GET
			con.setRequestMethod("GET");

			//add request header
			con.setRequestProperty("User-Agent", USER_AGENT);

			int responseCode = con.getResponseCode();
			System.out.println("\nSending 'GET' request to URL : " + url);
			System.out.println("Response Code : " + responseCode);

			BufferedReader in = new BufferedReader(
			        new InputStreamReader(con.getInputStream()));
			String inputLine;
			StringBuffer response = new StringBuffer();

			while ((inputLine = in.readLine()) != null) {
				response.append(inputLine);
			}
			in.close();

			//print result
			System.out.println(response.toString());

		}
		public boolean transferData(String url, String filename) throws Exception {

			long transferedSize = getFileSize(filename);
			Proxy proxy = new Proxy(Proxy.Type.HTTP, new InetSocketAddress("genproxy.amdocs.com",8080));
			URL website = new URL(url);
			
			URLConnection connection = website.openConnection(proxy);
			connection.setRequestProperty("Accept-Encoding", "identity"); // <--- Add this line
			connection.setRequestProperty("Range", "bytes="+transferedSize+"-");
			ReadableByteChannel rbc = Channels.newChannel(connection.getInputStream());
			long remainingSize = connection.getContentLength();
			long buffer = Byte.MAX_VALUE;
			if (remainingSize > 65536) {
			buffer = 1 << 16;
			} System.out.println("Remaining size: " + remainingSize);

			if (transferedSize == remainingSize) {
			System.out.println("File is complete");
			rbc.close();
			return true;
			}

			FileOutputStream fos = new FileOutputStream(filename, true);

			System.out.println("Continue downloading at " + transferedSize);
			while (remainingSize > 0) {
			long delta = fos.getChannel().transferFrom(rbc, transferedSize, buffer);
			transferedSize += delta;
			System.out.println(transferedSize + " bytes received");
			if (delta == 0) {
			break;
			}
			}
			fos.close();
			System.out.println("Download incomplete, retrying");

			return false;
		}
		
		public long getFileSize(String file) {
			File f = new File(file);
			System.out.println("Size: " + f.length());
			return f.length();
		}
		
		// HTTP POST request
		private void sendPost() throws Exception
		{		
			String localFile = "c:\\test_jas\\Ginger-Bell-Canada-GIT-master.zip";
			String url = "https://github.com/JaspreetAmdocs/Ginger-Bell-Canada-GIT.git";
			System.out.println("Downloading " + localFile);

				File targetFile = new File("c:\\test_jas\\Ginger-Bell-Canada-GIT-master.zip");
				
				URL website = new URL("https://github.com/JaspreetAmdocs/Ginger-Bell-Canada-GIT.git");
				
				Proxy proxy = new Proxy(Proxy.Type.HTTP, new InetSocketAddress("genproxy.amdocs.com",8080));
				URLConnection con =website.openConnection(proxy);
					ReadableByteChannel rbc = Channels.newChannel(con.getInputStream());
					
				    FileOutputStream outputStream = new FileOutputStream(targetFile);
				    
				    System.out.println("DownloadComplete");
				    outputStream.getChannel().transferFrom(rbc, 0, Long.MAX_VALUE);
		}
		
		public FormValidation doTestConnection( @QueryParameter String scType , @QueryParameter String scUrl , @QueryParameter String scUsername ,  @AncestorInPath ModelObject context ,@QueryParameter String scPassword, @QueryParameter String scProxyServer, @QueryParameter String scProxyPort)
		{
			isConnectionTested = false;				
			System.out.println("Testing 1 - Send Http GET request");
			System.out.println("\nTesting 2 - Send Http POST request");
			
			if(scUrl.endsWith("/"))
				scUrl= scUrl.substring(0, scUrl.length()-1);
			if(scType.equalsIgnoreCase("GIT"))
				scHelper = new GitHelper( scUrl , scUsername , scPassword , scProxyServer , scProxyPort);
			else
				scHelper = new SvnHelper( scUrl , scUsername , scPassword , scProxyServer , scProxyPort);
			
			if ( scHelper.isConnectionValid( ) )
			{
				isConnectionTested = true;
				scHelper.getAllSolutionsByHTMLReq();
				return FormValidation.ok( "Source Control Connection is valid" );
			}
			else
			{
				return FormValidation.error( scHelper.getConnectionErrorMEssage( ) );
			}
		}

		public FormValidation dofetchSolutionDetails( @QueryParameter String solutionName , @AncestorInPath AbstractProject project , @AncestorInPath ModelObject context )
		{
			isSolutionParsed = false;

			if ( scHelper == null || !scHelper.isConnectionValid( ) )
			{
				return FormValidation.error( "Please validate source control (2) connectivity before fetching solution" );
			}
			
			scHelper.fetchSolutionDetails(solutionName);
			isSolutionParsed = true;			

			return FormValidation.ok( "Solution was parsed succesfully ,  start typing in  'RunSet Name' and 'Environment Name'  text boxes to get results from the actual solution.<br/>Type . (DOT) to get all results" );
		}
		
		public AutoCompletionCandidates doAutoCompleteSolutionName( @QueryParameter String value )
		{
			return this.scHelper == null ? new AutoCompletionCandidates( ) : getAutoCompleteResults( this.scHelper.getSolutionsList( ) , value );
		}

		public AutoCompletionCandidates doAutoCompleteRunSetName( @QueryParameter String value )
		{
			return isSolutionParsed == false ? new AutoCompletionCandidates( ) : getAutoCompleteResults( this.scHelper.getRunSetList() , value );
		}

		public AutoCompletionCandidates doAutoCompleteTargetEnvCode( @QueryParameter String value )
		{
			return isSolutionParsed == false ? new AutoCompletionCandidates( ) : getAutoCompleteResults( this.scHelper.getEnvsList() , value );
		}

		private AutoCompletionCandidates getAutoCompleteResults( List<String> allPossibleResults , String searchForValue )
		{
			AutoCompletionCandidates autoCompletionCandidates = new AutoCompletionCandidates( );
			if ( allPossibleResults == null )				
			{
				return autoCompletionCandidates;
			}
			if ( searchForValue.equals( "." ) )
			{
				for ( String result : allPossibleResults )
				{
					autoCompletionCandidates.add( result );
				}
			}
			else
			{
				for ( String result : allPossibleResults )
				{
					if ( result.toLowerCase( ).startsWith( searchForValue.toLowerCase( ) ) )
					{
						autoCompletionCandidates.add( result );
					}
				}
			}
			return autoCompletionCandidates;
		}

		public ISourceControlHelper getScHelper()
		{
			return scHelper;
		}

		public void setScHelper( ISourceControlHelper scHelper )
		{
			this.scHelper = scHelper;
		}

		/**
		 * 
		 * 
		 * public ListBoxModel doFillRunSetNameItems( @QueryParameter String value , @AncestorInPath AbstractProject project , @AncestorInPath ModelObject context ) {
		 * 
		 * ListBoxModel m = new ListBoxModel( ); if ( !isSolutionParsed ) { m.add( "Solution changed , please trigger solution rescan by pressing 'Get Solution details' button" ); } else { }
		 * 
		 * return m; }
		 * 
		 * public ListBoxModel doFillTargetEnvCodeItems( @QueryParameter String value , @AncestorInPath AbstractProject project , @AncestorInPath ModelObject context ) { ListBoxModel m = new ListBoxModel( ); if ( !isSolutionParsed ) { m.add( "Solution changed , please trigger solution rescan by pressing 'Get Solution details' button" ); } else { }
		 * 
		 * return m; } public ListBoxModel doFillStateItems( @QueryParameter String country ) { ListBoxModel m = new ListBoxModel( ); for ( String s : new String[] { "A" , "B" , "C" } ) m.add( String.format( "State %s in %s" , s , country ) , country + ':' + s ); return m; }
		 * 
		 * public ListBoxModel doFillCityItems( @QueryParameter String country , @QueryParameter String state ) { ListBoxModel m = new ListBoxModel( ); for ( String s : new String[] { "X" , "Y" , "Z" } ) m.add( String.format( "City %s in %s %s" , s , state , country ) , state + ':' + s ); return m; }
		 * 
		 * 
		 * Performs on-the-fly validation of the form field 'name'.
		 *
		 * @param value
		 *            This parameter receives the value that the user has typed.
		 * @return Indicates the outcome of the validation. This is sent to the browser.
		 *         <p>
		 *         Note that returning { FormValidation#error(String)} does not prevent the form from being saved. It just means that a message will be displayed to the user.
		 * 
		 *         public FormValidation doCheckName( @QueryParameter String value ) throws IOException , ServletException { if ( value.length( ) == 0 ) return FormValidation.error( "Please set a name" ); if ( value.length( ) < 4 ) return FormValidation.warning( "Isn't the name too short?" ); return FormValidation.ok( ); }
		 */

		/**
		 * This method returns true if the global configuration says we should speak French.
		 *
		 * The method name is bit awkward because global.jelly calls this method to determine the initial state of the checkbox by the naming convention.
		 * 
		 * 
		 * public FormValidation doCheckGingerInstallationPath( @QueryParameter String value ) throws IOException , ServletException { if ( value.length( ) == 0 ) return FormValidation.error( "Please set a name" ); if ( value.length( ) < 4 ) return FormValidation.warning( "Isn't the name too short?" ); return FormValidation.ok( "doCheckgingerInstallationPath" ); }
		 * 
		 * public FormValidation doCheckTargetEnvCode( @QueryParameter String value ) throws IOException , ServletException { if ( value.length( ) == 0 ) return FormValidation.error( "Please set a name" ); if ( value.length( ) < 4 ) return FormValidation.warning( "Isn't the name too short?" ); return FormValidation.ok( "doCheckTargetEnvCode" ); }
		 * 
		 * public FormValidation doCheckRunSetName( @QueryParameter String value ) throws IOException , ServletException { if ( value.length( ) == 0 ) return FormValidation.error( "Please set a name" ); if ( value.length( ) < 4 ) return FormValidation.warning( "Isn't the name too short?" ); return FormValidation.ok( "doCheckRunSetName" ); }
		 */
	}
}
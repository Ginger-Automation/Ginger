//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.gingerPlugin.tasks;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.Serializable;
import org.jenkinsci.remoting.RoleChecker;
import hudson.FilePath;
import hudson.Launcher.LocalLauncher;
import hudson.Launcher.ProcStarter;
import hudson.model.TaskListener;
import hudson.remoting.Callable;
import com.amdocs.devops.ginger.General.*;

public class GingerRemoteTask implements Callable<String,IOException>,Serializable
{

	/**
	 * 
	 */
	private static final long	serialVersionUID		= 3831087175308345786L;
	private static final String	ENCRYPTION_KEY			= "D3^hdfr7%ws4Kb56=Qt";
	FilePath					workspace				= null;
	TaskListener				listener				= null;
	String						runSetName				= null;
	String						targetEnvCode			= null;
	String						solutionName			= null;
	String						gingerInstallationPath	= null;
	String						remoteSolutionPath		= null;
	String						solutionFullPath		= null;
	String						sourceControlType		= null;
	String						sourceControlUrl		= null;
	String						sourceControlUser		= null;
	String						sourceControlPswd		= null;
	String						sourceControlProxyServer= null;
	String						sourceControlProxyPort	= null;
	

	public GingerRemoteTask( final FilePath workspace , final TaskListener listener , String gingerInstallationPath , String sourceControlType,  String sourceControlUrl, String sourceControlUser, String sourceControlPswd, String remoteSolutionPath, String sourceControlProxyServer, String sourceControlProxyPort, String solutionName , String solutionFullPath , String runsetName , String targetEnv )
	{

		setWorkspace( workspace );

		setListener( listener );
		setGingerInstallationPath( gingerInstallationPath );
		setRemoteSolutionPath(remoteSolutionPath);
		setSolutionName( solutionName );
		setRunSetName( runsetName );
		setTargetEnvCode( targetEnv );
		setSolutionFullPath( solutionFullPath );
		setSourceControlType(sourceControlType);
		setSourceControlUrl(sourceControlUrl);
		setSourceControlUser(sourceControlUser);
		setSourceControlPswd(sourceControlPswd);
		setSourceControlProxyServer(sourceControlProxyServer);
		setSourceControlProxyPort(sourceControlProxyPort);		
	}

	public FilePath getWorkspace()
	{
		return workspace;
	}

	public void setWorkspace( FilePath workspace )
	{
		this.workspace = workspace;
	}

	public TaskListener getListener()
	{
		return listener;
	}

	public void setListener( TaskListener listener )
	{
		this.listener = listener;
	}

	@Override
	public void checkRoles( RoleChecker checker ) throws SecurityException
	{
		// TODO Auto-generated method stub

	}

	public String getGingerInstallationPath()
	{
		return gingerInstallationPath;
	}

	public void setGingerInstallationPath( String gingerInstallationPath )
	{
		this.gingerInstallationPath = gingerInstallationPath;
	}
	
	public String getSourceControlType()
	{
		return sourceControlType;
	}

	public void setSourceControlType( String sourceControlType )
	{
		this.sourceControlType = sourceControlType;
	}
	
	public String getSourceControlUrl()
	{
		return sourceControlUrl;
	}

	public void setSourceControlUrl( String sourceControlUrl )
	{
		this.sourceControlUrl = sourceControlUrl;
	}
	
	public String getSourceControlUser()
	{
		return sourceControlUser;
	}

	public void setSourceControlUser( String sourceControlUser )
	{
		this.sourceControlUser = sourceControlUser;
	}
	
	public String getSourceControlPswd()
	{
		return sourceControlPswd;
	}

	public void setSourceControlPswd( String sourceControlPswd )
	{
		this.sourceControlPswd = sourceControlPswd;
	}
	
	public String getSourceControlProxyServer()
	{
		return sourceControlProxyServer;
	}

	public void setSourceControlProxyServer( String sourceControlProxyServer )
	{
		this.sourceControlProxyServer = sourceControlProxyServer;
	}
	
	public String getSourceControlProxyPort()
	{
		return sourceControlProxyPort;
	}

	public void setSourceControlProxyPort( String sourceControlProxyPort )
	{
		this.sourceControlProxyPort = sourceControlProxyPort;
	}
	
	public String getRemoteSolutionPath()
	{
		return remoteSolutionPath;
	}

	public void setRemoteSolutionPath( String remoteSolutionPath )
	{
		this.remoteSolutionPath = remoteSolutionPath;
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

	public String getSolutionName()
	{
		return solutionName;
	}

	public void setSolutionName( String solutionName )
	{
		this.solutionName = solutionName;
	}

	public String getSolutionFullPath()
	{
		return solutionFullPath;
	}

	public void setSolutionFullPath( String solutionFullPath )
	{
		this.solutionFullPath = solutionFullPath;
	}
			
	@Override
	public String call() throws IOException
	{
		// This code will run on the build slave
		String solutionPath = getSolutionFullPath( );
		String targetEnvCode = getTargetEnvCode( );
		String runsetName = getRunSetName( );
				
		String sourceControlType = getSourceControlType( );
		String sourceControlUrl = getSourceControlUrl( );
		String sourceControlUser = getSourceControlUser( );
		String sourceControlPswd = getSourceControlPswd( );
		String sourceControlProxyServer = getSourceControlProxyServer( );
		String sourceControlProxyPort = getSourceControlProxyPort( );
		
		
		int retCode = -1;
		


		StringBuilder commandBuilder = new StringBuilder( );

		commandBuilder.append( gingerInstallationPath );
		commandBuilder.append( " " );
		final String tempFileName = workspace + "\\tempConfig.xml";
		BufferedWriter writer = null;

		StringBuilder configFileBuilder = new StringBuilder( );
		
		try{
			EncryptionHandler encHandler= new EncryptionHandler();
			sourceControlPswd = encHandler.encrypt(sourceControlPswd,ENCRYPTION_KEY);	
		}
		catch(Exception e)
		{
			
		}
		configFileBuilder.append( "SourceControlType=" + sourceControlType + "\n" );
		configFileBuilder.append( "SourceControlUrl=" + sourceControlUrl + "\n" );
		configFileBuilder.append( "SourceControlUser=" + sourceControlUser + "\n" );
		configFileBuilder.append( "SourceControlPassword=" + sourceControlPswd + "\n" );		
		configFileBuilder.append( "PasswordEncrypted=Y\n" );		
		configFileBuilder.append( "SourceControlProxyServer=" + sourceControlProxyServer + "\n" );
		configFileBuilder.append( "SourceControlProxyPort=" + sourceControlProxyPort + "\n" );
		
		configFileBuilder.append( "Solution=" + solutionPath + "\n" );
		configFileBuilder.append( "Env=" + targetEnvCode + "\n" );
		configFileBuilder.append( "RunSet=" + runsetName + "\n" );

		try
		{

			writer = new BufferedWriter( new FileWriter( tempFileName ) );
			writer.write( configFileBuilder.toString( ) );
			writer.flush( );
		}
		catch ( IOException e )
		{
		}
		finally
		{
			try
			{
				if ( writer != null )
					writer.close( );
			}
			catch ( IOException e )
			{
				e.printStackTrace( );
			}
		}
		commandBuilder.append( "ConfigFile=\"" + new File( tempFileName ).getAbsolutePath( )+"\"" );
		listener.getLogger( ).println( "Executing" + commandBuilder.toString( ) );

		ProcStarter proc = new LocalLauncher( listener ).launch( );
		proc.cmdAsSingleString( commandBuilder.toString( ) );

		proc.stderr( listener.getLogger( ) );
		proc.stdout( listener.getLogger( ) );

		try
		{
			retCode = proc.join( );
		}
		catch ( InterruptedException e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}
		
		return String.valueOf(retCode);
	}
}
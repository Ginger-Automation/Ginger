//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.ginger;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.logging.Logger;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.xml.sax.SAXException;

public class GingerProjectHelper
{
	// TODO LOGGING LOGGING LOGGING
	// TODO Add environment folder handling
	// TODO add eager parsing of both envs and runsets
	Logger			logger				= Logger.getLogger( "GingerProjectHelper" );
	String			fileSystemRootPath	= null;
	List<String>	projectEnvs;
	List<String>	runSetConfigs;

	public GingerProjectHelper( String filesystemRootPath )
	{		
		this.fileSystemRootPath = filesystemRootPath;
		parseProject();
	}

	public List<String> getProjectEnvironments()
	{
		return projectEnvs;
	}

	public List<String> getRunSetConfigs()
	{
		return runSetConfigs;
	}
	
	private List<String> updateListFromFolder(File dir, String fileExt)
	{
		File[] runSetDescriptors = dir.listFiles( );
		List<String> obj = new ArrayList<String>();

		for ( File fileDescriptor : runSetDescriptors )
		{
			if (fileDescriptor.isDirectory( ))
			{
				List<String> obj2 = new ArrayList<String>();
				obj2 = updateListFromFolder(fileDescriptor, fileExt);
				obj.addAll(obj2);
			}
			if ( !fileDescriptor.isDirectory( ) && fileDescriptor.getAbsolutePath( ).endsWith( fileExt ) )
			{
				if(fileExt.endsWith(".Ginger.RunSetConfig.xml"))
				{
					obj.add( getSingleRunsetConfig( fileDescriptor ) );
				}
				else if(fileExt.endsWith(".Ginger.Environment.xml"))
				{
					obj.add( getSingleEnvInfo( fileDescriptor ) );
				}
			}
			
		}
		return obj;
	}

	private void parseProject()
	{
		runSetConfigs = new ArrayList<String>( );
		File runSetDescriptors = new File( fileSystemRootPath + "/RunSetConfigs/" );
		runSetConfigs = updateListFromFolder(runSetDescriptors, ".Ginger.RunSetConfig.xml");
		
		projectEnvs = new ArrayList<String>( );
		File envFiles = new File( fileSystemRootPath + "/Environments/" );
		projectEnvs = updateListFromFolder(envFiles, ".Ginger.Environment.xml");
	}

	private String getSingleRunsetConfig( File runSetDescriptor )
	{
		String envName = "";
		// TODO outer Factory and builder initialization
		try
		{
			DocumentBuilderFactory factory =

					DocumentBuilderFactory.newInstance( );
			DocumentBuilder builder = null;
			builder = factory.newDocumentBuilder( );
			Document document = builder.parse( new FileInputStream( runSetDescriptor ) );

			Node node = document.getElementsByTagName( "Ginger.Run.RunSetConfig" ).item( 0 );// TODO externalize
			
			if(node== null)
				node = document.getElementsByTagName( "RunSetConfig" ).item( 0 );

			if(node != null)
				envName = node.getAttributes( ).getNamedItem( "Name" ).getNodeValue( );
		}
		catch ( SAXException e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}
		catch ( IOException e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}
		catch ( ParserConfigurationException e )
		{
			// TODO Auto-generated catch block
			e.printStackTrace( );
		}

		return envName;// TODO
	}

	private String getSingleEnvInfo( File envDescriptorFile )
	{
		String envName = "";
		// TODO outer Factory and builder initialization
		try
		{
			DocumentBuilderFactory factory =

					DocumentBuilderFactory.newInstance( );
			DocumentBuilder builder = null;
			builder = factory.newDocumentBuilder( );
			Document document = builder.parse( new FileInputStream( envDescriptorFile ) );

			Node node = document.getElementsByTagName( "GingerCore.Environments.ProjEnvironment" ).item( 0 );// TODO externalize
			
			if(node == null)
				node = document.getElementsByTagName( "ProjEnvironment" ).item( 0 );// TODO externalize
			
			if(node != null)
				envName = node.getAttributes( ).getNamedItem( "Name" ).getNodeValue( );
		}
		catch ( SAXException e )
		{

			e.printStackTrace( );
		}
		catch ( IOException e )
		{

			e.printStackTrace( );
		}
		catch ( ParserConfigurationException e )
		{

			e.printStackTrace( );
		}

		return envName;
	}
}

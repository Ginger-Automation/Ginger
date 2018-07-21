//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.ginger.helpers.tests;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import net.lingala.zip4j.core.ZipFile;
import net.lingala.zip4j.exception.ZipException;
import org.junit.After;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import com.amdocs.devops.ginger.GingerProjectHelper;

public class GingerHelperParseTest
{
	private final String		demoSolutionLocation	= "src/test/resources/DemoGingerSolution.zip";

	@Rule
	public TemporaryFolder		testFolder				= new TemporaryFolder( );
	private GingerProjectHelper	helper;

	@Before
	public void beforeTest() throws Exception
	{

		File demoSolutionFile = new File( demoSolutionLocation );
		try
		{
			ZipFile zipFile = new ZipFile( demoSolutionFile );

			zipFile.extractAll( testFolder.getRoot( ).getAbsolutePath( ) );

			helper = new GingerProjectHelper( testFolder.getRoot( ).getAbsolutePath( ) + "/Demo" );
		}
		catch ( ZipException e )
		{
			e.printStackTrace( );
		}
	}

	@Test
	public void testRunSetParseNames()
	{

		List<String> runSetConfigs = helper.getRunSetConfigs( );
		List<String> expectedRunSetConfigs = new ArrayList<String>( );
		expectedRunSetConfigs.add( "CombinedSearchOnENV" );
		expectedRunSetConfigs.add( "ExecuteOnDifferentEnvs" );
		expectedRunSetConfigs.add( "VariableOverrideRunset" );
		Assert.assertNotNull( "Expected to see parsed runset configs , got null" , runSetConfigs );
		Assert.assertArrayEquals( "Checking if runsets  equals" , expectedRunSetConfigs.toArray( ) , runSetConfigs.toArray( ) );

	}

	@Test
	public void testEnvParseNames()
	{
		List<String> expectedEnvs = new ArrayList<String>( );
		expectedEnvs.add( "SystemTest" );
		expectedEnvs.add( "Production" );
		expectedEnvs.add( "TemplateEAAS" );
		expectedEnvs.add( "UAT" );
		List<String> envs = helper.getProjectEnvironments( );
		Assert.assertNotNull( envs );

		Assert.assertArrayEquals( "Checking if envs are equal " , expectedEnvs.toArray( ) , envs.toArray( ) );
	}

	@After
	public void afterTest()
	{
		helper = null;
	}
}

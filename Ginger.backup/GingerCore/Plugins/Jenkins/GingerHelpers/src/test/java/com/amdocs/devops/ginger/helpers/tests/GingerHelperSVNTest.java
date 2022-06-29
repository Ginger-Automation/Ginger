//# Status=Cleaned; Comment=Cleaned on 05/15/18
package com.amdocs.devops.ginger.helpers.tests;

import org.junit.Assert;
import org.junit.Test;
import com.amdocs.devops.ginger.SourceControl.*;

public class GingerHelperSVNTest
{
	SvnHelper helper;
	@Test
	public void testSVnConnectionBadPassword()
	{
		helper = new SvnHelper( "http://stanislm01v:60123/svn/DevOps/Ginger_solutions" , "stas" , "stas1" ,"", "" );
		Assert.assertFalse( helper.isConnectionValid( ) );
	}
	
	@Test
	public void testSVnConnectionBadUsername()
	{
		helper = new SvnHelper( "http://stanislm01v:60123/svn/DevOps/Ginger_solutions" , "stas1" , "stas" ,"", "" );
		Assert.assertFalse( helper.isConnectionValid( ) );
	}
	
	@Test
	public void testSVnConnectionCatchWrongURL()
	{
		helper = new SvnHelper( "http://stanislm01v:60123/svn/DevOps/Ginger_solutions1" , "stas" , "stas","", ""  );
		Assert.assertFalse( helper.isConnectionValid( ) );
	}
	
	@Test
	public void testSVnConnectionCatchNotAllowed()
	{
		helper = new SvnHelper( "http://cmitechint1srv.corp.amdocs.com:81/svn/" , "StanislavMiasnikov" , "Ginger1234" ,"", "" );
		Assert.assertTrue( helper.isConnectionValid( ) );
	}
}

/*
 * Created by SharpDevelop.
 * User: Alan
 * Date: 6/8/2014
 * Time: 9:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.IO;

namespace GingerSikuliStandard.sikuli_REST
{
	/// <summary>
	/// Description of Pattern.
	/// </summary>
	public class Pattern
	{
		/// <summary>
		/// The path to the image to perform actions on.
		/// </summary>
		public String ImagePath {get; set;}
		/// <summary>
		/// The target offset of the image, going from the center as 0,0, which is the default.
		/// </summary>
		public Point Offset {get; set;}
		/// <summary>
		/// The percentage similarity that the tool looks for when searching for the pattern. 0.7 is default (70%)
		/// </summary>
		public Double Similar {get; set;}
		
		public Pattern() : this("", new Point(0,0), 0.7) {}
		public Pattern(String imagePath) : this(imagePath, new Point(0,0), 0.7) {}
		public Pattern(String imagePath, Point offset) : this(imagePath, offset, 0.7) {}
		public Pattern(String imagePath, Double similar) : this(imagePath, new Point(0,0), similar) {}
		/// <summary>
		/// Instantiates a new instance of the Pattern object, to be used by the tool to find the specified image on the screen.
		/// </summary>
		/// <param name="imagePath">The path to the image used in this pattern object; usually will be in .png format</param>
		/// <param name="offset">The target offset of the image, going from the center as 0,0, which is the default</param>
		/// <param name="similar">The percentage similarity that the tool looks for when searching for the pattern.  0.7 is the default (70%)</param>
		public Pattern(String imagePath, Point offset, Double similar)
		{
			ImagePath = imagePath;
			Offset = offset;
			Similar = similar;
		}
		/// <summary>
		/// Method to get the json_Pattern object that correlates with this Pattern object.  To be used by the Sikuli4Net tool in passing information to the service.  
		/// </summary>
		/// <returns></returns>
		public sikuli_JSON.json_Pattern ToJsonPattern()
		{
			sikuli_JSON.json_Pattern jPattern = new GingerSikuliStandard.sikuli_JSON.json_Pattern();
			jPattern.imagePath = this.ImagePath;
			jPattern.offset_x = this.Offset.X;
			jPattern.offset_y = this.Offset.Y;
			jPattern.similar = (float)this.Similar;
			return jPattern;
		}
	}
}

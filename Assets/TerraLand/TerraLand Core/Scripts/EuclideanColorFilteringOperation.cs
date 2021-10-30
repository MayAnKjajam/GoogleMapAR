using UnityEngine;
using AForge;
using AForge.Imaging;
using AForge.Imaging.ColorReduction;
using AForge.Imaging.ComplexFilters;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;
using AForge.Math.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

public class EuclideanColorFilteringOperation : BaseInPlacePartialFilter
{
	private short radius = 100;
	private RGB center = new RGB( 255, 255, 255 );
	private RGB output = new RGB( 255, 255, 255 );
	private RGB fill = new RGB( 0, 0, 0 );
	private bool fillOutside = true;
	
	// private format translation dictionary
	private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>( );
	
	/// <summary>
	/// Format translations dictionary.
	/// </summary>
	public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
	{
		get { return formatTranslations; }
	}
	
	/// <summary>
	/// RGB sphere's radius, [0, 450].
	/// </summary>
	/// 
	/// <remarks>Default value is 100.</remarks>
	/// 
	public short Radius
	{
		get { return radius; }
		set
		{
			radius = System.Math.Max( (short) 0, System.Math.Min( (short) 450, value ) );
		}
	}
	
	/// <summary>
	/// RGB sphere's center.
	/// </summary>
	/// 
	/// <remarks>Default value is (255, 255, 255) - white color.</remarks>
	/// 
	public RGB CenterColor
	{
		get { return center; }
		set { center = value; }
	}

	public RGB OutputColor
	{
		get { return output; }
		set { output = value; }
	}
	
	/// <summary>
	/// Fill color used to fill filtered pixels.
	/// </summary>
	public RGB FillColor
	{
		get { return fill; }
		set { fill = value; }
	}
	
	/// <summary>
	/// Determines, if pixels should be filled inside or outside specified
	/// RGB sphere.
	/// </summary>
	/// 
	/// <remarks><para>Default value is set to <see langword="true"/>, which means
	/// the filter removes colors outside of the specified range.</para></remarks>
	/// 
	public bool FillOutside
	{
		get { return fillOutside; }
		set { fillOutside = value; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EuclideanColorFilteringOperation"/> class.
	/// </summary>
	/// 
	public EuclideanColorFilteringOperation()
	{
		formatTranslations[PixelFormat.Format24bppRgb]  = PixelFormat.Format24bppRgb;
		formatTranslations[PixelFormat.Format32bppRgb]  = PixelFormat.Format32bppRgb;
		formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="EuclideanColorFilteringOperation"/> class.
	/// </summary>
	/// 
	/// <param name="center">RGB sphere's center.</param>
	/// <param name="radius">RGB sphere's radius.</param>
	/// 
	public EuclideanColorFilteringOperation( RGB center, short radius ) :
		this( )
	{
		this.center = center;
		this.radius = radius;
	}
	
	/// <summary>
	/// Process the filter on the specified image.
	/// </summary>
	/// 
	/// <param name="image">Source image data.</param>
	/// <param name="rect">Image rectangle for processing by the filter.</param>
	///
	protected override unsafe void ProcessFilter( UnmanagedImage image, Rectangle rect)
	{
		// get pixel size
		int pixelSize = ( image.PixelFormat == PixelFormat.Format24bppRgb ) ? 3 : 4;
		
		int startX  = rect.Left;
		int startY  = rect.Top;
		int stopX   = startX + rect.Width;
		int stopY   = startY + rect.Height;
		int offset  = image.Stride - rect.Width * pixelSize;
		int radius2 = radius * radius;
		
		int dr, dg, db;
		// sphere's center
		int cR = center.Red;
		int cG = center.Green;
		int cB = center.Blue;
		//int cA = center.Alpha;
		
		// fill color
		byte fR = output.Red;
		byte fG = output.Green;
		byte fB = output.Blue;
		//byte fA = output.Alpha;

		bool alphaDraw = false;

		if(fR <= 10 && fG <= 10 && fB <= 10)
			alphaDraw = true;

		// do the job
		byte* ptr = (byte*) image.ImageData.ToPointer( );
		
		// allign pointer to the first pixel to process
		ptr += ( startY * image.Stride + startX * pixelSize );
		
		// for each row
		for ( int y = startY; y < stopY; y++ )
		{
			// for each pixel
			for ( int x = startX; x < stopX; x++, ptr += pixelSize )
			{
				dr = cR - ptr[RGB.R];
				dg = cG - ptr[RGB.G];
				db = cB - ptr[RGB.B];
				
				// calculate the distance
				if ( dr * dr + dg * dg + db * db <= radius2 )
				{
					// inside sphere
					if ( !fillOutside )
					{
						ptr[RGB.R] = fR;
						ptr[RGB.G] = fG;
						ptr[RGB.B] = fB;

						if(alphaDraw)
							ptr[RGB.A] = 255;
						else
							ptr[RGB.A] = 0;
					}
				}
				else
				{
					// outside sphere
					if ( fillOutside )
					{
						ptr[RGB.R] = fR;
						ptr[RGB.G] = fG;
						ptr[RGB.B] = fB;

						if(alphaDraw)
							ptr[RGB.A] = 255;
						else
							ptr[RGB.A] = 0;
					}
				}
			}
			ptr += offset;
		}
	}
}


//	Form継承 2017/11/25 T.Umezawa

using System;
using System.Collections.Generic;

class MyForm : System.Windows.Forms.Form
{
	protected System.Timers.Timer		mTimer = new System.Timers.Timer();
	protected System.Drawing.SolidBrush	mSBWhite = new System.Drawing.SolidBrush( System.Drawing.Color.White );

	protected override void OnLoad( EventArgs e )
	{
		ClientSize = new System.Drawing.Size( 960, 720 );
		Left = 396;	//	キャプチャ都合上
		Top = 20;	//	キャプチャ都合上

		DoubleBuffered = true;
		BackColor = System.Drawing.Color.FromArgb( 0x55, 0x88, 0xff );

		mTimer.Elapsed += new System.Timers.ElapsedEventHandler( onMyTimer );
	}

	protected override void OnPaint( System.Windows.Forms.PaintEventArgs e )
	{
		base.OnPaint( e );

		System.Drawing.Graphics	g = e.Graphics;
		g.ScaleTransform( 4, 4 );
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

		onMyPaint( g );
	}

	protected virtual void onMyPaint( System.Drawing.Graphics g )
	{
	}

	protected virtual void onMyTimer( object sender, System.Timers.ElapsedEventArgs e )
	{
	}
}

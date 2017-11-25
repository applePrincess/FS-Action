//	#5 固定画面アクション FS Action 2017/11/25 T.Umezawa

using System;
using System.Collections.Generic;


class Util
{
	public static int GetAngle4i( double dx, double dy )
	{
		return( (int)( ( GetAngle360( dx, dy ) + 45 ) / 90 ) & 0x03 );
	}

	public static double GetAngle360( double dx, double dy )
	{
		double	r = Math.Atan2( dy, dx ) * 180 / Math.PI;
		if( r < 0 ){
			r += 360;
		}
		return( r );
	}
}


class Map
{
	static System.Drawing.Bitmap[]	sBM = {
		new System.Drawing.Bitmap( "block.png" ),
		new System.Drawing.Bitmap( "flag.png" ),
	};

	public static byte[,]		sMap = new byte[ 23, 30 ];


	public static void create()
	{
		for( int x = 0; x < sMap.GetLength( 1 ); x++ ){
			sMap[ sMap.GetLength( 0 ) - 1, x ] = 1;
			sMap[ sMap.GetLength( 0 ) - 2, x ] = 1;
		}

		byte	v = 1;
		int		n = 1;
		for( int y = 3; y <= 18; y += 3 ){
			for( int x = 0; x <sMap.GetLength( 1 ); x++, n-- ){
				if( n == 0 ){
					v = (byte)( 1 - v );
					n = FSAct.sRnd.Next( 2 ) + v + 1;
				}
				sMap[ y, x ] = v;
			}
		}

		sMap[ 2, 28 ] = 2;
	}

	public static void draw( System.Drawing.Graphics g )
	{
		for( int y = 0; y < sMap.GetLength( 0 ); y++ ){
			for( int x = 0; x < sMap.GetLength( 1 ); x++ ){
				if( sMap[ y, x ] != 0 ){
					g.DrawImage( sBM[ sMap[ y, x ] - 1 ], x * 8, y * 8 );
				}
			}
		}
	}

	public static bool IsArea( int x, int y )
	{
		return( x >= 0 && x < sMap.GetLength( 1 ) &&
		        y >= 0 && y < sMap.GetLength( 0 ) );
	}

	public static bool IsBlock( float x, float y )
	{
		x -= 0.5f;
		y -= 0.5f;

		int		x0 = (int)Math.Floor( x );
		int		y0 = (int)Math.Floor( y );
		int		x1 = (int)Math.Ceiling( x );
		int		y1 = (int)Math.Ceiling( y );
		return( IsBlock( x0, y0 ) ||
		        IsBlock( x1, y0 ) ||
		        IsBlock( x0, y1 ) ||
		        IsBlock( x1, y1 ) );
	}

	public static bool IsBlock( int x, int y )
	{
		return( !IsArea( x, y ) || sMap[ y, x ] == 1 );
	}
}


class Unit
{
	public static readonly int		DOT = 8;
	public static readonly int		DTH = DOT / 2;

	protected float	mX, mY;
	protected float	mDX, mDY;
	protected int	mJump;

	public Unit( float x, float y, float dx, float dy )
	{
		mX = x;
		mY = y;
		mDX = dx;
		mDY = dy;
	}

	public int getAngle4i( Unit u )
	{
		return( Util.GetAngle4i( u.mX - mX, u.mY - mY ) );
	}

	public bool isBlock()
	{
		return( Map.IsBlock( mX, mY ) );
	}

	public void jump( float dy )
	{
		if( mJump == 0 ){
			mJump = 1;
		}
		mDY = dy;
	}

	public virtual void step()
	{
		if( mJump > 0 ){
			mJump++;
		}

		mX += mDX;

		if( isBlock() ){
			mDX = -mDX;
			mX += mDX;
		}

		mY += mDY;

		if( mJump > 0 && mDY < 6.0f / DOT ){	//	重力
			mDY += 1.0f / 32;
		}

		if( mDY < 0 && isBlock() ){			//	上昇中
			if( !Map.IsBlock( mX + 1.0f / 16, mY ) ){
				mX += 1.0f / 16;
			}else if( !Map.IsBlock( mX - 1.0f / 16, mY ) ){
				mX -= 1.0f / 16;
			}else{							//	天井にぶつかる
				mDY = 0;
				mY = (int)( mY - 0.5f ) + 1.5f;
			}
		}

		if( mDY > 0 && isBlock() ){			//	下降中
			mDY = 0;						//	着地
			mY = (int)( mY - 0.5f ) + 0.5f;
			mJump = 0;
		}

		if( mJump == 0 && !Map.IsBlock( mX, mY + 1.0f / DOT ) ){	//	地面から落ちる
			mJump = 1;
			mY += 1.0f / DOT;
			mDY = 1.0f / 16;
		}
	}

	public bool isCollision( Unit u )
	{
		return( Math.Abs( mX - u.mX ) < 6.0f / DOT && Math.Abs( mY - u.mY ) < 6.0f / DOT );
	}
}


class Player : Unit
{
	public static readonly int		JUMP = 8;
	static System.Drawing.Bitmap[]	sBM = {
		new System.Drawing.Bitmap( "player.png" ),
		new System.Drawing.Bitmap( "player2.png" )
	};
	static System.Drawing.Rectangle	sRect = new System.Drawing.Rectangle( 0, 0, 8, 8 );

	public int		mType;
	public int		mBtn;

	public Player( int type ) : base( 0.5f, 20.5f, 1.0f / 16, 0 )
	{
		mType = type;
	}

	public void draw( System.Drawing.Graphics g )
	{
		sRect.X = ( (int)( mX * 8 ) & 1 ) * DOT;
		sRect.Y = Math.Sign( mDX ) * DTH + DTH;
		if( mJump != 0 ){
			sRect.X = 0;
		}
		g.DrawImage( sBM[ mType ], mX * DOT - DTH, mY * DOT - DTH, sRect, System.Drawing.GraphicsUnit.Pixel );
	}

	public void jump()
	{
		if( mJump < JUMP ){			//	接地中又は離陸時の場合
			jump( -5.0f / 16 );
		}
	}

	public override void step()
	{
		if( mBtn > 0 ){
			jump();
		}

		base.step();

		if( mX <= 0.5f || mX >= Map.sMap.GetLength( 1 ) - 0.5f ){
			mDX = Math.Sign( Map.sMap.GetLength( 1 ) / 2 - mX ) / 16.0f;
		}

		if( Map.sMap[ (int)mY, (int)mX ] == 2 ){
			FSAct.sGameClear = true;
		}
	}
}


class Enemy : Unit
{
	static System.Drawing.Bitmap[]	sBM = {
		new System.Drawing.Bitmap( "monster.png" ),
		new System.Drawing.Bitmap( "monster2.png" ),
	};

	int		mType;

	public Enemy( int type ) : base( FSAct.sRnd.Next( Map.sMap.GetLength( 1 ) - 2 ) + 1, 0.5f, FSAct.sRnd.Next( 2 ) / 16.0f - 1.0f / 32, 0 )
	{
		mType = type;
	}

	public void draw( System.Drawing.Graphics g )
	{
		if( mDX < 0 ){
			g.DrawImage( sBM[ mType ], mX * DOT - DTH, mY * DOT - DTH );
		}else{
			g.DrawImage( sBM[ mType ], mX * DOT + DTH, mY * DOT - DTH, -DOT, DOT );
		}
	}

	public void step( List<Enemy> le )
	{
		step();

		if( mX <= 0.5f || mX >= Map.sMap.GetLength( 1 ) - 0.5f ){
			mDX = Math.Sign( Map.sMap.GetLength( 1 ) / 2 - mX ) / 32.0f;
		}

		foreach( Enemy en in le ){
			if( en == this || !isCollision( en ) ){
				continue;
			}
			mDX = Math.Sign( mX - en.mX ) / 32.0f;
			if( mDX == 0 ){
				mDX = 1.0f / 32;
			}
			en.mDX = -mDX;
		}
	}
}


class FSAct : MyForm
{
	public static Random		sRnd = new Random();

	System.Drawing.Font			mFont = new System.Drawing.Font( "MS Gothic", 5 );
	int							mCount;
	List<Player>				mLPlayer = new List<Player>();
	List<Enemy>					mLEnemy;
	public static bool			sGameClear, sGameOver;
	int							mStage = 1;
	int							mScene;

	protected override void OnLoad( EventArgs e )
	{
		base.OnLoad( e );
		mTimer.Interval = 33;
		mTimer.Start();
	}

	protected override void OnKeyDown( System.Windows.Forms.KeyEventArgs e )
	{
		input( 1, e.KeyCode == System.Windows.Forms.Keys.R );
		base.OnKeyDown( e );
	}

	protected override void OnKeyUp( System.Windows.Forms.KeyEventArgs e )
	{
		release( 1 );
		base.OnKeyDown( e );
	}

	protected override void OnMouseDown( System.Windows.Forms.MouseEventArgs e )
	{
		input( 0, e.Button == System.Windows.Forms.MouseButtons.Right );
		base.OnMouseDown( e );
	}

	protected override void OnMouseUp( System.Windows.Forms.MouseEventArgs e )
	{
		release( 0 );
		base.OnMouseDown( e );
	}

	protected override void onMyPaint( System.Drawing.Graphics g )
	{
		if( mScene == 0 ){
			g.DrawString( "ジャンプアクション３ Jump Action3", mFont, mSBWhite, 60, 30 );
			g.DrawString( "PRESS ANY KEY", mFont, mSBWhite, 90, 90 );
			return;
		}

		Map.draw( g );
		foreach( Player pl in mLPlayer ){
			pl.draw( g );
		}
		foreach( Enemy en in mLEnemy ){
			en.draw( g );
		}

		g.DrawString( "TIME " + mCount, mFont, mSBWhite, 0, 0 );
		g.DrawString( "STAGE " + mStage, mFont, mSBWhite, 40, 0 );

		if( sGameClear ){
			g.DrawString( "STAGE CLEAR!", mFont, mSBWhite, 90, 90 );
		}

		if( sGameOver ){
			g.DrawString( "GAME OVER", mFont, mSBWhite, 90, 90 );
		}
	}

	protected override void onMyTimer( object sender, System.Timers.ElapsedEventArgs e )
	{
		if( sGameClear || sGameOver ){
			return;
		}

		mCount++;

		foreach( Player pl in mLPlayer ){
			pl.step();
			for( int i = mLEnemy.Count - 1; i >= 0; i-- ){
				if( pl.isCollision( mLEnemy[ i ] ) ){
					if( pl.getAngle4i( mLEnemy[ i ] ) == 1 ){
						mLEnemy.RemoveAt( i );
						pl.jump( -4.0f / 16 );
					}else{
						sGameOver = true;
					}
				}
			}
		}

		for( int i = mLEnemy.Count - 1; i >= 0; i-- ){
			Enemy	en = mLEnemy[ i ];
			en.step( mLEnemy );
		}

		Invalidate();
	}

	void input( int type, bool res )
	{
		if( mScene == 0 ){
			mStage = 1;
			start();
			mLPlayer.Add( new Player( type ) );
		}else if( sGameClear ){
			mStage++;
			start();
			mLPlayer.Add( new Player( type ) );
		}else if( res ){
			mStage = 1;
			start();
			mLPlayer.Add( new Player( type ) );
		}else if( mLPlayer[ 0 ].mType != type ){
			if( mLPlayer.Count == 1 ){
				mLPlayer.Add( new Player( type ) );
			}else{
				mLPlayer[ 1 ].mBtn = 1;
			}
		}else{
			mLPlayer[ 0 ].mBtn = 1;
		}
	}

	void release( int type )
	{
		foreach( Player p in mLPlayer ){
			if( p.mType == type ){
				p.mBtn = 0;
			}
		}
	}

	void start()
	{
		mScene = 1;
		sGameClear = false;
		sGameOver = false;
		mCount = 0;
		mLPlayer.Clear();
		mLEnemy = new List<Enemy>();
		for( int i = 0; i < mStage; i++ ){
			mLEnemy.Add( new Enemy( 0 ) );
			mLEnemy.Add( new Enemy( 1 ) );
		}

		Map.create();
	}

	[STAThread]
	static void Main()
	{
		System.Windows.Forms.Application.Run( new FSAct() );
	}
}

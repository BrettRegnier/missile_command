﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace missile_command
{
	public partial class Window : Form
	{
		// TODO make wave game mode

		private Random rand = new Random();

		private int fps = 0, frames = 0;
		private long tickCount = Environment.TickCount;
		private long elapsedTime = Environment.TickCount;

		private long score; // TODO move into score handler/UI stuff

		List<Entity> lEntity;
		List<Player> lPlayer;

		List<List<Entity>> lEntities;

		State ms;

		// Methods
		public Window()
		{
			this.InitializeComponent();
			Main();
		}

		private void Main()
		{
			// TODO make menu
			// TODO make pause menu
			InitForm();

			// TODO switch to using a component list.
			lEntities = new List<List<Entity>>();
			lPlayer = new List<Player>();

			//InitGame();
			//InitPlayers(1);
			ms = new MenuState(this);
			this.Invalidate(); // Start the game.
		}
		private void InitForm()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);

			// Maximize and hide everything else
			StartPosition = FormStartPosition.Manual;
			FormBorderStyle = FormBorderStyle.None;

			ClientSize = Utils.gameBounds;
		}
		private void InitGame()
		{
			//lEntities = new List<List<Entity>>();
			// 5 Types of entites based on accounts enum
			for (int i = 0; i < 5; i++)
				lEntities.Add(new List<Entity>());

			// Create landmasses
			Point baseLand = new Point(0, Utils.gameBounds.Height);
			Size baseSize = new Size(Utils.gameBounds.Width, Utils.LAND_MASS_HEIGHT);
			LandMass lm = new LandMass(baseLand.X, baseLand.Y, baseSize.Width, baseSize.Height);
			lEntities[(int)ETag.SYSTEM].Add(lm);

			for (int i = 0; i < 3; i++)
			{
				Point p = new Point(Utils.HILL_POSITIONS_X[i], lm.Body.Top);
				lEntities[(int)ETag.SYSTEM].Add(new LandMass(p.X, p.Y, Utils.HILL_MASS_WIDTH, Utils.HILL_MASS_HEIGHT));
			}

			for (int i = 0; i < 6; i++)
			{
				City c = new City();
				lEntities[(int)ETag.SYSTEM].Add(c);
			}
		}
		private void InitPlayers(int numPlayers)
		{
			for (int i = 0; i < numPlayers; i++)
			{
				Point ori = new Point((Utils.gameBounds.Width / 3) * (i + 1), 200);
				PType pt = PType.PLAYER;
				ETag ap = ETag.P1;
				if (i == 1)
					ap = ETag.P2;
				else if (i == 2)
					ap = ETag.P3;

				Player p = new Player(pt, ap);
				lPlayer.Add(p);
			}

			Player currentPlayer = lPlayer[0];
			for (int i = 0; i < 3; i++)
			{
				if (numPlayers == 2)
				{
					if (i == 1)
						currentPlayer = lPlayer[i];
				}
				else if (numPlayers == 3)
					currentPlayer = lPlayer[i];

				Size size = new Size(Config.Instance().TurretRadius(), Config.Instance().TurretRadius());
				Turret t = EntityFactory.MakeTurret(lEntities[(int)ETag.SYSTEM][i + 1].Body.TopCenter, size, PType.PLAYER, currentPlayer.GetTag());
				t.TurretShoot += P_TurretShoot;
				lEntities[(int)currentPlayer.GetTag()].Add(t);
				currentPlayer.AttachTurret(t);
			}

			KeypressHandler.Instance().Initialize(lPlayer);
		}
		private void Loop()
		{
			// TODO break when paused/exit
			while (true)
			{
				if (Environment.TickCount >= elapsedTime + 1000)
				{
					fps = frames;
					frames = 0;
					elapsedTime = Environment.TickCount;
					Console.Out.WriteLine("Frames: " + fps);
				}

				if (Environment.TickCount >= tickCount + 15)
				{
					tickCount = Environment.TickCount;
					this.Invalidate();
					break;
				}
			}
		}
		private void UpdateGame(object sender, PaintEventArgs e)
		{
			try
			{
				ms.Draw(e.Graphics);

				
				// TODO uncomment
				//SpawnEnemies();

				for (int i = 0; i < lPlayer.Count; i++)
					lPlayer[i].Draw(e.Graphics);

				for (int i = 0; i < lEntities.Count; i++)
					for (int j = 0; j < lEntities[i].Count; j++)
						lEntities[i][j].Draw(e.Graphics);

				// :O almost n^3
				for (int i = 0; i < lEntities.Count - 1; i++)
				{
					for (int j = 0; j < lEntities[(int)ETag.ENEMY].Count; j++)
					{
						for (int k = 0; k < lEntities[i].Count; k++)
						{
							lEntities[(int)ETag.ENEMY][j].Collider.CollisionDetection(lEntities[i][k].Collider);
						}
					}
				}

				frames++;
				//KeypressHandler.Instance().MoveCursor();

				// Update after drawing
				Loop();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				this.Invalidate();
			}
		}
		private void P_TurretShoot(Point origin, Point destination, ETag a)
		{
			// use player upgrades, if I add them, to determine the size.
			Size size = Config.Instance().DefaultBombSize();
			Bomb bmb = EntityFactory.MakeBomb(origin, size, destination, PType.PLAYER, a);
			bmb.DestroyBomb += DestroyGameObject;
			lEntities[(int)bmb.Tag].Add(bmb);
		}
		private void SpawnEnemies()
		{
			// TODO complex calculation based on ticks to determine when the next spawn is.
			if (Environment.TickCount >= elapsedTime + 1000)
			{
				Point spawnPoint = new Point(rand.Next(0, Utils.gameBounds.Width), 0);
				// TODO make a list of guaranteed points
				Point destination = new Point(rand.Next(0, Utils.gameBounds.Width), Utils.gameBounds.Height);
				Size size = Config.Instance().DefaultBombSize();
				Bomb bmb = EntityFactory.MakeBomb(spawnPoint, size, destination, PType.ENEMY, missile_command.ETag.ENEMY);
				bmb.DestroyBomb += DestroyGameObject;
				lEntities[(int)bmb.Tag].Add(bmb);
			}
		}
		private void SpawnTest()
		{
			Point spawnPoint = new Point(rand.Next(0, Utils.gameBounds.Width), 0);
			//Point destination = new Point(Utils.gameBounds.Width / 2, Utils.gameBounds.Height);
			Point destination = new Point(spawnPoint.X, Utils.gameBounds.Height);
			Size size = Config.Instance().DefaultBombSize();
			Bomb bmb = EntityFactory.MakeBomb(spawnPoint, size, destination, PType.ENEMY, missile_command.ETag.ENEMY);
			bmb.DestroyBomb += DestroyGameObject;
			lEntities[(int)bmb.Tag].Add(bmb);
		}
		private void MassTest()
		{
			for (int i = 0; i < 100; i++)
				SpawnTest();
		}
		private void GameForm_KeyDown(object sender, KeyEventArgs e)
		{
			KeypressHandler.Instance().KeyDown(e);
			if (e.KeyCode == Keys.H)
				SpawnTest();
			else if (e.KeyCode == Keys.J)
				MassTest();
			else if (e.KeyCode == Keys.Escape)
				this.Close();
		}
		private void GameForm_KeyUp(object sender, KeyEventArgs e)
		{
			KeypressHandler.Instance().KeyUp(e);
		}
		private void DestroyGameObject(Entity gameObject)
		{
			lEntities[(int)gameObject.Tag].Remove(gameObject);
		}
		private void AddScore(int score)
		{
			this.score += score;
		}
		private void GameOver()
		{
			// TODO show gameover on screen, and a menu to enter score, quit(close), restart game
		}
	}
}
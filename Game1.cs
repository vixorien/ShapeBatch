using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

using ShapeUtils;

namespace ShapeBatchDemo
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// Begin a shape batch
			ShapeBatch.Begin(GraphicsDevice);

			// Draw several example shapes
			DrawExampleShapes(gameTime);

			// End the shape batch
			ShapeBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Draws several example shapes to showcase ShapeBatch capabilities
		/// </summary>
		/// <param name="gt">Game time information</param>
		public void DrawExampleShapes(GameTime gt)
		{
			// Sample values for "animation"
			float rotation = (float)gt.TotalGameTime.TotalSeconds;
			float sinWave = MathF.Sin(rotation) + 2.0f;

			// Lines
			ShapeBatch.Line(new Vector2(50, 50), new Vector2(150, 70), Color.White);
			ShapeBatch.Line(new Vector2(50, 150), new Vector2(150, 220), 8.0f, Color.Pink, Color.Purple);
			ShapeBatch.Line(new Vector2(100, 250), 75.0f, rotation, Color.DarkKhaki);
			ShapeBatch.Line(new Vector2(100, 350), 75.0f, rotation + MathF.PI / 2.0f, 10.0f, Color.MediumPurple);

			// Boxes
			ShapeBatch.Box(200, 50, 50, 50, Color.IndianRed, Color.IndianRed, Color.BlueViolet, Color.BlueViolet);
			ShapeBatch.Box(new Rectangle(220, 150, 50, 60), Color.PapayaWhip);

			// Outline boxes
			ShapeBatch.BoxOutline(200, 250, 50, 50, Color.Gainsboro);
			ShapeBatch.BoxOutline(new Rectangle(220, 350, 75, 60), Color.LawnGreen, Color.DarkGreen, Color.MediumSeaGreen, Color.DarkGreen);

			// Circles
			ShapeBatch.Circle(new Vector2(350, 75), sinWave * 20.0f, Color.Black, Color.Aquamarine);
			ShapeBatch.Circle(new Vector2(350, 225), sinWave * 10.0f + 20.0f, 8, Color.AliceBlue);
			ShapeBatch.Circle(new Vector2(350, 375), sinWave * 5.0f + 20.0f, 5, -rotation, Color.LightGoldenrodYellow);

			// Outline circles
			ShapeBatch.CircleOutline(new Vector2(425, 125), 25.0f, Color.Aquamarine);
			ShapeBatch.CircleOutline(new Vector2(425, 275), 30.0f, 8, Color.AliceBlue);
			ShapeBatch.CircleOutline(new Vector2(425, 425), 35.0f, 5, rotation, Color.LightGoldenrodYellow);

			// Triangles
			ShapeBatch.Triangle(new Vector2(550, 75), 100, Color.Orange, Color.OrangeRed, Color.Yellow);
			ShapeBatch.Triangle(new Vector2(550, 250), 75, rotation, Color.Red, Color.ForestGreen, Color.Blue);
			ShapeBatch.Triangle(new Vector2(550, 475), new Vector2(475, 420), new Vector2(675, 350), Color.CornflowerBlue, Color.DarkBlue, Color.LightBlue);

			// Outline triangles
			ShapeBatch.TriangleOutline(new Vector2(650, 150), 60, Color.Orange);
			ShapeBatch.TriangleOutline(new Vector2(650, 275), 60, rotation / -2.0f, Color.Red, Color.ForestGreen, Color.Blue);
			ShapeBatch.TriangleOutline(new Vector2(650, 450), new Vector2(550, 360), new Vector2(750, 400), Color.CornflowerBlue);
		}
	}
}
// -----------------------------------------
// ShapeBatch
//
// Copyright (c) Chris Cascioli
// Licensed under the MIT License
//
// https://github.com/vixorien/shapebatch
// -----------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShapeUtils
{
	/// <summary>
	/// Allows simple shapes to be drawn in MonoGame.  These shapes
	/// are batched and sent to the GPU in groups for efficiency.  
	/// 
	/// Ideally, do NOT use ShapeBatch while a SpriteBatch is currently active.
	/// </summary>
	static class ShapeBatch
	{
		// Internal initialization and drawing fields
		private static bool initialized = false;
		private static bool batchActive = false;
		private static BasicEffect effect = null;
		private static GraphicsDevice device = null;

		// The batches of lines and polygons
		private static List<VertexPositionColor> lines = new List<VertexPositionColor>();
		private static List<VertexPositionColor> polygons = new List<VertexPositionColor>();

		// Depth values necessary for interleaving lines and polygons, 
		// even though they're tracked as two separate batches.
		private static float currentDepth = 1.0f;
		private const float DepthStep = 0.0000001f;

		// How many primitives can be batched before being drawn?
		private const int PrimitivesPerBatch = 1000;

		// The default length for the segments (lines or triangles) of a circle
		private const float DefaultCircleSegmentLength = 3.0f;

		/// <summary>
		/// Internal initialize function that is called any time
		/// a batch is started.  Only performs initialization 
		/// once per program.
		/// </summary>
		/// <param name="device">The GraphicsDevice used for drawing</param>
		private static void Initialize(GraphicsDevice device)
		{
			// Anything to do?
			if (initialized)
				return;

			// Save the device so we can draw whenever we need to
			ShapeBatch.device = device;

			// Create an effect for raw GPU drawing
			effect = new BasicEffect(device);
			effect.VertexColorEnabled = true;

			// Ready to go, batch isn't active yet
			initialized = true;
			batchActive = false;
		}


		/// <summary>
		/// Begins a batch.  Shapes can only be drawn during a batch.
		/// </summary>
		/// <param name="device">The GraphicsDevice used for drawing.  Most likely Game1's GraphicsDevice property.</param>
		public static void Begin(GraphicsDevice device)
		{
			if (batchActive)
				throw new InvalidOperationException("Cannot call Begin() twice before calling End()");

			// Initialize the draw helper if necessary
			Initialize(device);

			// We're actively batching
			batchActive = true;
			currentDepth = 1.0f;
		}


		/// <summary>
		/// Ends a batch and immediately draws any remaining shapes.
		/// </summary>
		public static void End()
		{
			if (!batchActive)
				throw new InvalidOperationException("Must call Begin() before End()");

			// Draw any remaining shapes
			FlushShapes();

			// The batch has ended
			batchActive = false;
		}


		/// <summary>
		/// Draws a 1-pixel-wide line
		/// </summary>
		/// <param name="startPos">The starting position</param>
		/// <param name="endPos">The ending position</param>
		/// <param name="color">The color of the line</param>
		public static void Line(Vector2 startPos, Vector2 endPos, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Use the width-checking helper to handle this line, then progress depth
			BatchWideLine(startPos, endPos, 1.0f, color, color);
			NextDepth();
		}


		/// <summary>
		/// Draws a 1-pixel-wide line
		/// </summary>
		/// <param name="startPos">The starting position</param>
		/// <param name="endPos">The ending position</param>
		/// <param name="startColor">The color of the line at the starting position</param>
		/// <param name="endColor">The color of the line at the ending position</param>
		public static void Line(Vector2 startPos, Vector2 endPos, Color startColor, Color endColor)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Use the width-checking helper to handle this line, then progress depth
			BatchWideLine(startPos, endPos, 1.0f, startColor, endColor);
			NextDepth();
		}


		/// <summary>
		/// Draws a line
		/// </summary>
		/// <param name="startPos">The starting position</param>
		/// <param name="endPos">The ending position</param>
		/// <param name="width">The width of the line (minimum 1 pixel)</param>
		/// <param name="color">The color of the line</param>
		public static void Line(Vector2 startPos, Vector2 endPos, float width, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Use the width-checking helper to handle this line, then progress depth
			BatchWideLine(startPos, endPos, width, color, color);
			NextDepth();
		}


		/// <summary>
		/// Draws a line
		/// </summary>
		/// <param name="startPos">The starting position</param>
		/// <param name="endPos">The ending position</param>
		/// <param name="width">The width of the line (minimum 1 pixel)</param>
		/// <param name="startColor">The color of the line at the starting position</param>
		/// <param name="endColor">The color of the line at the ending position</param>
		public static void Line(Vector2 startPos, Vector2 endPos, float width, Color startColor, Color endColor)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Use the width-checking helper to handle this line, then progress depth
			BatchWideLine(startPos, endPos, width, startColor, endColor);
			NextDepth();
		}


		/// <summary>
		/// Draws a 1-pixel-wide line.  This overload returns the end position of the line.
		/// </summary>
		/// <param name="startPos">The starting position of the line</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of the line, in radians, measured from the positive x axis</param>
		/// <param name="color">The color of the line</param>
		/// <returns>The end position of the line.</returns>
		public static Vector2 Line(Vector2 startPos, float length, float angle, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Use the overload that handles the width, assuming a width of 1.0
			return Line(startPos, length, angle, 1.0f, color);
		}


		/// <summary>
		/// Draws a 1-pixel-wide line.  This overload returns the end position of the line.
		/// </summary>
		/// <param name="startPos">The starting position of the line</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of the line, in radians, measured from the positive x axis</param>
		/// <param name="startColor">The color of the line at the starting position</param>
		/// <param name="endColor">The color of the line at the ending position</param>
		/// <returns>The end position of the line.</returns>
		public static Vector2 Line(Vector2 startPos, float length, float angle, Color startColor, Color endColor)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Use the overload that handles the width, assuming a width of 1.0
			return Line(startPos, length, angle, 1.0f, startColor, endColor);
		}


		/// <summary>
		/// Draws a line.  This overload returns the end position of the line.
		/// </summary>
		/// <param name="startPos">The starting position of the line</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of the line, in radians, measured from the positive x axis</param>
		/// <param name="width">The width of the line (minimum 1 pixel)</param>
		/// <param name="color">The color of the line</param>
		/// <returns>The end position of the line.</returns>
		public static Vector2 Line(Vector2 startPos, float length, float angle, float width, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Calculate the ending position as an offset from the starting position
			Vector2 endPos = startPos;
			endPos += new Vector2(MathF.Cos(-angle), MathF.Sin(-angle)) * length;

			// Use the width-checking helper to handle this line, then progress depth
			BatchWideLine(startPos, endPos, width, color, color);
			NextDepth();

			// Return the ending position in the event that's useful to the caller
			return endPos;
		}


		/// <summary>
		/// Draws a line.  This overload returns the end position of the line.
		/// </summary>
		/// <param name="startPos">The starting position of the line</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of the line, in radians, measured from the positive x axis</param>
		/// <param name="width">The width of the line (minimum 1 pixel)</param>
		/// <param name="startColor">The color of the line at the starting position</param>
		/// <param name="endColor">The color of the line at the ending position</param>
		/// <returns>The end position of the line.</returns>
		public static Vector2 Line(Vector2 startPos, float length, float angle, float width, Color startColor, Color endColor)
		{
			if (!batchActive)
				throw new InvalidOperationException("Line() must be called between Begin() and End()");

			// Calculate the ending position as an offset from the starting position
			Vector2 endPos = startPos;
			endPos += new Vector2(MathF.Cos(-angle), MathF.Sin(-angle)) * length;

			// Use the width-checking helper to handle this line, then progress depth
			BatchWideLine(startPos, endPos, width, startColor, endColor);
			NextDepth();

			// Return the ending position in the event that's useful to the caller
			return endPos;
		}


		/// <summary>
		/// Draws a solid (filled-in) box
		/// </summary>
		/// <param name="x">The x position of the top left corner of the box</param>
		/// <param name="y">The y position of the top left corner of the box</param>
		/// <param name="width">The width of the box</param>
		/// <param name="height">The height of the box</param>
		/// <param name="color">The color of the box</param>
		public static void Box(float x, float y, float width, float height, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Box() must be called between Begin() and End()");

			// Create the corners
			Vector2 topLeft = new Vector2(x, y);
			Vector2 topRight = new Vector2(x + width, y);
			Vector2 bottomRight = new Vector2(x + width, y + height);
			Vector2 bottomLeft = new Vector2(x, y + height);

			// Create the polygons
			BatchPolygon(topLeft, bottomRight, bottomLeft, color, color, color);
			BatchPolygon(topLeft, topRight, bottomRight, color, color, color);

			NextDepth();
		}


		/// <summary>
		/// Draws a solid (filled-in) box
		/// </summary>
		/// <param name="x">The x position of the top left corner of the box</param>
		/// <param name="y">The y position of the top left corner of the box</param>
		/// <param name="width">The width of the box</param>
		/// <param name="height">The height of the box</param>
		/// <param name="colorTopLeft">The color of the top left corner of the box</param>
		/// <param name="colorTopRight">The color of the top right corner of the box</param>
		/// <param name="colorBottomRight">The color of the bottom right corner of the box</param>
		/// <param name="colorBottomLeft">The color of the bottom left corner of the box</param>
		public static void Box(float x, float y, float width, float height, Color colorTopLeft, Color colorTopRight, Color colorBottomRight, Color colorBottomLeft)
		{
			if (!batchActive)
				throw new InvalidOperationException("Box() must be called between Begin() and End()");

			// Create the corners
			Vector2 topLeft = new Vector2(x, y);
			Vector2 topRight = new Vector2(x + width, y);
			Vector2 bottomRight = new Vector2(x + width, y + height);
			Vector2 bottomLeft = new Vector2(x, y + height);

			// Create the polygons
			BatchPolygon(topLeft, bottomRight, bottomLeft, colorTopLeft, colorBottomRight, colorBottomLeft);
			BatchPolygon(topLeft, topRight, bottomRight, colorTopLeft, colorTopRight, colorBottomRight);

			NextDepth();
		}


		/// <summary>
		/// Draws a solid (filled-in) box
		/// </summary>
		/// <param name="rect">The rectangle specifying the box's position and size</param>
		/// <param name="color">The color of the outline</param>
		public static void Box(Rectangle rect, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Box() must be called between Begin() and End()");

			Box(rect.X, rect.Y, rect.Width, rect.Height, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) box
		/// </summary>
		/// <param name="rect">The rectangle specifying the box's position and size</param>
		/// <param name="colorTopLeft">The color of the top left corner of the box</param>
		/// <param name="colorTopRight">The color of the top right corner of the box</param>
		/// <param name="colorBottomRight">The color of the bottom right corner of the box</param>
		/// <param name="colorBottomLeft">The color of the bottom left corner of the box</param>
		public static void Box(Rectangle rect, Color colorTopLeft, Color colorTopRight, Color colorBottomRight, Color colorBottomLeft)
		{
			if (!batchActive)
				throw new InvalidOperationException("Box() must be called between Begin() and End()");

			Box(rect.X, rect.Y, rect.Width, rect.Height, colorTopLeft, colorTopRight, colorBottomRight, colorBottomLeft);
		}


		/// <summary>
		/// Draws the outline of a box
		/// </summary>
		/// <param name="x">The x position of the top left corner of the box</param>
		/// <param name="y">The y position of the top left corner of the box</param>
		/// <param name="width">The width of the box</param>
		/// <param name="height">The height of the box</param>
		/// <param name="color">The color of the outline</param>
		public static void BoxOutline(float x, float y, float width, float height, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("BoxOutline() must be called between Begin() and End()");

			// Create the corners
			Vector2 topLeft = new Vector2(x, y);
			Vector2 topRight = new Vector2(x + width, y);
			Vector2 bottomRight = new Vector2(x + width, y + height);
			Vector2 bottomLeft = new Vector2(x, y + height + 1); // This corner always rasterizes incorrectly, so adjust by 1 pixel

			// Draw the four lines that make up the box
			BatchLine(topLeft, topRight, color, color);         // Top
			BatchLine(topRight, bottomRight, color, color);     // Right
			BatchLine(bottomRight, bottomLeft, color, color);   // Bottom
			BatchLine(bottomLeft, topLeft, color, color);       // Left

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws the outline of a box
		/// </summary>
		/// <param name="x">The x position of the top left corner of the box</param>
		/// <param name="y">The y position of the top left corner of the box</param>
		/// <param name="width">The width of the box</param>
		/// <param name="height">The height of the box</param>
		/// <param name="colorTopLeft">The color of the top left corner of the box</param>
		/// <param name="colorTopRight">The color of the top right corner of the box</param>
		/// <param name="colorBottomRight">The color of the bottom right corner of the box</param>
		/// <param name="colorBottomLeft">The color of the bottom left corner of the box</param>
		public static void BoxOutline(float x, float y, float width, float height, Color colorTopLeft, Color colorTopRight, Color colorBottomRight, Color colorBottomLeft)
		{
			if (!batchActive)
				throw new InvalidOperationException("BoxOutline() must be called between Begin() and End()");

			// Create the corners
			Vector2 topLeft = new Vector2(x, y);
			Vector2 topRight = new Vector2(x + width, y);
			Vector2 bottomRight = new Vector2(x + width, y + height);
			Vector2 bottomLeft = new Vector2(x, y + height + 1); // This corner always rasterizes incorrectly, so adjust by 1 pixel

			// Draw the four lines that make up the box
			BatchLine(topLeft, topRight, colorTopLeft, colorTopRight);				// Top
			BatchLine(topRight, bottomRight, colorTopRight, colorBottomRight);		// Right
			BatchLine(bottomRight, bottomLeft, colorBottomRight, colorBottomLeft);	// Bottom
			BatchLine(bottomLeft, topLeft, colorBottomLeft, colorTopLeft);			// Left

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws the outline of a box
		/// </summary>
		/// <param name="rect">The rectangle specifying the box's position and size</param>
		/// <param name="color">The color of the outline</param>
		public static void BoxOutline(Rectangle rect, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("BoxOutline() must be called between Begin() and End()");

			// Decompose the rectangle and call the other overload
			BoxOutline(rect.X, rect.Y, rect.Width, rect.Height, color);
		}


		/// <summary>
		/// Draws the outline of a box
		/// </summary>
		/// <param name="rect">The rectangle specifying the box's position and size</param>
		/// <param name="colorTopLeft">The color of the top left corner of the box</param>
		/// <param name="colorTopRight">The color of the top right corner of the box</param>
		/// <param name="colorBottomRight">The color of the bottom right corner of the box</param>
		/// <param name="colorBottomLeft">The color of the bottom left corner of the box</param>
		public static void BoxOutline(Rectangle rect, Color colorTopLeft, Color colorTopRight, Color colorBottomRight, Color colorBottomLeft)
		{
			if (!batchActive)
				throw new InvalidOperationException("BoxOutline() must be called between Begin() and End()");

			// Decompose the rectangle and call the other overload
			BoxOutline(rect.X, rect.Y, rect.Width, rect.Height, colorTopLeft, colorTopRight, colorBottomRight, colorBottomLeft);
		}


		/// <summary>
		/// Draws a solid (filled-in) circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="segments">The number of segments (triangles) that are used to fill in the circle.  Minimum of 3.</param>
		/// <param name="rotation">The rotation of the circle (this is much more obvious with fewer segments)</param>
		/// <param name="color">The color of the circle</param>
		public static void Circle(Vector2 center, float radius, int segments, float rotation, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Circle() must be called between Begin() and End()");

			// Call the overload that takes two colors
			Circle(center, radius, segments, rotation, color, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="segments">The number of segments (triangles) that are used to fill in the circle.  Minimum of 3.</param>
		/// <param name="rotation">The rotation of the circle (this is much more obvious with fewer segments)</param>
		/// <param name="colorCenter">The color of the center of the circle</param>
		/// <param name="colorEdge">The color of the edge of the circle</param>
		public static void Circle(Vector2 center, float radius, int segments, float rotation, Color colorCenter, Color colorEdge)
		{
			if (!batchActive)
				throw new InvalidOperationException("Circle() must be called between Begin() and End()");

			// Minimum of three segments
			segments = Math.Max(1, segments);

			// How far does each segment extend, in radians?
			float step = MathF.PI * 2.0f / segments;

			// Batch a triangle for each segment
			for (int i = 0; i < segments; i++)
			{
				// The angle of each side of the triangle
				float a0 = rotation + i * step;
				float a1 = rotation + (i + 1) * step;

				// The positions of the far vertices of the triangle
				Vector2 pos0 = center + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;
				Vector2 pos1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

				// Each triangle shares the center position of the circle
				BatchPolygon(center, pos0, pos1, colorCenter, colorEdge, colorEdge);
			}

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws a solid (filled-in) circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="segments">The number of segments (triangles) that are used to fill in the circle.  Minimum of 3.</param>
		/// <param name="color">The color of the circle</param>
		public static void Circle(Vector2 center, float radius, int segments, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Circle() must be called between Begin() and End()");

			// Call the overload that takes a rotation
			Circle(center, radius, segments, 0.0f, color, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="segments">The number of segments (triangles) that are used to fill in the circle.  Minimum of 3.</param>
		/// <param name="colorCenter">The color of the center of the circle</param>
		/// <param name="colorEdge">The color of the edge of the circle</param>
		public static void Circle(Vector2 center, float radius, int segments, Color colorCenter, Color colorEdge)
		{
			if (!batchActive)
				throw new InvalidOperationException("Circle() must be called between Begin() and End()");

			// Call the overload that takes a rotation
			Circle(center, radius, segments, 0.0f, colorCenter, colorEdge);
		}


		/// <summary>
		/// Draws a solid (filled-in) circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="rotation">The rotation of the circle (this is much more obvious with fewer segments)</param>
		/// <param name="color">The color of the circle</param>
		public static void Circle(Vector2 center, float radius, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Circle() must be called between Begin() and End()");

			// Call the overload that calculates the number of segments
			Circle(center, radius, color, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="rotation">The rotation of the circle (this is much more obvious with fewer segments)</param>
		/// <param name="colorCenter">The color of the center of the circle</param>
		/// <param name="colorEdge">The color of the edge of the circle</param>
		public static void Circle(Vector2 center, float radius, Color colorCenter, Color colorEdge)
		{
			if (!batchActive)
				throw new InvalidOperationException("Circle() must be called between Begin() and End()");

			// Calculate an appropriate number of segments based on radius
			float angle = MathF.Asin(DefaultCircleSegmentLength / radius);
			int segments = (int)(MathF.PI * 2.0f / angle);

			Circle(center, radius, segments, 0.0f, colorCenter, colorEdge);
		}


		/// <summary>
		/// Draws the outline of a circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="segments">The number of segments (lines) that make up the circle.  Minimum of 3.</param>
		/// <param name="rotation">The rotation of the circle (this is much more obvious with fewer segments)</param>
		/// <param name="color">The color of the circle</param>
		public static void CircleOutline(Vector2 center, float radius, int segments, float rotation, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("CircleOutline() must be called between Begin() and End()");

			// Minimum of three segments
			segments = Math.Max(1, segments);

			// How far does each segment extend, in radians?
			float step = MathF.PI * 2.0f / segments;

			// Batch a line for each segment
			for (int i = 0; i < segments; i++)
			{
				// The angle of each line endpoint
				float a0 = rotation + i * step;
				float a1 = rotation + (i + 1) * step;

				// The positions line endpoints
				Vector2 pos0 = center + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;
				Vector2 pos1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

				// Batch a single line
				BatchLine(pos0, pos1, color, color);
			}

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws the outline of a circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="segments">The number of segments (lines) that make up the circle.  Minimum of 3.</param>
		/// <param name="color">The color of the circle</param>
		public static void CircleOutline(Vector2 center, float radius, int segments, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("CircleOutline() must be called between Begin() and End()");

			CircleOutline(center, radius, segments, 0.0f, color);
		}


		/// <summary>
		/// Draws the outline of a circle
		/// </summary>
		/// <param name="center">The position of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="color">The color of the circle</param>
		public static void CircleOutline(Vector2 center, float radius, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("CircleOutline() must be called between Begin() and End()");

			// Calculate an appropriate number of segments based on radius
			float angle = MathF.Asin(DefaultCircleSegmentLength / radius);
			int segments = (int)(MathF.PI * 2.0f / angle);

			CircleOutline(center, radius, segments, 0.0f, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) triangle
		/// </summary>
		/// <param name="p0">The position of the first vertex</param>
		/// <param name="p1">The position of the second vertex</param>
		/// <param name="p2">The position of the third vertex</param>
		/// <param name="color">The color of the triangle</param>
		public static void Triangle(Vector2 p0, Vector2 p1, Vector2 p2, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Triangle() must be called between Begin() and End()");

			// Call the overload that takes 3 colors
			Triangle(p0, p1, p2, color, color, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) triangle
		/// </summary>
		/// <param name="p0">The position of the first vertex</param>
		/// <param name="p1">The position of the second vertex</param>
		/// <param name="p2">The position of the third vertex</param>
		/// <param name="color0">The color of the first vertex</param>
		/// <param name="color1">The color of the second vertex</param>
		/// <param name="color2">The color of the third vertex</param>
		public static void Triangle(Vector2 p0, Vector2 p1, Vector2 p2, Color color0, Color color1, Color color2)
		{
			if (!batchActive)
				throw new InvalidOperationException("Triangle() must be called between Begin() and End()");

			// Need to make sure we're in clockwise winding order!

			// Calculate vectors running down the edges of the triangle
			Vector3 edge0 = new Vector3(p1 - p0, 0);
			Vector3 edge1 = new Vector3(p2 - p0, 0);

			// The cross product of those edges is a vector perpendicular
			// to the other two
			Vector3 cross = Vector3.Cross(edge0, edge1);

			// Check the Z value of the cross product to determine
			// if we need to swap the winding order of the vertices
			if (cross.Z >= 0)
			{
				BatchPolygon(p0, p1, p2, color0, color1, color2); // 0, 1, 2
			}
			else
			{
				BatchPolygon(p0, p2, p1, color0, color2, color1); // 0, 2, 1
			}

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws a solid (filled-in) equilateral triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="rotation">The rotation of the triangle</param>
		/// <param name="color">The color of the triangle</param>
		public static void Triangle(Vector2 center, float height, float rotation, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Triangle() must be called between Begin() and End()");

			// Call the overload that takes 3 colors
			Triangle(center, height, rotation, color, color, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) equilateral triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="rotation">The rotation of the triangle</param>
		/// <param name="color0">The color of the first vertex</param>
		/// <param name="color1">The color of the second vertex</param>
		/// <param name="color2">The color of the third vertex</param>
		public static void Triangle(Vector2 center, float height, float rotation, Color color0, Color color1, Color color2)
		{
			if (!batchActive)
				throw new InvalidOperationException("Triangle() must be called between Begin() and End()");

			// Pre-calculations to speed things up
			float rad30 = MathF.PI / 6.0f;  // 30 degrees in radians
			float tan30 = MathF.Tan(rad30); // Tangent of 30 degrees

			// Calculate various lengths from the height
			float halfBase = tan30 * height;
			float centerToBase = tan30 * halfBase;
			float centerToTop = height - centerToBase;

			// Angles to vertices, including custom rotation
			float topAngle = -MathF.PI / 2 + rotation;
			float brAngle = rad30 + rotation;
			float blAngle = rad30 * 5 + rotation;

			// Offsets to vertices from center
			Vector2 topOffset = new Vector2(MathF.Cos(topAngle), MathF.Sin(topAngle)) * centerToTop;
			Vector2 brOffset = new Vector2(MathF.Cos(brAngle), MathF.Sin(brAngle)) * centerToTop;
			Vector2 blOffset = new Vector2(MathF.Cos(blAngle), MathF.Sin(blAngle)) * centerToTop;

			// Actual pixel vertex locations
			Vector2 v0 = center + topOffset;
			Vector2 v1 = center + brOffset;
			Vector2 v2 = center + blOffset;

			// Draw the triangle
			Triangle(v0, v1, v2, color0, color1, color2);
		}


		/// <summary>
		/// Draws a solid (filled-in) equilateral triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="color">The color of the triangle</param>
		public static void Triangle(Vector2 center, float height, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("Triangle() must be called between Begin() and End()");

			// Draw the triangle using a default rotation
			Triangle(center, height, 0.0f, color, color, color);
		}


		/// <summary>
		/// Draws a solid (filled-in) equilateral triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="color">The color of the triangle</param>
		public static void Triangle(Vector2 center, float height, Color color0, Color color1, Color color2)
		{
			if (!batchActive)
				throw new InvalidOperationException("Triangle() must be called between Begin() and End()");

			// Draw the triangle using a default rotation
			Triangle(center, height, 0.0f, color0, color1, color2);
		}


		/// <summary>
		/// Draws the outline of a triangle
		/// </summary>
		/// <param name="p0">The position of the first vertex</param>
		/// <param name="p1">The position of the second vertex</param>
		/// <param name="p2">The position of the third vertex</param>
		/// <param name="color">The color of the triangle</param>
		public static void TriangleOutline(Vector2 p0, Vector2 p1, Vector2 p2, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("TriangleOutline() must be called between Begin() and End()");

			// Call the overload that takes 3 colors
			TriangleOutline(p0, p1, p2, color, color, color);
		}


		/// <summary>
		/// Draws the outline of a triangle
		/// </summary>
		/// <param name="p0">The position of the first vertex</param>
		/// <param name="p1">The position of the second vertex</param>
		/// <param name="p2">The position of the third vertex</param>
		/// <param name="color0">The color of the first vertex</param>
		/// <param name="color1">The color of the second vertex</param>
		/// <param name="color2">The color of the third vertex</param>
		public static void TriangleOutline(Vector2 p0, Vector2 p1, Vector2 p2, Color color0, Color color1, Color color2)
		{
			if (!batchActive)
				throw new InvalidOperationException("TriangleOutline() must be called between Begin() and End()");

			// Batch lines between the vertices
			BatchLine(p0, p1, color0, color1);
			BatchLine(p1, p2, color1, color2);
			BatchLine(p2, p0, color2, color0);

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws the outline of a triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="rotation">The rotation of the triangle</param>
		/// <param name="color">The color of the triangle</param>
		public static void TriangleOutline(Vector2 center, float height, float rotation, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("TriangleOutline() must be called between Begin() and End()");

			// Call the overload that takes 3 colors
			TriangleOutline(center, height, rotation, color, color, color);
		}


		/// <summary>
		/// Draws the outline of a triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="rotation">The rotation of the triangle</param>
		/// <param name="color0">The color of the first vertex</param>
		/// <param name="color1">The color of the second vertex</param>
		/// <param name="color2">The color of the third vertex</param>
		public static void TriangleOutline(Vector2 center, float height, float rotation, Color color0, Color color1, Color color2)
		{
			if (!batchActive)
				throw new InvalidOperationException("TriangleOutline() must be called between Begin() and End()");

			// Pre-calculations to speed things up
			float rad30 = MathF.PI / 6.0f;  // 30 degrees in radians
			float tan30 = MathF.Tan(rad30); // Tangent of 30 degrees

			// Calculate various lengths from the height
			float halfBase = tan30 * height;
			float centerToBase = tan30 * halfBase;
			float centerToTop = height - centerToBase;

			// Angles to vertices, including custom rotation
			float topAngle = -MathF.PI / 2 + rotation;
			float brAngle = rad30 + rotation;
			float blAngle = rad30 * 5 + rotation;

			// Offsets to vertices from center
			Vector2 topOffset = new Vector2(MathF.Cos(topAngle), MathF.Sin(topAngle)) * centerToTop;
			Vector2 brOffset = new Vector2(MathF.Cos(brAngle), MathF.Sin(brAngle)) * centerToTop;
			Vector2 blOffset = new Vector2(MathF.Cos(blAngle), MathF.Sin(blAngle)) * centerToTop;

			// Actual pixel vertex locations
			Vector2 v0 = center + topOffset;
			Vector2 v1 = center + brOffset;
			Vector2 v2 = center + blOffset;

			// Batch lines between the vertices
			BatchLine(v0, v1, color0, color1);
			BatchLine(v1, v2, color1, color2);
			BatchLine(v2, v0, color2, color0);

			// Progress to the next depth
			NextDepth();
		}


		/// <summary>
		/// Draws the outline of a triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="color">The color of the triangle</param>
		public static void TriangleOutline(Vector2 center, float height, Color color)
		{
			if (!batchActive)
				throw new InvalidOperationException("TriangleOutline() must be called between Begin() and End()");

			// Draw the triangle using a default rotation
			TriangleOutline(center, height, 0.0f, color, color, color);
		}


		/// <summary>
		/// Draws the outline of a triangle
		/// </summary>
		/// <param name="center">The center of the triangle (equidistant from all three vertices)</param>
		/// <param name="height">The height of the triangle as measued from the top vertex to the base</param>
		/// <param name="color0">The color of the first vertex</param>
		/// <param name="color1">The color of the second vertex</param>
		/// <param name="color2">The color of the third vertex</param>
		public static void TriangleOutline(Vector2 center, float height, Color color0, Color color1, Color color2)
		{
			if (!batchActive)
				throw new InvalidOperationException("TriangleOutline() must be called between Begin() and End()");

			// Draw the triangle using a default rotation
			TriangleOutline(center, height, 0.0f, color0, color1, color2);
		}


		/// <summary>
		/// Adds a single line to the batch.  Potentially causes a Flush() if there are 
		/// already enough primitives in the line batch.
		/// </summary>
		/// <param name="p0">The starting position</param>
		/// <param name="p1">The ending position</param>
		/// <param name="color0">The color of the line at the starting position</param>
		/// <param name="color1">The color of the line at the ending position</param>
		private static void BatchLine(Vector2 p0, Vector2 p1, Color color0, Color color1)
		{
			// Any need to flush?
			if (lines.Count / 2 >= PrimitivesPerBatch)
			{
				// Need to flush both sets (lines and polygons) since their
				// depths may be interleaved at this point
				FlushShapes();
			}

			// Add the vertices to the line list
			lines.Add(new VertexPositionColor(PixelsToNDCs(p0, currentDepth), color0));
			lines.Add(new VertexPositionColor(PixelsToNDCs(p1, currentDepth), color1));
		}


		/// <summary>
		/// Batches either a true, 1-pixel-wide line or a set of triangles for wider lines.
		/// Potentially causes a Flush() if there are already enough primitives in either batch.
		/// </summary>
		/// <param name="p0">The starting position</param>
		/// <param name="p1">The ending position</param>
		/// <param name="width">The width of the line (minimum 1)</param>
		/// <param name="color0">The color of the line at the starting position</param>
		/// <param name="color1">The color of the line at the ending position</param>
		private static void BatchWideLine(Vector2 p0, Vector2 p1, float width, Color color0, Color color1)
		{
			// Minimum of 1 on the width
			width = MathF.Max(1.0f, width);

			// An actual single-pixel line?
			if (width == 1)
			{
				// Yup, just batch a simple line
				BatchLine(p0, p1, color0, color1);
			}
			else
			{
				// Nope, need to draw a rotated box

				// Calculate a vector that's perpendicular to
				// the line, but half the desired width
				Vector2 dir = p1 - p0;
				Vector2 halfWidthPerp = Vector2.Normalize(new Vector2(dir.Y, -dir.X));
				halfWidthPerp *= width / 2.0f;

				// Create the corners
				Vector2 topLeft = p0 - halfWidthPerp;
				Vector2 topRight = p0 + halfWidthPerp;
				Vector2 bottomRight = p1 + halfWidthPerp;
				Vector2 bottomLeft = p1 - halfWidthPerp;

				// Create the polygons (these should always have the correct winding order!)
				BatchPolygon(topLeft, bottomRight, bottomLeft, color0, color1, color1);
				BatchPolygon(topLeft, topRight, bottomRight, color0, color0, color1);
			}
		}


		/// <summary>
		/// Adds a single triangle to the polygon batch.  Potentially causes a Flush()
		/// if there are already enough primitives in the polygon batch.
		/// </summary>
		/// <param name="p0">First vertex position</param>
		/// <param name="p1">Second vertex position</param>
		/// <param name="p2">Third vertex position</param>
		/// <param name="color0">The color of the first vertex</param>
		/// <param name="color1">The color of the second vertex</param>
		/// <param name="color2">The color of the third vertex</param>
		private static void BatchPolygon(Vector2 p0, Vector2 p1, Vector2 p2, Color color0, Color color1, Color color2)
		{
			// Any need to flush?
			if (polygons.Count / 3 >= PrimitivesPerBatch)
			{
				// Need to flush both sets (lines and polygons) since their
				// depths may be interleaved at this point
				FlushShapes();
			}

			// Add the vertices to the polygon list
			polygons.Add(new VertexPositionColor(PixelsToNDCs(p0, currentDepth), color0));
			polygons.Add(new VertexPositionColor(PixelsToNDCs(p1, currentDepth), color1));
			polygons.Add(new VertexPositionColor(PixelsToNDCs(p2, currentDepth), color2));
		}


		/// <summary>
		/// Immediately attempts to draw any batched shapes, if they exist.  This will
		/// set up the rendering pipeline, reset the current depth and clear the depth buffer.
		/// </summary>
		private static void FlushShapes()
		{
			// Ensure the current effect is active
			effect.CurrentTechnique.Passes[0].Apply();

			// Perform the draw if necessary
			if (lines.Count > 0)
			{
				device.DrawUserPrimitives(PrimitiveType.LineList, lines.ToArray(), 0, lines.Count / 2);
				lines.Clear();
			}

			if (polygons.Count > 0)
			{
				device.DrawUserPrimitives(PrimitiveType.TriangleList, polygons.ToArray(), 0, polygons.Count / 3);
				polygons.Clear();
			}

			// Reset the depth and clear the depth buffer
			currentDepth = 1.0f;
			device.Clear(ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
		}


		/// <summary>
		/// Proceeds to the next depth value
		/// </summary>
		private static void NextDepth()
		{
			currentDepth -= DepthStep;
		}


		/// <summary>
		/// Converts a pixel position within the window to Normalized Device
		/// Coordinates (-1 to 1 on both X and Y), using the specified depth
		/// for the Z position.  This flips the Y as pixel coordinates are
		/// inverted on the Y from NDCs.
		/// </summary>
		/// <param name="x">Pixel x position</param>
		/// <param name="y">Pixel y position</param>
		/// <param name="depth">Depth value to use for the final NDC</param>
		/// <returns>A 3-component vector where X and Y are between -1 and 1</returns>
		private static Vector3 PixelsToNDCs(float x, float y, float depth)
		{
			return new Vector3(
				x / device.PresentationParameters.BackBufferWidth * 2 - 1,
				-y / device.PresentationParameters.BackBufferHeight * 2 + 1,
				depth);
		}


		/// <summary>
		/// Converts a pixel position within the window to Normalized Device
		/// Coordinates (-1 to 1 on both X and Y), using the specified depth
		/// for the Z position.  This flips the Y as pixel coordinates are
		/// inverted on the Y from NDCs.
		/// </summary>
		/// <param name="pixelPosition">Pixel position</param>
		/// <param name="depth">Depth value to use for the final NDC</param>
		/// <returns>A 3-component vector where X and Y are between -1 and 1</returns>
		private static Vector3 PixelsToNDCs(Vector2 pixelPosition, float depth)
		{
			return PixelsToNDCs(pixelPosition.X, pixelPosition.Y, depth);
		}
	}
}

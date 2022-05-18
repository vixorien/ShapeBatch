# ShapeBatch
A simple 2D primitive shape drawing system for MonoGame

## Setup
Copy the ShapeBatch.cs file into your project.  The ShapeBatch static class is in the ShapeUtils namespace.

## Basic Usage
Use ShapeBatch during the Draw() method in MonoGame

```
ShapeBatch.Begin(GraphicsDevice);

// Call any of the static shape methods here, such as:
// ShapeBatch.Line();
// ShapeBatch.Box();
// ShapeBatch.BoxOutline();
// ShapeBatch.Circle();
// ShapeBatch.CircleOutline();
// ShapeBatch.Triangle();
// ShapeBatch.TriangleOutline();
// The parameters vary per method.  Multiple overloads are provided for ease of use.

ShapeBatch.End();
```

## Notes
- This is separate from SpriteBatch and, as such, should not be intermingled with an active SpriteBatch
- If you need to draw both sprites and shapes, begin and end those batches separately

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhad.Core
{
    /// <summary>
    /// Defines the contract for graphics operations required by the Dhad language.
    /// This interface will be implemented by the UI layer (e.g., Dhad.IDE).
    /// </summary>
    public interface IGraphics
    {
        /// <summary>
        /// Clears the drawing surface to the default background color.
        /// </summary>
        void ClearSurface();

        /// <summary>
        /// Sets the current drawing color for lines and outlines.
        /// </summary>
        /// <param name="colorName">The Dhad language color name (e.g., "أحمر").</param>
        void SetPenColor(string colorName);

        /// <summary>
        /// Sets the width (thickness) of the drawing pen.
        /// </summary>
        /// <param name="width">The width in pixels.</param>
        void SetPenWidth(int width);

        /// <summary>
        /// Draws a single point at the specified coordinates.
        /// </summary>
        void DrawPoint(int x, int y);

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        void DrawLine(int x1, int y1, int x2, int y2);

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="x">X-coordinate of the top-left corner of the bounding rectangle.</param>
        /// <param name="y">Y-coordinate of the top-left corner of the bounding rectangle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="fillColorName">Optional Dhad language color name to fill the circle.</param>
        void DrawCircle(int x, int y, int radius, string? fillColorName = null);

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="x">X-coordinate of the top-left corner.</param>
        /// <param name="y">Y-coordinate of the top-left corner.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="fillColorName">Optional Dhad language color name to fill the rectangle.</param>
        void DrawRectangle(int x, int y, int width, int height, string? fillColorName = null);

        /// <summary>
        /// Draws text at the specified coordinates.
        /// </summary>
        /// <param name="x">X-coordinate of the top-left corner of the text.</param>
        /// <param name="y">Y-coordinate of the top-left corner of the text.</param>
        /// <param name="text">The text to draw.</param>
        void DrawText(int x, int y, string text);

        /// <summary>
        /// (Optional) Sets the color used for filling shapes and drawing text.
        /// </summary>
        /// <param name="colorName">The Dhad language color name.</param>
        // void SetFillColor(string colorName);

        /// <summary>
        /// (Optional) Sets the font used for drawing text.
        /// </summary>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="size">Size of the font.</param>
        // void SetFont(string fontName, int size);

        /// <summary>
        /// Ensures the drawing surface is visible to the user.
        /// Called before the first drawing command in a program run.
        /// </summary>
        void EnsureVisible();

    }
}
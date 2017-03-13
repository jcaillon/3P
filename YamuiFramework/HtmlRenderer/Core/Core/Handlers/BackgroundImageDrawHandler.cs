#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (BackgroundImageDrawHandler.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================

#endregion

using System;
using YamuiFramework.HtmlRenderer.Core.Adapters;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;
using YamuiFramework.HtmlRenderer.Core.Core.Dom;

namespace YamuiFramework.HtmlRenderer.Core.Core.Handlers {
    /// <summary>
    /// Contains all the paint code to paint different background images.
    /// </summary>
    internal static class BackgroundImageDrawHandler {
        /// <summary>
        /// Draw the background image of the given box in the given rectangle.<br/>
        /// Handle background-repeat and background-position values.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="box">the box to draw its background image</param>
        /// <param name="imageLoadHandler">the handler that loads image to draw</param>
        /// <param name="rectangle">the rectangle to draw image in</param>
        public static void DrawBackgroundImage(RGraphics g, CssBox box, ImageLoadHandler imageLoadHandler, RRect rectangle) {
            // image size depends if specific rectangle given in image loader
            var imgSize = new RSize(imageLoadHandler.Rectangle == RRect.Empty ? imageLoadHandler.Image.Width : imageLoadHandler.Rectangle.Width,
                imageLoadHandler.Rectangle == RRect.Empty ? imageLoadHandler.Image.Height : imageLoadHandler.Rectangle.Height);

            // get the location by BackgroundPosition value
            var location = GetLocation(box.BackgroundPosition, rectangle, imgSize);

            var srcRect = imageLoadHandler.Rectangle == RRect.Empty
                ? new RRect(0, 0, imgSize.Width, imgSize.Height)
                : new RRect(imageLoadHandler.Rectangle.Left, imageLoadHandler.Rectangle.Top, imgSize.Width, imgSize.Height);

            // initial image destination rectangle
            var destRect = new RRect(location, imgSize);

            // need to clip so repeated image will be cut on rectangle
            var lRectangle = rectangle;
            lRectangle.Intersect(g.GetClip());
            g.PushClip(lRectangle);

            switch (box.BackgroundRepeat) {
                case "no-repeat":
                    g.DrawImage(imageLoadHandler.Image, destRect, srcRect);
                    break;
                case "repeat-x":
                    DrawRepeatX(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                    break;
                case "repeat-y":
                    DrawRepeatY(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                    break;
                default:
                    DrawRepeat(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                    break;
            }

            g.PopClip();
        }

        #region Private methods

        /// <summary>
        /// Get top-left location to start drawing the image at depending on background-position value.
        /// </summary>
        /// <param name="backgroundPosition">the background-position value</param>
        /// <param name="rectangle">the rectangle to position image in</param>
        /// <param name="imgSize">the size of the image</param>
        /// <returns>the top-left location</returns>
        private static RPoint GetLocation(string backgroundPosition, RRect rectangle, RSize imgSize) {
            double left = rectangle.Left;
            if (backgroundPosition.IndexOf("left", StringComparison.OrdinalIgnoreCase) > -1) {
                left = (rectangle.Left + .5f);
            } else if (backgroundPosition.IndexOf("right", StringComparison.OrdinalIgnoreCase) > -1) {
                left = rectangle.Right - imgSize.Width;
            } else if (backgroundPosition.IndexOf("0", StringComparison.OrdinalIgnoreCase) < 0) {
                left = (rectangle.Left + (rectangle.Width - imgSize.Width)/2 + .5f);
            }

            double top = rectangle.Top;
            if (backgroundPosition.IndexOf("top", StringComparison.OrdinalIgnoreCase) > -1) {
                top = rectangle.Top;
            } else if (backgroundPosition.IndexOf("bottom", StringComparison.OrdinalIgnoreCase) > -1) {
                top = rectangle.Bottom - imgSize.Height;
            } else if (backgroundPosition.IndexOf("0", StringComparison.OrdinalIgnoreCase) < 0) {
                top = (rectangle.Top + (rectangle.Height - imgSize.Height)/2 + .5f);
            }

            return new RPoint(left, top);
        }

        /// <summary>
        /// Draw the background image at the required location repeating it over the X axis.<br/>
        /// Adjust location to left if starting location doesn't include all the range (adjusted to center or right).
        /// </summary>
        private static void DrawRepeatX(RGraphics g, ImageLoadHandler imageLoadHandler, RRect rectangle, RRect srcRect, RRect destRect, RSize imgSize) {
            while (destRect.X > rectangle.X)
                destRect.X -= imgSize.Width;

            using (var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location)) {
                g.DrawRectangle(brush, rectangle.X, destRect.Y, rectangle.Width, srcRect.Height);
            }
        }

        /// <summary>
        /// Draw the background image at the required location repeating it over the Y axis.<br/>
        /// Adjust location to top if starting location doesn't include all the range (adjusted to center or bottom).
        /// </summary>
        private static void DrawRepeatY(RGraphics g, ImageLoadHandler imageLoadHandler, RRect rectangle, RRect srcRect, RRect destRect, RSize imgSize) {
            while (destRect.Y > rectangle.Y)
                destRect.Y -= imgSize.Height;

            using (var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location)) {
                g.DrawRectangle(brush, destRect.X, rectangle.Y, srcRect.Width, rectangle.Height);
            }
        }

        /// <summary>
        /// Draw the background image at the required location repeating it over the X and Y axis.<br/>
        /// Adjust location to left-top if starting location doesn't include all the range (adjusted to center or bottom/right).
        /// </summary>
        private static void DrawRepeat(RGraphics g, ImageLoadHandler imageLoadHandler, RRect rectangle, RRect srcRect, RRect destRect, RSize imgSize) {
            while (destRect.X > rectangle.X)
                destRect.X -= imgSize.Width;
            while (destRect.Y > rectangle.Y)
                destRect.Y -= imgSize.Height;

            using (var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location)) {
                g.DrawRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }
        }

        #endregion
    }
}
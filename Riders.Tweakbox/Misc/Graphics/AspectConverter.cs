using Riders.Tweakbox.Misc.Graphics.Structs;

namespace Riders.Tweakbox.Misc.Graphics
{
    public class AspectConverter
    {
        /// <summary>
        /// Original aspect ratio of the game.
        /// </summary>
        public const float OriginalGameAspect = 4F / 3F;

        /// <summary>
        /// The width the game elements were programmed to be positioned against.
        /// </summary>
        public const float GameCanvasWidth = 640;

        /// <summary>
        /// The height the game elements were programmed to be positioned against.
        /// </summary>
        public const float GameCanvasHeight = 480;

        /// <summary>
        /// Beyond this aspect the screen should scale horizontally, else vertically.
        /// </summary>
        public float AspectRatioLimit { get; set; }

        public AspectConverter(float aspectRatioLimit)
        {
            AspectRatioLimit = aspectRatioLimit;
        }

        /// <summary>
        /// Converts a fixed width and aspect ratio to a width-height resolution pair.
        /// </summary>
        /// <param name="width">The width of the resolution to produce.</param>
        /// <param name="aspectRatio">The aspect ratio of the resolution to produce. e.g. 4F/3F.</param>
        /// <param name="resolution">The final output resolution where the width equals <see cref="width"/> and height is variable.</param>
        public static void WidthToResolution(int width, float aspectRatio, out Resolution resolution)
        {
            resolution = new Resolution
            {
                Width = width,
                Height = (int) (width / aspectRatio)
            };
        }

        /// <summary>
        /// Converts a fixed height and aspect ratio to a width-height resolution pair.
        /// </summary>
        /// <param name="height">The height of the resolution to produce.</param>
        /// <param name="aspectRatio">The aspect ratio of the resolution to produce. e.g. 4F/3F.</param>
        /// <param name="resolution">The final output resolution where the height equals <see cref="height"/> and width is variable.</param>
        public static void HeightToResolution(int height, float aspectRatio, out Resolution resolution)
        {
            resolution = new Resolution
            {
                Height = height,
                Width = (int) (height * aspectRatio)
            };
        }

        /// <summary>
        /// Obtains the relative aspect ratio of a given aspect compared to the game's aspect.
        /// </summary>
        /// <param name="currentAspect">The current aspect of the game window.</param>
        /// <returns>The aspect of the game window relative to the original aspect of the game.</returns>
        public static float GetRelativeAspect(float currentAspect)
        {
            return currentAspect / OriginalGameAspect;
        }

        /// <summary>
        /// Scales a width value by the relative aspect ratio.
        /// </summary>
        public float ScaleByRelativeAspectX(float value, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect > AspectRatioLimit)
            {
                return value / relativeAspectRatio;
            }

            return value;
        }

        /// <summary>
        /// Scales a height value by the relative aspect ratio.
        /// </summary>
        public float ScaleByRelativeAspectY(float value, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect < AspectRatioLimit)
            {
                return value * relativeAspectRatio;
            }

            return value;
        }

        /// <summary>
        /// Returns the extra width added by the left and right borders extending beyond the 4:3 aspect.
        /// </summary>
        public float GetBorderWidthX(float actualAspect, float height)
        {
            if (actualAspect > AspectRatioLimit)
            {
                AspectConverter.HeightToResolution((int)height, actualAspect, out Resolution resolutionOurAspect);
                AspectConverter.HeightToResolution((int)height, AspectConverter.OriginalGameAspect, out Resolution resolutionGameAspect);

                return resolutionOurAspect.Width - resolutionGameAspect.Width;
            }

            return 0;
        }

        /// <summary>
        /// Returns the extra height added by the top and bottom borders extending before the 4:3 aspect.
        /// </summary>
        public float GetBorderHeightY(float actualAspect, float width)
        {
            if (actualAspect < AspectRatioLimit)
            {
                AspectConverter.WidthToResolution((int)width, actualAspect, out Resolution resolutionOurAspect);
                AspectConverter.WidthToResolution((int)width, AspectConverter.OriginalGameAspect, out Resolution resolutionGameAspect);

                return resolutionOurAspect.Height - resolutionGameAspect.Height;
            }

            return 0;
        }

        /// <summary>
        /// Used for shifting item locations of an orthographic projection (e.g. special stage HUD)
        /// that are relative to the left edge of the screen.
        /// Note: Assumes resolution is 640x480.
        /// </summary>
        /// <param name="originalPosition">Original position of the object.</param>
        /// <param name="relativeAspectRatio">Relative aspect ratio of the desired aspect compared to game's aspect.</param>
        /// <param name="actualAspect">The desired aspect ratio.</param>
        public float ProjectFromOldToNewCanvasX(float originalPosition, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect > AspectRatioLimit)
            {
                // Now the projection is the right size, however it is not centered to our screen.
                AspectConverter.HeightToResolution((int)GameCanvasHeight, actualAspect, out Resolution resolution); // Get resolution with our aspect equal to the height. 
                float borderWidth = resolution.Width - GameCanvasWidth;   // Get the extra width (left and right border)
                float leftBorderOnly = (borderWidth / 2);                    // We only want left border.
                float originalPlusLeftBorder = leftBorderOnly + originalPosition;

                return originalPlusLeftBorder / relativeAspectRatio;
            }

            return originalPosition;
        }

        /// <summary>
        /// Used for shifting item locations of an orthographic projection (e.g. special stage HUD)
        /// that are relative to the top edge of the screen.
        /// Note: Assumes resolution is 640x480.
        /// </summary>
        /// <param name="originalPosition">Original position of the object.</param>
        /// <param name="relativeAspectRatio">Relative aspect ratio of the desired aspect compared to game's aspect.</param>
        /// <param name="actualAspect">The desired aspect ratio.</param>
        public float ProjectFromOldToNewCanvasY(float originalPosition, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect < AspectRatioLimit)
            {
                // Now the projection is the right size, however it is not centered to our screen.
                AspectConverter.WidthToResolution((int)GameCanvasWidth, actualAspect, out Resolution resolution); // Get resolution with our aspect equal to the height. 
                float borderHeight = resolution.Height - GameCanvasHeight;   // Get the extra height (top and bottom border)
                float topBorderOnly = (borderHeight / 2);                    // We only want top border.
                float originalPlusTopBorder = topBorderOnly + originalPosition; // Our top border is in the aspect ratio it originated from,
                                                                                // we need to scale it to the new ratio.

                return originalPlusTopBorder * relativeAspectRatio;
            }

            return originalPosition;
        }
    }
}

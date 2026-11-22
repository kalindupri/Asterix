using UnityEngine;

namespace Ironoak.Core
{
    public enum DisplayMode
    {
        /// <summary>Legacy pixel-art presentation: 480x270 render texture, point-upscaled.</summary>
        PixelPerfect,
        /// <summary>Painted-art presentation: the camera renders straight to the screen at full resolution.</summary>
        Native
    }

    /// <summary>
    /// Project-wide presentation switches. Edit Assets/_Ironoak/Resources/IronoakConfig.asset
    /// (created by the setup pipeline). Command-line overrides: -ironoak-pixel, -ironoak-pixel-player.
    /// </summary>
    [CreateAssetMenu(menuName = "Ironoak/Config", fileName = "IronoakConfig")]
    public class IronoakConfig : ScriptableObject
    {
        public DisplayMode displayMode = DisplayMode.Native;
        [Tooltip("Use the painted cut-out puppet for Brenna instead of the pixel sprite frames.")]
        public bool cutoutPlayer = true;
        [Tooltip("Native mode: default view height in world units (tiles). Camera zones can zoom out from this.")]
        [Range(7f, 17f)] public float viewHeight = 11f;
        [Tooltip("Native mode: the widest a camera zone may zoom out to (view height in units).")]
        [Range(10f, 24f)] public float maxZoneViewHeight = 17f;

        static IronoakConfig _current;

        public static IronoakConfig Current
        {
            get
            {
                if (_current != null) return _current;
                var asset = Resources.Load<IronoakConfig>("IronoakConfig");
                _current = asset != null ? Instantiate(asset) : CreateInstance<IronoakConfig>();
                foreach (var arg in System.Environment.GetCommandLineArgs())
                {
                    if (arg == "-ironoak-pixel") _current.displayMode = DisplayMode.PixelPerfect;
                    if (arg == "-ironoak-pixel-player") _current.cutoutPlayer = false;
                }
                return _current;
            }
        }

        public static bool PixelPerfect => Current.displayMode == DisplayMode.PixelPerfect;

        /// <summary>Default orthographic half-height for the current display mode.</summary>
        public static float DefaultHalfHeight => PixelPerfect ? 270f / 2f / 16f : Current.viewHeight * 0.5f;
    }
}

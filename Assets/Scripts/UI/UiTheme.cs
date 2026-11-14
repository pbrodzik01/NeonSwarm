using UnityEngine;

namespace NeonSwarm
{
    [CreateAssetMenu(menuName = "Neon Swarm/UI Theme", fileName = "UiTheme")]
    public class UiTheme : ScriptableObject
    {
        public Font font;

        [Header("Sprites")]
        public Sprite rounded;
        public Sprite roundedOutline;
        public Sprite circle;
        public Sprite joystickBase;
        public Sprite joystickKnob;
        public Sprite heart;
        public Sprite heartEmpty;
        public Sprite coin;
        public Sprite pause;
        public Sprite pulse;
        public Sprite lockIcon;
        public Sprite star;
        public Sprite trophy;
        public Sprite shield;
        public Sprite rapid;
        public Sprite ship;

        [Header("Colours")]
        public Color background = new Color(0.04f, 0.06f, 0.12f, 1f);
        public Color panel = new Color(0.07f, 0.1f, 0.2f, 0.96f);
        public Color panelLight = new Color(0.12f, 0.16f, 0.3f, 1f);
        public Color primary = new Color(0.15f, 0.91f, 1f, 1f);
        public Color accent = new Color(1f, 0.24f, 0.8f, 1f);
        public Color gold = new Color(1f, 0.82f, 0.25f, 1f);
        public Color success = new Color(0.3f, 1f, 0.55f, 1f);
        public Color danger = new Color(1f, 0.3f, 0.37f, 1f);
        public Color text = Color.white;
        public Color textMuted = new Color(0.66f, 0.73f, 0.86f, 1f);
        public Color dark = new Color(0.02f, 0.03f, 0.07f, 1f);

        private static UiTheme current;

        public static void ClearCache()
        {
            current = null;
        }

        public static UiTheme Current
        {
            get
            {
                if (current == null)
                {
                    current = Resources.Load<UiTheme>("UiTheme");
                    if (current == null)
                    {
                        current = CreateInstance<UiTheme>();
                    }
                }
                if (current.font == null)
                {
                    current.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return current;
            }
        }
    }
}

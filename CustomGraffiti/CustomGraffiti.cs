using Reptile;
using Reptile.Phone;
using UnityEngine;

namespace CustomGraffiti
{
    public class CustomGraffiti
    {
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                _appEntry.Title = value;
                _art.title = value;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                _texture = value;
                _appEntry.GraffitiTexture = value;
                _art.graffitiMaterial.mainTexture = value;
            }
        }

        public GraffitiSize Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                _appEntry.Size = value;
                _art.graffitiSize = value;
            }
        }

        public GraffitiArt.Combos Combo
        {
            get
            {
                return _combo;
            }
            set
            {
                _combo = value;
                _appEntry.Combos = value;
                _art.combos = value;
            }
        }

        public GraffitiArt.Combos combos => Combo;

        public GraffitiArt Art => _art;
        public GraffitiAppEntry AppEntry => _appEntry;

        private string _name;
        private Texture2D _texture;
        private GraffitiSize _size;
        private GraffitiArt.Combos _combo;

        private GraffitiAppEntry _appEntry;
        private GraffitiArt _art;

        public CustomGraffiti()
        {
            _art = new GraffitiArt();
            _art.graffitiMaterial = new Material(Shader.Find("Standard"));

            _texture = null;
            _size = GraffitiSize.S;
            _combo = GraffitiArt.Combos.NONE;
            _name = "Empty Custom Graffiti";

            _appEntry = ScriptableObject.CreateInstance<GraffitiAppEntry>();
            _art.unlockable = _appEntry;
        }

        public void AddToGraffitiArtInfo()
        {
            GraffitiLoader loader = GetGraffitiLoader();

            if (loader == null)
            {
                Debug.LogWarning("GraffitiLoader was null.");
                return;
            }

            if (loader.GraffitiArtInfo == null)
            {
                Debug.LogWarning("GraffitiArtInfo was null.");
                return;
            }

            if (loader.GraffitiArtInfo.graffitiArt == null)
            {
                Debug.LogWarning("graffitiArt List was null.");
                return;
            }

            foreach (GraffitiArt art in loader.GraffitiArtInfo.graffitiArt)
            {
                if (art == Art)
                {
                    Debug.LogWarning("This custom graffiti is already in the graffiti art info. Returning.");
                    return;
                }
            }

            loader.GraffitiArtInfo.graffitiArt.Add(Art);
        }

        public void FixShader()
        {
            FixShaderGraffiti(_art.graffitiMaterial);
        }

        public static void FixShaderGraffiti(Material material)
        {
            GraffitiLoader loader = GetGraffitiLoader();

            if (loader == null)
            {
                Debug.LogWarning("Failed to fix shader graffiti.");
                return;
            }

            Material gameMaterial = loader.GraffitiArtInfo.FindByCharacter(Characters.metalHead).graffitiMaterial;
            material.shader = gameMaterial.shader;
        }

        public static GraffitiLoader GetGraffitiLoader()
        {
            if (Core.Instance == null)
            {
                //Debug.LogWarning("Core instance not initialized yet.");
                return null;
            }

            if (Core.Instance.Assets == null)
            {
                Debug.LogWarning("Core Assets not initialized yet.");
                return null;
            }

            return Core.Instance.Assets.GetAssetsLoader<GraffitiLoader>();
        }
    }
}

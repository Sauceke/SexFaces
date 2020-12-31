using BepInEx;
using KKAPI;
using KKAPI.Chara;

namespace KK_SexFaces
{
    [BepInPlugin(GUID, "Sex Faces", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    internal class SexFacesPlugin : BaseUnityPlugin
    {
        public const string GUID = "Sauceke.SexFaces";
        public const string Version = "1.0.0";

        private void Start()
        {
            Hooks.InstallHooks();
            CharacterApi.RegisterExtraBehaviour<SexFacesController>(GUID);
            SexFacesGui.Init(this);
        }
    }
}

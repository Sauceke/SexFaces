using BepInEx;
using BepInEx.Logging;
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
        public static new ManualLogSource Logger;

        private void Start()
        {
            Logger = base.Logger;
            Hooks.InstallHooks();
            CharacterApi.RegisterExtraBehaviour<SexFacesController>(GUID);
            SexFacesGui.Init(this);
        }
    }
}

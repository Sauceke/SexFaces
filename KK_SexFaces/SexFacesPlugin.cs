using BepInEx;
using BepInEx.Logging;
using AutoVersioning;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

namespace KK_SexFaces
{
    [BepInPlugin(GUID, "Sex Faces", VersionInfo.Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    internal class SexFacesPlugin : BaseUnityPlugin
    {
        public const string GUID = "Sauceke.SexFaces";
        public static new ManualLogSource Logger;

        private void Start()
        {
            Logger = base.Logger;
            Hooks.InstallHooks();
            GameAPI.RegisterExtraBehaviour<GameController>(GUID);
            CharacterApi.RegisterExtraBehaviour<SexFacesController>(GUID);
            SexFacesGui.Init(this);
        }
    }
}

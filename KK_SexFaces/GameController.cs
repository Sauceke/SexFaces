using KKAPI.MainGame;

namespace KK_SexFaces
{
    class GameController : GameCustomFunctionController
    {
        protected override void OnStartH(BaseLoader proc, HFlag hFlag, bool vr)
        {
            SexFacesPlugin.Logger.LogDebug("H scene started.");
            hFlag.lstHeroine.ForEach(heroine => GetController(heroine).RunLoop(
                hFlag, heroine.HExperience));
        }

        private static SexFacesController GetController(SaveData.Heroine heroine)
        {
            return heroine.chaCtrl.GetComponent<SexFacesController>();
        }
    }
}

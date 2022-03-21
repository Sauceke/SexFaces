using KKAPI.MainGame;
using UnityEngine;

namespace SexFaces
{
    internal class GameController : GameCustomFunctionController
    {
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            SexFacesPlugin.Logger.LogDebug("H scene started.");
            var hand = vr ? null : ((HSceneProc)proc).hand;
            hFlag.lstHeroine.ForEach(heroine =>
                heroine.GetSexFacesController().RunLoop(hFlag, heroine.HExperience, hand));
            hFlag.player.GetSexFacesController()
                .RunLoop(hFlag, hFlag.lstHeroine[0].HExperience, hand);
        }
    }
}

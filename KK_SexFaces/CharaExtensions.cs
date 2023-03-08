namespace SexFaces
{
    internal static class CharaExtensions
    {
        public static SexFacesController GetSexFacesController(this SaveData.Heroine heroine) =>
            heroine.chaCtrl.GetComponent<SexFacesController>();

        public static SexFacesController GetSexFacesController(this SaveData.Player player) =>
            player.chaCtrl.GetComponent<SexFacesController>();
    }
}

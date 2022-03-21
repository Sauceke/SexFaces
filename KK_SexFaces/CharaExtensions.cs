namespace SexFaces
{
    internal static class CharaExtensions
    {
        public static SexFacesController GetSexFacesController(this SaveData.Heroine heroine)
        {
            return heroine.chaCtrl.GetComponent<SexFacesController>();
        }

        public static SexFacesController GetSexFacesController(this SaveData.Player player)
        {
            return player.chaCtrl.GetComponent<SexFacesController>();
        }
    }
}

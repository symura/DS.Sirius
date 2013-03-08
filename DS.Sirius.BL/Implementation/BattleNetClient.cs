
using DS.Sirius.BL.Definition;
using DS.Sirius.Core.Models;
using DS.Sirius.DA.Definition;
using DS.Sirius.DA.Implementation;

namespace DS.Sirius.BL.Implementation
{
    public class BattleNetClient : IBattleNetClient
    {

        #region Singleton instance
        private static BattleNetClient _instance;

        public static BattleNetClient Current
        {
            get { return _instance ?? (_instance = new BattleNetClient()); }
        }
        #endregion        

        public static string Region { get; set; }

        public Career GetCareerByBattleTag(string battleTag)
        {
            var repository = GetRepository();

            battleTag = battleTag.Replace("#", "-");

            return repository.GetCareerByBattleTag(battleTag);
        }

        public Hero GetHeroByID(string battleTag, long id)
        {
            var repository = GetRepository();

            battleTag = battleTag.Replace("#", "-");

            return repository.GetHeroById(battleTag, id);

        }

        private static IBattleNetRepository GetRepository()
        {
            return new BattleNetRepository(Region);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using DS.Sirius.Core.BattleNet;
using DS.Sirius.Core.BattleNet.Models;
using DS.Sirius.Web.Models;

namespace DS.Sirius.Web.Controllers
{
    public class CharacterController : Controller
    {
        public ActionResult Character()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }


        public async Task<ActionResult> Profile(string Text)
        {

            //BattleNetClient.Current.GetCareerAsync(Text, GetCareer);

            //return RedirectToAction("Profile", await BattleNetClient.Current.GetCareerAsync(Text));

            var model = await BattleNetClient.Current.GetCareerAsync(Text);

            CareerModel cm = new CareerModel();
            cm.HeroCollection = new List<HeroModel>();
            if (model != null)
            {
                cm.HeroCollection.AddRange(model.heroes.Select(hero => new HeroModel() {HeroClass = hero.characterClass, HeroName = hero.name, HeroID = hero.id, BattleTag = Text}));
            }

            return View("Profile", cm); //RedirectToAction("Profile");
        }

        

        private void GetCareer(Career career)
        {
            if (career != null)
                Redirect("Character/Profile");                
        }

        public async Task<ActionResult> Hero(string battleTag, int heroID)
        {
            var model = await BattleNetClient.Current.GetHeroAsync(battleTag, heroID);

            var heroModel = new HeroModel();

            if (model != null)
            {


            }


            return View();
        }

    }
}

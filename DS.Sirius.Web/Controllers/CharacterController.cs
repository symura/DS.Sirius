using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using DS.Sirius.BL.Implementation;

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

            var model = BattleNetClient.Current.GetCareerByBattleTag(Text);
            
            return View("Profile", model); //RedirectToAction("Profile");
        }

               

        public async Task<ActionResult> Hero(string battleTag, long heroID)
        {
            var model = BattleNetClient.Current.GetHeroByID(battleTag, heroID);

            //var heroModel = new HeroModel();

            //if (model != null)
            //{


            //}


            return View();
        }

    }
}

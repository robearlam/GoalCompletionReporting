using System.Web.Mvc;
using GoalCompletionReporting.Web.Models;
using Sitecore.Analytics;
using Sitecore.Analytics.Model.Entities;

namespace GoalCompletionReporting.Web.Controllers
{
    public class IdentifyUserController : Controller
    {
        private const string EmailKey = "Preferred";

        [HttpGet]
        public ActionResult IdentifyUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult IdentifyUser(IdentifyUserModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ViewBag.Message = "Please enter an email address";
                return View(model);
            }

            IdentifyAndUpdateContact(model);
            ViewBag.Message = "Contact Identified";
            return View(model);
        }

        private static void IdentifyAndUpdateContact(IdentifyUserModel model)
        {
            Tracker.Current.Session.Identify(model.Email);
            var contact = Tracker.Current.Contact;
            var facet = contact.GetFacet<IContactEmailAddresses>("Emails");

            if (!facet.Entries.Contains(EmailKey))
            {
                facet.Entries.Create(EmailKey);
            }

            if (string.IsNullOrEmpty(facet.Preferred))
            {
                facet.Preferred = EmailKey;
            }

            facet.Entries[EmailKey].SmtpAddress = model.Email;
        }
    }
}
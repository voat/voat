using System.Web;
using System.Web.Mvc;
using SimpleHoneypot.Core;

[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.SimpleHoneypot), "Start")]

namespace $rootnamespace$.App_Start {
    public static class SimpleHoneypot {
        public static void Start() {
            RegisterHoneypotInputNames(Honeypot.InputNames);
            //Optional, configure the css class name for the honeypot input (default: input-imp-long)
            //Regardless, the class should be in your stylesheet with display: none

            //Honeypot.SetCssClassName("NewCssClassName");
        }
		
		public static void RegisterHoneypotInputNames(HoneypotInputNameCollection collection) {
            //Honeypot will use 2 words at random to create the input name {0}-{1}
            collection.Add(new[]
                           {
                               "User",
                               "Name",
                               "Age",
                               "Question",
                               "List",
                               "Why",
                               "Type",
                               "Phone",
                               "Fax",
                               "Custom",
                               "Relationship",
                               "Friend",
                               "Pet",
                               "Reason"
                           });
            //This is optional, if you don't want the honeypot input to generate a random input name per request
            //You can skip adding any items to the collection and set a DefaultInputName (default: Phone-Data-Home)

            //Honeypot.SetDefaultInputName("NewDefaultInputName");
        }
    }
}

import 'https://cdn.jsdelivr.net/gh/orestbida/cookieconsent@v3.0.0-rc.17/dist/cookieconsent.umd.js';

CookieConsent.run({
    guiOptions: {
        consentModal: {
            layout: "box inline",
            position: "bottom left",
            equalWeightButtons: false,
            flipButtons: true
        },
        preferencesModal: {
            layout: "bar wide",
            position: "right",
            equalWeightButtons: true,
            flipButtons: false
        }
    },
    categories: {
        necessary: {
            readOnly: true
        },
        analytics: {}
    },
    language: {
        default: "en",
        translations: {
            en: {
                consentModal: {
                    title: "Hello traveller, it's cookie time!",
                    description: "Guess what? We've got cookies! Not the snack, but the cool kind to boost your experience here. If that sounds good, hit accept and enjoy the site",
                    acceptAllBtn: "Accept",
                    acceptNecessaryBtn: "Reject",
                    showPreferencesBtn: "Manage preferences",
                    /*footer: "<a href=\"#link\">Privacy Policy</a>\n<a href=\"#link\">Terms and conditions</a>"*/
                },
                preferencesModal: {
                    title: "Consent Preferences Center",
                    acceptAllBtn: "Accept all",
                    acceptNecessaryBtn: "Reject all",
                    savePreferencesBtn: "Save preferences",
                    closeIconLabel: "Close modal",
                    serviceCounterLabel: "Service|Services",
                    sections: [
                        {
                            title: "Cookie Usage",
                            description: "Our website utilizes cookies, primarily for enhancing user experience and analyzing website performance. This includes employing Google Search Console and Analytics to understand user interactions and improve our services accordingly. By accepting, you agree to this data usage."
                        },
                        {
                            title: "Necessary Cookies <span class=\"pm__badge\">Required</span>",
                            description: "These will be cookies created by the framework, and to keep track of the version that are required for the application to run. This is very technical, but this site is written in Blazor Web assembly, which relies heavily on caching resources on the client. This is done to keep load times low and ensure offline capabilities. This is also why the version is necessary - so you the user knows what they are using.",
                            linkedCategory: "necessary"
                        },
                        {
                            title: "Analytics Cookies",
                            description: "These cookies allows WhiteHat to keep track of how many people uses the site, and provide information that helps us grow and improve.",
                            linkedCategory: "analytics"
                        },
                        {
                            title: "More information",
                            description: "If you want to reach out, have any questions or want to report any errors, please contact me <a class=\"cc__link\" href=\"https://oliver-brodersen.com\">here</a>."
                        }
                    ]
                }
            }
        }
    }
});
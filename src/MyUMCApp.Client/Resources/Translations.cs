namespace MyUMCApp.Client.Resources;

public static class Translations
{
    public static Dictionary<string, Dictionary<string, string>> Resources = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            // Common
            ["Welcome"] = "Welcome",
            ["Login"] = "Login",
            ["Register"] = "Register",
            ["Profile"] = "Profile",
            ["Events"] = "Events",
            ["Sermons"] = "Sermons",
            ["Store"] = "Store",
            ["Blog"] = "Blog",
            ["Announcements"] = "Announcements",
            ["Settings"] = "Settings",
            ["Logout"] = "Logout",

            // Authentication
            ["SignIn"] = "Sign In",
            ["SignInWith"] = "Sign in with {0}",
            ["SignUp"] = "Sign Up",
            ["ForgotPassword"] = "Forgot Password",
            ["RememberMe"] = "Remember Me",

            // Profile
            ["PersonalInfo"] = "Personal Information",
            ["FullName"] = "Full Name",
            ["Email"] = "Email",
            ["Phone"] = "Phone",
            ["Organization"] = "Organization",
            ["SaveChanges"] = "Save Changes",
            ["ChurchHistory"] = "Church History",
            ["GivingHistory"] = "Giving History",

            // Events
            ["UpcomingEvents"] = "Upcoming Events",
            ["EventDetails"] = "Event Details",
            ["Location"] = "Location",
            ["Date"] = "Date",
            ["Time"] = "Time",
            ["Register"] = "Register",
            ["RegisterForEvent"] = "Register for Event",

            // Store
            ["AddToCart"] = "Add to Cart",
            ["RemoveFromCart"] = "Remove from Cart",
            ["Checkout"] = "Checkout",
            ["Price"] = "Price",
            ["Quantity"] = "Quantity",
            ["Total"] = "Total",
            ["ShoppingCart"] = "Shopping Cart",

            // Admin
            ["Dashboard"] = "Dashboard",
            ["ManageUsers"] = "Manage Users",
            ["ManageEvents"] = "Manage Events",
            ["ManageContent"] = "Manage Content",
            ["ManageStore"] = "Manage Store",
            ["AddNew"] = "Add New",
            ["Edit"] = "Edit",
            ["Delete"] = "Delete",

            // Validation
            ["Required"] = "This field is required",
            ["InvalidEmail"] = "Please enter a valid email address",
            ["InvalidPhone"] = "Please enter a valid phone number",
            ["PasswordMismatch"] = "Passwords do not match",
        },

        ["sn"] = new Dictionary<string, string>
        {
            // Common
            ["Welcome"] = "Mauya",
            ["Login"] = "Pinda",
            ["Register"] = "Nyoresa",
            ["Profile"] = "Pfungwa",
            ["Events"] = "Zviitiko",
            ["Sermons"] = "Mharidzo",
            ["Store"] = "Chitoro",
            ["Blog"] = "Nhau",
            ["Announcements"] = "Zvisungo",
            ["Settings"] = "Zvimiso",
            ["Logout"] = "Buda",

            // Authentication
            ["SignIn"] = "Pinda",
            ["SignInWith"] = "Pinda ne {0}",
            ["SignUp"] = "Nyoresa",
            ["ForgotPassword"] = "Wakanganwa Password",
            ["RememberMe"] = "Ndirangarire",

            // Profile
            ["PersonalInfo"] = "Ruzivo Rwemunhu",
            ["FullName"] = "Zita Rizere",
            ["Email"] = "Email",
            ["Phone"] = "Runhare",
            ["Organization"] = "Sangano",
            ["SaveChanges"] = "Chengetedza Shanduko",
            ["ChurchHistory"] = "Nhoroondo YeChechi",
            ["GivingHistory"] = "Nhoroondo YeKupa",

            // Events
            ["UpcomingEvents"] = "Zviitiko Zvinouya",
            ["EventDetails"] = "Tsanangudzo YeChiitiko",
            ["Location"] = "Nzvimbo",
            ["Date"] = "Zuva",
            ["Time"] = "Nguva",
            ["Register"] = "Nyoresa",
            ["RegisterForEvent"] = "Nyoresa Chiitiko",

            // Store
            ["AddToCart"] = "Wedzera MuTroko",
            ["RemoveFromCart"] = "Bvisa MuTroko",
            ["Checkout"] = "Bhadhara",
            ["Price"] = "Mutengo",
            ["Quantity"] = "Uwandu",
            ["Total"] = "Zvose",
            ["ShoppingCart"] = "Troko Yekutenga",

            // Admin
            ["Dashboard"] = "Dashboard",
            ["ManageUsers"] = "Tarisa Vashandisi",
            ["ManageEvents"] = "Tarisa Zviitiko",
            ["ManageContent"] = "Tarisa Zvinyorwa",
            ["ManageStore"] = "Tarisa Chitoro",
            ["AddNew"] = "Wedzera Zvitsva",
            ["Edit"] = "Gadzirisa",
            ["Delete"] = "Dzima",

            // Validation
            ["Required"] = "Panoda kuzadziswa",
            ["InvalidEmail"] = "Ndapota isa email yakarurama",
            ["InvalidPhone"] = "Ndapota isa nhamba yakarurama",
            ["PasswordMismatch"] = "MaPassword haana kufanana",
        },

        ["nd"] = new Dictionary<string, string>
        {
            // Common
            ["Welcome"] = "Wamukelekile",
            ["Login"] = "Ngena",
            ["Register"] = "Bhalisa",
            ["Profile"] = "Imininingwane",
            ["Events"] = "Izehlakalo",
            ["Sermons"] = "Intshumayelo",
            ["Store"] = "Isitolo",
            ["Blog"] = "Ibhulogi",
            ["Announcements"] = "Izaziso",
            ["Settings"] = "Izilungiselelo",
            ["Logout"] = "Phuma",

            // Authentication
            ["SignIn"] = "Ngena",
            ["SignInWith"] = "Ngena nge {0}",
            ["SignUp"] = "Bhalisa",
            ["ForgotPassword"] = "Ukhohlwe Iphasiwedi",
            ["RememberMe"] = "Ngikhumbule",

            // Profile
            ["PersonalInfo"] = "Imininingwane Yakho",
            ["FullName"] = "Ibizo Eliphelele",
            ["Email"] = "I-imeyili",
            ["Phone"] = "Ucingo",
            ["Organization"] = "Inhlangano",
            ["SaveChanges"] = "Londoloza Izinguquko",
            ["ChurchHistory"] = "Umlando Wesonto",
            ["GivingHistory"] = "Umlando Wokupha",

            // Events
            ["UpcomingEvents"] = "Izehlakalo Ezizayo",
            ["EventDetails"] = "Imininingwane Yesehlo",
            ["Location"] = "Indawo",
            ["Date"] = "Usuku",
            ["Time"] = "Isikhathi",
            ["Register"] = "Bhalisa",
            ["RegisterForEvent"] = "Bhalisa Isehlo",

            // Store
            ["AddToCart"] = "Faka Enqoleni",
            ["RemoveFromCart"] = "Khipha Enqoleni",
            ["Checkout"] = "Thenga",
            ["Price"] = "Intengo",
            ["Quantity"] = "Inani",
            ["Total"] = "Isamba",
            ["ShoppingCart"] = "Inqola Yokuthenga",

            // Admin
            ["Dashboard"] = "Ibhodi",
            ["ManageUsers"] = "Phatha Abasebenzisi",
            ["ManageEvents"] = "Phatha Izehlakalo",
            ["ManageContent"] = "Phatha Okuqukethwe",
            ["ManageStore"] = "Phatha Isitolo",
            ["AddNew"] = "Engeza Okutsha",
            ["Edit"] = "Hlela",
            ["Delete"] = "Susa",

            // Validation
            ["Required"] = "Leli gama liyadingeka",
            ["InvalidEmail"] = "Sicela ufake i-imeyili evumelekile",
            ["InvalidPhone"] = "Sicela ufake inombolo evumelekile",
            ["PasswordMismatch"] = "Amaphasiwedi awahambelani",
        }
    };
} 
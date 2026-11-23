using AppModels.Login;


namespace Web.Components.Pages.Login
{
    public partial class Login
    {

        private string Error = "";

        private string ImagePath { get; set; }
        private string NumberFormat { get; set; }



        public LoginModel UserLogin { get; set; } = new();

        protected override void OnInitialized()
        {
            ImagePath = ConfigService.GetValue<string>("AssetImagePath") ?? "";
            NumberFormat = Global.Format.Number;
        }

        public void OnLogin()
        {
            Error = "";

            //if (string.IsNullOrWhiteSpace(UserLogin?.UserID))
            //{
            //    Error = "Please enter your user ID.";
            //    return;
            //}

            //if (string.IsNullOrWhiteSpace(UserLogin?.Password))
            //{
            //    Error = "Please enter your password.";
            //    return;
            //}

            //DbService.IsConnected();


            if (DbService.IsDBExist)
            {
                DbService.CreateDataTables();
            }


            // Get the user profile based on the provided UserID and Password from DB
            // if found. Change User Authentication state to logged in and redirect to the home page.
        }


    }
}

using AppModels.Login;


namespace Web.Components.Pages.Login
{
    public partial class Login
    {

        private string Error = "";

        private string ImagePath { get; set; }
        private string NumberFormat { get; set; }
        bool IsDB_OK;



        public LoginModel UserLogin { get; set; } = new();

        protected override void OnInitialized()
        {
            ImagePath = Config.GetValue<string>("AssetImagePath") ?? "";
            NumberFormat = Global.Format.Number!;
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

            

            #region Creating / Patching Database

            if (DbService.IsDBExist)
            {
                
                IsDB_OK = DbService.CreateDataTables;
                IsDB_OK = DbService.UpdatePatches;
            }


            if(!IsDB_OK)
            {
                Error = "Database is not valid.... Critical Error found";
            }
            #endregion

        }


    }
}

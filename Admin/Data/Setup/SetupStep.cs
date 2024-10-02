namespace Admin.Data.Setup
{
    public enum SetupStep
    {
        // SetupSetps are in order of execution
        CreateAccount, // 1st
        RegisterBusiness, // 2nd
        Billing, // 3rd
        CreateService // 4th
    }
}

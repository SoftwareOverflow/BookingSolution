namespace Admin.Data.Setup
{
    public class SetupModel
    {
        private readonly Dictionary<SetupStep, bool> _steps = new () { 
            { SetupStep.CreateAccount, false },
            { SetupStep.RegisterBusiness, false },
            { SetupStep.Billing, false },
            { SetupStep.CreateService, false },
        };

        public SetupStep CurrentStep { get; private set; } = SetupStep.RegisterBusiness;

        public void SetStepComplete(SetupStep step)
        {
            _steps[step] = true;

            var list = _steps.Keys.Order().ToList();
            var index = list.IndexOf(step);
            if (index != list.Count - 1)
            {
                CurrentStep = list[index + 1];
            }
        }

        public Dictionary<SetupStep, bool> GetSteps() => _steps;

        public bool IsSetupComplete => true; // _steps.Values.All(x => x);
    }
}

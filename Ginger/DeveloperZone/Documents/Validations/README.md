# Creation fields validations
Can be used in Wizard pages or any object edit page

## Instead of showing the user a message of whats wrong, we show him a red note so he can fix
it saved click for the user, and show him exectly the field(s) which needs to be fixed
if several fields failed validations he will see them all

Adding validation to fields like:
- Field cannot be empty
- Grid must have rows 
- Agent/Env name must be unique



-- verify Env is unique
ProjEnvironment env = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where x.Name == mWizard.NewEnvironment.Name select x).SingleOrDefault();
                    if (env == null)
                    {
                        mWizard.EnvsFolder.AddRepositoryItem(mWizard.NewEnvironment);
                    }
                    else
                    {
                        // WizardEventArgs.AddError("Environment with the same name already exist");
                    }
                    


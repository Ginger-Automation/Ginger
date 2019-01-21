using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    public class PomRelearnWizard : WizardBase
    {
        public ApplicationPOMModel mDuplicatedPOM;
        public ApplicationPOMModel mOriginalPOM;


        public override string Title { get { return "POM Delta Check Wizard"; } }

        private Agent mAgent = null;

        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
            }
        }

        public bool IsLearningWasDone { get; set; }
        public IWindowExplorer IWindowExplorerDriver
        {
            get
            {
                if (Agent != null)
                    return ((IWindowExplorer)(Agent.Driver));
                else
                    return null;
            }
        }

        public PomRelearnWizard(ApplicationPOMModel POM, Agent agent)
        {
            mOriginalPOM = POM;
            mDuplicatedPOM = POM.CreateCopy(false) as ApplicationPOMModel;
            mDuplicatedPOM.ContainingFolder = POM.ContainingFolder;
            mDuplicatedPOM.ContainingFolderFullPath = POM.ContainingFolderFullPath;
            mAgent = agent;

            AddPage(Name: "Delta Status", Title: "Delta Status", SubTitle: "Get latest changes from page", Page: new POMDeltaWizardPage());
        }

        public override void Finish()
        {
            List<ElementInfo>  ElementsToUpdate = mDuplicatedPOM.CopiedUnienedList.Where(x => x.IsSelected == true).ToList();
            foreach (ElementInfo EI in ElementsToUpdate)
            {
                if (EI.DeltaStatus == ElementInfo.eDeltaStatus.Deleted)
                {
                    continue;
                }

                List<ElementLocator> LocatorrsToRemove = EI.Locators.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < LocatorrsToRemove.Count; i++)
                {
                    EI.Locators.Remove(LocatorrsToRemove[i]);
                }

                List<ControlProperty> PropertiesToRemove = EI.Properties.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < PropertiesToRemove.Count; i++)
                {
                    EI.Properties.Remove(PropertiesToRemove[i]);
                }


                foreach (ElementLocator EL in EI.Locators)
                {
                    if (EL.DeltaStatus == ElementInfo.eDeltaStatus.Modified)
                    {
                        EL.LocateValue = EL.UpdatedValue;
                    }
                }

                foreach (ControlProperty CP in EI.Properties)
                {
                    if (CP.DeltaStatus == ElementInfo.eDeltaStatus.Modified)
                    {
                        CP.Value = CP.UpdatedValue;
                    }
                }

                if (EI.ElementGroup == ElementInfo.eElementGroup.Mapped)
                {
                    mOriginalPOM.MappedUIElements.Remove(mOriginalPOM.MappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault());
                    mOriginalPOM.MappedUIElements.Add(EI);
                }
                else
                {
                    mOriginalPOM.UnMappedUIElements.Remove(mOriginalPOM.UnMappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault());
                    mOriginalPOM.UnMappedUIElements.Add(EI);
                }
            }

            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mOriginalPOM);
        }
    }
}

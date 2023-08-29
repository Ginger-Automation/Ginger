using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using System.Text;
using BPMNTask = Amdocs.Ginger.CoreNET.BPMN.Task;

BusinessFlow businessFlow;
BuildGingerEntities();
ExportActivityGroupBPMN(businessFlow.ActivitiesGroups[0]);

void BuildGingerEntities()
{
    businessFlow = new();
    businessFlow.Activities.Add(new Activity()
    {
        ActivityName = "Activity 1"
    });
    businessFlow.Activities.Add(new Activity()
    {
        ActivityName = "Activity 2"
    });
    businessFlow.ActivitiesGroups.Add(new ActivitiesGroup()
    {
        Name = "Activity Group"
    });
    businessFlow.ActivitiesGroups[0].AddActivityToGroup(businessFlow.Activities[0]);
    businessFlow.ActivitiesGroups[0].AddActivityToGroup(businessFlow.Activities[1]);
}

Collaboration ActivityGroupToCollaboration(ActivitiesGroup activityGroup)
{
    TargetApplication targetApplication = new()
    {
        AppName = "MyTargetApp"
    };
    IEnumerable<Activity> targetAppActivities = activityGroup.ActivitiesIdentifiers
                .Select(ident => ident.IdentifiedActivity)
                .Where(activity => string.Equals(activity.TargetApplication, targetApplication.Guid.ToString()));

    Collaboration collaboration = new(activityGroup.Guid, CollaborationType.SubProcess, activityGroup.Name);
    Participant participant = new(targetApplication.Guid, targetApplication.Name);

    participant.Process.AddStartEvent(name: "");
    IFlowSource previousOriginator = participant.Process.StartEvent!;
    foreach (Activity activity in targetAppActivities)
    {
        BPMNTask task = participant.Process.AddTask(new Process.AddTaskArguments(activity.Guid, activity.ActivityName));
        Flow.Create(name: "", source: previousOriginator, target: task);
        previousOriginator = task;
    }
    participant.Process.AddEndEvent(name: "");
    Flow.Create(name: "", source: previousOriginator, target: participant.Process.EndEvent);

    collaboration.AddParticipant(participant);

    return collaboration;
}

void ExportActivityGroupBPMN(ActivitiesGroup activityGroup)
{
    string xml = new BPMNXMLSerializer().Serialize(ActivityGroupToCollaboration(activityGroup));
    Console.WriteLine(xml);
}
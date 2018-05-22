using System.Collections.Generic;

namespace Library.Activities
{
    interface IActivitiesManager
    {
        IEnumerable<string> GetAvailableActivities();
        string PerformAction(int activityId);
    }
}
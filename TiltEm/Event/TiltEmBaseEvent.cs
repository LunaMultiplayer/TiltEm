using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TiltEm.Event
{
    public abstract class TiltEmBaseEvent
    {
        public static void Awake()
        {
            var lmpEventClasses = Assembly.GetExecutingAssembly().GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(TiltEmBaseEvent)));
            foreach (var lmpEventClass in lmpEventClasses)
            {
                var eventFields = lmpEventClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToArray();
                if (eventFields.Any())
                {
                    foreach (var eventField in eventFields)
                    {
                        var val = Activator.CreateInstance(eventField.FieldType, eventField.Name);
                        eventField.SetValue(null, val);
                    }
                }
            }
        }
    }
}

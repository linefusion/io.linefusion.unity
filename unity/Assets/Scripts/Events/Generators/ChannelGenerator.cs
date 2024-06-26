#if SOURCE_GENERATOR

{{~
func get_attr(type, name)
  for attr in type.Attributes
    if attr.FullName == name
      ret attr
    end
  end
  ret null
end 

func get_events()
  $events = []
  for struct in data.Structs
    attr = get_attr struct "MyProject.Events.EventAttribute"
    if attr == null
      continue
    end
    $events = Array.Add $events struct
  end
  ret $events | Array.Sort "FullName"
end

events = get_events
-}}

{{~ capture output ~}}
/// <auto-generated />

using System;
using UnityEngine;

{{~ for event in events ~}}
  {{~ if event.Namespace != null ~}}
namespace {{ event.Namespace }}
{
  {{~ end ~}}
    public class {{ event.Name }}Channel : Event<{{ event.Name }}>
    {
        {{~ if event.Constructors == empty ~}}
        public void Fire()
        {
            this.Dispatch(new {{ event.FullName }}());
        }
        
        public void FireAt(UnityEngine.Vector3 position)
        {
            this.Dispatch(new {{ event.FullName }}(), position);
        }
        {{~ else ~}}
          {{~ for constructor in event.Constructors ~}}
        public void Fire({{- for param in constructor.Parameters -}}
          {{ param.Type.FullName }} {{ param.Name }}
          {{- if !for.last }}, {{ end -}}
          {{~ end -}})
        {
            this.Dispatch(new {{ event.FullName }}({{- for param in constructor.Parameters -}}
              {{ param.Name }}
              {{- if !for.last }}, {{ end -}}
              {{~ end -}}));
        }
        
        public void FireAt({{- for param in constructor.Parameters -}}
          {{ param.Type.FullName }} {{ param.Name }}
          {{- if !for.last }}, {{ end -}}
          {{~ end -}}{{- if constructor.Parameters != empty }}, {{ end -}}UnityEngine.Vector3 __position)
        {
            this.Dispatch(new {{ event.FullName }}({{- for param in constructor.Parameters -}}
              {{ param.Name }}
              {{- if !for.last }}, {{ end -}}
              {{~ end -}}), __position);
        }

          {{~ end ~}}
        {{~ end ~}}
    }
  {{~ if event.Namespace != null ~}}
}
  {{~ end ~}}
{{~ end ~}}

namespace MyProject.Events
{
    public static partial class Channel
    {
{{~ for event in events ~}}
        public static {{ event.FullName }}Channel {{ event.Name }} { get; private set; }
{{~ end ~}}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
{{~ for event in events ~}}
            {{ event.Name }} = new {{ event.FullName }}Channel();
{{~ end ~}}
        }
    }
}

{{~ end ~}}

{{~ Save(output, "../Channels/Channel.cs") ~}}

#endif

namespace MyProject.Events
{
    public static partial class Channel
    {
    }
}

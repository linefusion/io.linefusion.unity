
#if INSIDE_GENERATOR

{{ for struct in assembly.structs }}
  {{ for constructor in struct.constructors }}
    // {{ constructor }}
  {{ end }}
{{ end }}

#endif

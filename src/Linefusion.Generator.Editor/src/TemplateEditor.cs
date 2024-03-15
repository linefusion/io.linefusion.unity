namespace Linefusion.Generators.Editor
{
    using Linefusion.Generator;
    using Linefusion.Generators;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(Template))]
    public class TemplateEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            //InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var type = serializedObject.FindProperty("Type");
            var isGenerator = ((TemplateType)type.intValue).HasFlag(TemplateType.Generator);
            var isInclude = ((TemplateType)type.intValue).HasFlag(TemplateType.Include);

            root.Add(
                new PropertyField(serializedObject.FindProperty("Contents")) { label = "Contents" }
            );

            if (isGenerator && !isInclude)
            {
                var run = new Button() { text = "Generate", style = { height = 30 }, };
                run.clicked += () =>
                {
                    var template = this.target as Template;
                    TemplateGenerator.Generate(template, AssetDatabase.GetAssetPath(target));
                };
                root.Add(run);
            }

            if (isInclude)
            {
                var trigger = new Button() { text = "Trigger", style = { height = 30 }, };
                trigger.clicked += () =>
                {
                    TemplateGenerator.Generate();
                };
                root.Add(trigger);
            }

            return root;
        }
    }
}

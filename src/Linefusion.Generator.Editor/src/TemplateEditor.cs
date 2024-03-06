
namespace Linefusion.Generators.Editor
{
    

    using Linefusion.Generators;

    using UnityEngine;
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

            root.Add(new PropertyField(serializedObject.FindProperty("Contents")) { label = "Contents" });
            
            var run = new Button() 
            {
                text = "Run Generator",
                style = { height = 30 },
            };
            
            run.clicked += () => 
            {
                var template = this.target as Template;
                TemplateGenerator.Generate(template, AssetDatabase.GetAssetPath(target));
            };

            root.Add(run);


            return root;
        }
    }
}
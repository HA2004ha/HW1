using UnityEditor;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// Assigns private [SerializeField] fields on freshly added components. The scene builder
    /// creates objects at runtime, so there is no inspector to drag references into.
    /// </summary>
    internal sealed class Wire
    {
        private readonly SerializedObject serialized;

        private Wire(Component target)
        {
            serialized = new SerializedObject(target);
        }

        public static Wire On(Component target) => new Wire(target);

        public Wire Set(string field, object value)
        {
            var property = serialized.FindProperty(field);
            Debug.Assert(property != null, $"Missing serialized field '{field}' on {serialized.targetObject.name}");
            if (property == null || value == null) return this;

            switch (value)
            {
                case Object o: property.objectReferenceValue = o; break;
                case bool b: property.boolValue = b; break;
                case int i: property.intValue = i; break;
                case float f: property.floatValue = f; break;
                case string s: property.stringValue = s; break;
                case Vector2 v2: property.vector2Value = v2; break;
                case Vector3 v3: property.vector3Value = v3; break;
                case Color col: property.colorValue = col; break;
                case System.Enum e: property.enumValueIndex = System.Convert.ToInt32(e); break;
            }

            return this;
        }

        public Wire Refs(string field, params Object[] values)
        {
            var property = serialized.FindProperty(field);
            if (property == null) return this;

            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            return this;
        }

        public Wire Floats(string field, params float[] values)
        {
            var property = serialized.FindProperty(field);
            if (property == null) return this;

            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).floatValue = values[i];
            }

            return this;
        }

        public Wire Apply()
        {
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(serialized.targetObject);
            return this;
        }
    }
}

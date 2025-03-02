using System.Collections.Generic;
using UnityEngine;

public class LsystemScript : MonoBehaviour
{
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject stairPrefab;
    [SerializeField] private GameObject intersectionPrefab;
    [SerializeField] private float segmentLength = 10f;
    [SerializeField] private float stairHeight = 3f;
    [SerializeField] private float intersectionOffset = 5f;
    [SerializeField] private int maxDepth = 2;

    private const string axiom = "F";
    private Dictionary<char, string> rules;
    private string currentString;

    void Start()
    {
        rules = new Dictionary<char, string>
        {
            /* 
             'F' = couloirs 
             'I' = intersections (ou entrées)
             'S' = escaliers
             'P' = platforme
            */
            { 'F', "FIF" },
            { 'I', "[S]F" }             
        };

        currentString = axiom; 
        GenerateLSystem(maxDepth);
        BuildStation();
    }

    void GenerateLSystem(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            string nextString = "";
            foreach (char c in currentString)
            {
                nextString += rules.ContainsKey(c) ? rules[c] : c.ToString();
            }
            currentString = nextString;
        }
    }

    void BuildStation()
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();

        foreach (char c in currentString)
        {
            switch (c)
            {
                case 'F':
                    CreateSegment(corridorPrefab);
                    break;
                case 'I':
                    transformStack.Push(new TransformInfo(transform.position, transform.rotation));
                    CreateIntersection();
                    transformStack.Pop();
                    break;
                case 'P':
                    CreateSegment(platformPrefab);
                    break;
                case 'S':
                    CreateStair();
                    break;
                case '[':
                    transformStack.Push(new TransformInfo(transform.position, transform.rotation));
                    transform.Rotate(Vector3.up, -90);
                    break;
                case ']':
                    if (transformStack.Count > 0)
                    {
                        TransformInfo ti = transformStack.Pop();
                        transform.position = ti.position;
                        transform.rotation = ti.rotation;
                        transform.Rotate(Vector3.up, 90);
                    }
                    break;
            }
        }
    }

    void CreateSegment(GameObject prefab)
    {
        Vector3 startPosition = transform.position;
        Instantiate(prefab, startPosition, transform.rotation);
        transform.Translate(Vector3.forward * segmentLength);
    }

    void CreateStair()
    {
        Vector3 startPosition = transform.position;
        Instantiate(stairPrefab, startPosition, transform.rotation);


        // Créer la plateforme sous l'escalier
        Vector3 platformPosition = startPosition;
        platformPosition.y -= stairHeight; // Descendre au sol
        platformPosition += transform.forward * (segmentLength / 150); // Centrer sous l'escalier
        platformPosition += transform.right * (segmentLength * 7); // Avancer la plateforme
        Quaternion platformRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 90, 0); // Rotation de la plateforme 90°
        Instantiate(platformPrefab, platformPosition, platformRotation);

        // Monter et avancer
        transform.Translate(Vector3.up * stairHeight);
        transform.Translate(Vector3.forward * (segmentLength / 2));
    }


    void CreateIntersection()
    {
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        Instantiate(intersectionPrefab, originalPosition, originalRotation);

        transform.Translate(Vector3.forward * intersectionOffset);
    }



    private class TransformInfo
    {
        public Vector3 position;
        public Quaternion rotation;

        public TransformInfo(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }
}
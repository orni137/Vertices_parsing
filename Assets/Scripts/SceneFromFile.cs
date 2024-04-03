using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using UnityEngine.UIElements;

public class SceneFromFile : MonoBehaviour {

    public string fileName = "SceneData06";
    const int nOfTriLists = 8;
    List<GameObject> geomObjList = new List<GameObject>();
    List<GameObject> pointList = new List<GameObject>();
    List<Transform>[] linesList = new List<Transform>[nOfTriLists];
    List<Vector3>[] trianglesListArray = new List<Vector3>[nOfTriLists];
    public Material[] materials = new Material[nOfTriLists];
    public Material pointMaterial; 
    public GameObject pointObj;
    public Transform pointCloud, edgeMesh;
    public float sceneScale = 3.0f;

    bool firstPoint = true;
    Vector3 baseVect = Vector3.zero;
    void checkBase (ref Vector3 vect)
    {
        if (firstPoint)
        {
            firstPoint = false;
            baseVect = vect;
        } 
        vect -= baseVect;
    }

    void makeObject(int n) {
        GameObject geomObj = new GameObject("GeomObj" + geomObjList.Count.ToString());
        geomObj.transform.localRotation = Quaternion.identity;
        geomObj.AddComponent<MeshFilter>();

        Mesh layerMesh = new Mesh();
        Vector3[] newVertices = new Vector3[trianglesListArray[n].Count];
        int[] newTriangles = new int[trianglesListArray[n].Count];

        for (int i = 0; i < trianglesListArray[n].Count; i++) {
            newVertices[i] = trianglesListArray[n][i];
            newTriangles[i] = i;
        }

        trianglesListArray[n].Clear();

        layerMesh.vertices = newVertices;
        layerMesh.triangles = newTriangles;

        layerMesh.RecalculateNormals();
        layerMesh.RecalculateBounds();

        geomObj.GetComponent<MeshFilter>().mesh = layerMesh;
        geomObj.AddComponent<MeshRenderer>();
        geomObj.GetComponent<MeshRenderer>().material = materials[n];

        foreach(Transform line in linesList[n]) {
            line.transform.SetParent(geomObj.transform);
        }
        linesList[n].Clear();

        geomObjList.Add(geomObj);
    }

    void startPointCloud() {
        GameObject go = new GameObject("Point Cloud ");
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        pointCloud = go.transform;
    }

    int parseInt(string str) {
        IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-GB");
        return int.Parse(str, provider);
    }
    void addTriangle(int n) {
        IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-GB");

        for (int j = 1; j <= 3; j++) {
            Vector3 vert = new Vector3();
            string[] substr_loc = substr[j].Split('(', ';', ')');
            vert.x = float.Parse(substr_loc[1], provider);
            vert.y = float.Parse(substr_loc[2], provider);
            vert.z = float.Parse(substr_loc[3], provider);

            vert *= sceneScale;
            checkBase(ref vert);

            trianglesListArray[n].Add(vert);
        }
    }
    void addLine(int n) {
        Transform line = Instantiate(edgeMesh, new Vector3(0, 0, 0), Quaternion.identity);

        IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-GB");
        Vector3 vert1 = new Vector3();
        string[] substr_loc = substr[1].Split('(', ';', ')');
        vert1.x = float.Parse(substr_loc[1], provider);
        vert1.y = float.Parse(substr_loc[2], provider);
        vert1.z = float.Parse(substr_loc[3], provider);
        vert1 *= sceneScale;
        checkBase(ref vert1);

        Vector3 vert2 = new Vector3();
        substr_loc = substr[2].Split('(', ';', ')');
        vert2.x = float.Parse(substr_loc[1], provider);
        vert2.y = float.Parse(substr_loc[2], provider);
        vert2.z = float.Parse(substr_loc[3], provider);
        vert2 *= sceneScale;
        checkBase(ref vert2);

        Vector3 axis = vert2 - vert1;
        line.position = vert1 + 0.5f * axis;
        line.rotation = Quaternion.LookRotation(axis) * Quaternion.Euler(90, 0, 0);
        line.localScale = new Vector3(0.4f, 0.5f * axis.magnitude, 0.4f);
        line.name = "Line";
        line.GetComponent<MeshRenderer>().material = materials[n];

        linesList[n].Add(line);
    }

    void createPoint() {
        IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-GB");

        Vector3 vert = new Vector3();
        string[] substr_loc = substr[1].Split('(', ';', ')');
        vert.x = float.Parse(substr_loc[1], provider);
        vert.y = float.Parse(substr_loc[2], provider);
        vert.z = float.Parse(substr_loc[3], provider);

        vert *= sceneScale;
        checkBase(ref vert);

        float scale = float.Parse(substr[2], provider) * sceneScale * 25;

        GameObject point = Instantiate(pointObj, vert, Quaternion.identity, pointCloud);
        point.transform.localScale = new Vector3(scale, scale, scale);
        point.GetComponent<MeshRenderer>().material = pointMaterial;
        // point.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        pointList.Add(point);
    }

    string[] substr;

    void readStrings() {
        TextAsset textFile = Resources.Load<TextAsset>(fileName);
        string wholeFile = textFile.text;
        string[] fLines = wholeFile.Split('\r', '\n');
        for (int i = 0; i < fLines.Length; i++) {
            if (fLines[i].Length >= 27) {
                fLines[i] = fLines[i].Remove(0, 27);
            }
            if (fLines[i].Length == 0) continue;
            substr = fLines[i].Split('\t');

            if (substr[0] == "3D_object") {
                trianglesListArray[parseInt(substr[1])].Clear();
            }
            if (substr[0] == "point_cloud") {
                startPointCloud();
            }
            if (substr[0] == "triangle") {
                addTriangle(parseInt(substr[4]));
            }
            if (substr[0] == "line") {
                addLine(parseInt(substr[3]));
            }
            if (substr[0] == "point") {
                createPoint();
            }
            if (substr[0] == "end") {
                makeObject(parseInt(substr[1]));
            }
        }
    }


    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < nOfTriLists; i++) {
            trianglesListArray[i] = new List<Vector3>();
            linesList[i] = new List<Transform>();
        }
        readStrings();
    }

    // Update is called once per frame
    void Update() {

    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour {

    public Transform allPlayers;
    float moveSpeed;

    byte[,,] data;
    Vector3 dims;

    Transform closestPlayer;

    public Transform footPre;
    Transform[,,] feet;
    public Transform body;

    bool isHit;
    Vector3 hitLoc;
    Vector3 hitVec;

    int totalHealth;
    int currHealth;

    bool isDead;

	// Use this for initialization
	void Start () {
        moveSpeed = 1f;
        moveSpeed *= Random.Range(0.1f, 1f);

        dims = new Vector3(4, 4, 4);
        data = new byte[(int)dims.x, (int)dims.y, (int)dims.z];
        feet = new Transform[(int)(dims.x), (int)(dims.y), (int)(dims.z)];
        transform.localScale = new Vector3(1f / (dims.x), 1f / (dims.y), 1f / (dims.z));

        totalHealth = 0;
        Regenerate(data);
        meshCubes(data);
        gameObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().isTrigger = true;

        body.GetComponent<BoxCollider>().size = new Vector3(dims.x - 2, dims.y - 2, dims.z - 2);
    }

    void Regenerate(byte[,,] data)
    {
        for (int x = 0; x < data.GetLength(0); x++)
        {
            for (int y = 1; y < data.GetLength(1); y++)
            {
                for (int z = 0; z < data.GetLength(2) - 1; z++)
                {
                    data[x, y, z] = (byte)1;
                    totalHealth++;
                }
            }
        }

        float pegSize = 0.25f;
        Vector3 or = new Vector3(-dims.x * 0.5f, -dims.y * 0.5f, -dims.z * 0.5f);

        /*
        for (int z = 1; z < data.GetLength(2); z++)
        {
            data[0, (int)(dims.y * 0.5f), z] = (byte)1;
        }
        for (int z = 1; z < data.GetLength(2); z++)
        {
            data[(int)(dims.x - 1), (int)(dims.y * 0.5f), z] = (byte)1;
        }
        */

        totalHealth += 2;
        data[0, (int)(dims.y * 0.5f), (int)(dims.z - 1)] = (byte)1;
        Transform foot = Instantiate<Transform>(footPre);
        foot.parent = transform;
        foot.localScale = new Vector3(pegSize, pegSize, 1f);
        foot.localPosition = new Vector3((or.x + 0.5f), (or.y + dims.y * 0.5f + 0.5f), (or.z + dims.z - 0.5f));
        feet[0, (int)(dims.y * 0.5f), (int)(dims.z - 1)] = foot;


        data[(int)(dims.x - 1), (int)(dims.y * 0.5f), (int)(dims.z - 1)] = (byte)1;
        foot = Instantiate<Transform>(footPre);
        foot.parent = transform;
        foot.localScale = new Vector3(pegSize, pegSize, 1f);
        foot.localPosition = new Vector3((or.x + dims.x - 0.5f), (or.y + dims.y * 0.5f + 0.5f), (or.z + dims.z - 0.5f));
        feet[(int)(dims.x - 1), (int)(dims.y * 0.5f), (int)(dims.z - 1)] = foot;

        // feet
        for (int z = 0; z < data.GetLength(2) - 1; z++)
        {
            data[0, 0, z] = (byte)1;
            totalHealth++;

            foot = Instantiate<Transform>(footPre);
            foot.parent = transform;
            foot.localScale = new Vector3(pegSize, 1f, pegSize);
            foot.localPosition = new Vector3((or.x + 0.5f), (or.y + 0.5f), (or.z + 0.5f + z));
            feet[0, 0, z] = foot;
            
        }
        for (int z = 0; z < data.GetLength(2) - 1; z++)
        {
            data[(int)(dims.x - 1), 0, z] = (byte)1;
            totalHealth++;

            foot = Instantiate<Transform>(footPre);
            foot.parent = transform;
            foot.localScale = new Vector3(pegSize, 1f, pegSize);
            foot.localPosition = new Vector3((or.x + dims.x - 0.5f), (or.y + 0.5f), (or.z + 0.5f + z));
            feet[(int)(dims.x - 1), 0, z] = foot;
            foot.parent = transform;
        }

        currHealth = totalHealth;
        isDead = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (isDead && transform.position.y < -1f)
        {
            ZombieSpawner.currCount--;
            DestroyImmediate(gameObject);
        }
	}

    void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        allPlayers = GameObject.Find("All Players").transform;
        closestPlayer = allPlayers;
        float closestDist = 10000000f;
        for (int i = 0; i < allPlayers.childCount; i++)
        {
            float dist = Vector3.Distance(transform.position, allPlayers.GetChild(i).position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPlayer = allPlayers.GetChild(i);
            }
        }

        Rigidbody body = GetComponent<Rigidbody>();

        if (isHit)
        {
            float bulletForce = 0.5f;
            GetComponent<Rigidbody>().AddForceAtPosition(hitVec * bulletForce, hitLoc, ForceMode.Impulse);
            isHit = false;
        }

        Vector3 towards = (closestPlayer.position - transform.position).normalized;
        Vector3 towardsPlane = new Vector3(towards.x, 0f, towards.z);
        Vector3 towardsPlaneLocal = transform.InverseTransformDirection(towardsPlane);
        Vector3 upLocal = transform.InverseTransformDirection(Vector3.up);
        Vector3 forw = new Vector3(transform.forward.x, 0f, transform.forward.z);
        float inter = 0.05f;
        Quaternion rot = Quaternion.FromToRotation(forw, (towardsPlane* inter + forw*(1f - inter))*2f);
        //rot = Quaternion.LookRotation(towardsPlane)
        body.MoveRotation(rot * transform.rotation);
        //body.MoveRotation(Quaternion.LookRotation(towards, Vector3.up));
        body.MovePosition(transform.position + towardsPlane * moveSpeed * Time.deltaTime);

        float deadRatio = 0.7f;
        if (currHealth <= totalHealth * deadRatio && !isDead)
        {
            Kill();
        }
    }

    public void Kill()
    {
        //Debug.Log("dead");
        isDead = true;

        for (int x = 0; x < data.GetLength(0); x++)
        {
           for (int y = 0; y < data.GetLength(1); y++)
            {
                for (int z = 0; z < data.GetLength(2); z++)
                {
                    if (feet[x, y, z] != null)
                    {
                        GameObject.DestroyImmediate(feet[x, y, z].gameObject);
                        feet[x, y, z] = null;
                    }
                }
            }
        }

        GameObject.DestroyImmediate(body.gameObject);
    }

    public Vector3 Hit(Vector3 worldPoint, Vector3 surfNorm, float dist, Vector3 worldDir, bool modify)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector3 localDir = transform.InverseTransformVector(worldDir).normalized;
        //Debug.Log(localPoint);

        Vector3 or = new Vector3(-dims.x * 0.5f + 0.01f, -dims.y * 0.5f + 0.01f, -dims.z * 0.5f + 0.01f);
        int x = (int)(localPoint.x - or.x);
        int y = (int)(localPoint.y - or.y);
        int z = (int)(localPoint.z - or.z);

        float step = 0.05f;
        bool hit = false;
        while (!hit && x >= 0 && x < dims.x && y >= 0 && y < dims.y && z >= 0 && z < dims.z)
        {
            if (data[x, y, z] == 1)
            {
                hit = true;
                if (modify)
                {
                    data[x, y, z] = (byte)0;
                    currHealth--;

                    if (feet[x, y, z] != null)
                    {
                        GameObject.DestroyImmediate(feet[x, y, z].gameObject);
                        feet[x, y, z] = null;
                    }

                    meshCubes(data);
                }
            }

            localPoint += localDir * step;
            x = (int)(localPoint.x - or.x);
            y = (int)(localPoint.y - or.y);
            z = (int)(localPoint.z - or.z);
        }

        Vector3 worldHit = Vector3.zero;
        if (hit)
        {
            worldHit = transform.TransformPoint(localPoint);
            if (modify)
            {
                hitLoc = worldHit;
                hitVec = worldDir;
                isHit = true;
            }
        }
        return worldHit;
    }

    public void meshCubes(byte[,,] data)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        int orx = (int)(-dims.x * 0.5);
        int ory = (int)(-dims.y * 0.5);
        int orz = (int)(-dims.z * 0.5);

        int size = data.GetLength(0);
        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int front = z + 1;
                    int back = z - 1;
                    int left = x - 1;
                    int right = x + 1;
                    int up = y + 1;
                    int down = y - 1;

                    if (data[x, y, z] != 0 && (back < 0 || data[x, y, back] == 0)) // back
                    {
                        appendFace(verts, uvs, tris, new Vector3(orx + x, ory + y, orz + z), Vector3.right, Vector3.up, false);
                    }
                    if (data[x, y, z] != 0 && (front >= size || data[x, y, front] == 0)) // front
                    {
                        appendFace(verts, uvs, tris, new Vector3(orx + x, ory + y, orz + 1 + z), Vector3.right, Vector3.up, true);
                    }
                    if (data[x, y, z] != 0 && (left < 0 || data[left, y, z] == 0)) // left
                    {
                        appendFace(verts, uvs, tris, new Vector3(orx + x, ory + y, orz + 1 + z), Vector3.back, Vector3.up, false);
                    }
                    if (data[x, y, z] != 0 && (right >= size || data[right, y, z] == 0)) // right
                    {
                        appendFace(verts, uvs, tris, new Vector3(orx + 1 + x, ory + y, orz + 1 + z), Vector3.back, Vector3.up, true);
                    }
                    if (data[x, y, z] != 0 && (down < 0 || data[x, down, z] == 0)) // bottom
                    {
                        appendFace(verts, uvs, tris, new Vector3(orx + x, ory + y, orz + 1 + z), Vector3.right, Vector3.back, false);
                    }
                    if (data[x, y, z] != 0 && (up >= size || data[x, up, z] == 0)) // top
                    {
                        appendFace(verts, uvs, tris, new Vector3(orx + x, ory + 1 + y, orz + 1 + z), Vector3.right, Vector3.back, true);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void appendFace(List<Vector3> verts, List<Vector2> uvs, List<int> tris, Vector3 orig, Vector3 right, Vector3 up, bool flipped)
    {
        int startIndex = verts.Count;
        int i0 = startIndex;
        int i1 = startIndex + 1;
        int i2 = startIndex + 2;
        int i3 = startIndex + 3;

        verts.Add(orig);
        verts.Add(orig + up);
        verts.Add(orig + up + right);
        verts.Add(orig + right);

        tris.Add(i0);
        tris.Add(i1);
        tris.Add(i2);
        tris.Add(i0);
        tris.Add(i2);
        tris.Add(i3);

        if (flipped)
        {
            tris[tris.Count - 1] = i1;
            tris[tris.Count - 5] = i3;
        }

        uvs.Add(Vector3.zero);
        uvs.Add(Vector3.zero);
        uvs.Add(Vector3.zero);
        uvs.Add(Vector3.zero);
    }
}

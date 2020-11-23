using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Transform pistol;
    public Transform rifle;
    Transform[] weapons;
    int currWeapon;

    private Vector3 shoulder;

    public LineRenderer laser;

    bool feetOnGround;

    public LineRenderer bullet;
    bool isFiring;
    bool triggerDown;

    bool isRunning;

    bool shot;

    public float blowBack;

    // Use this for initialization
    void Start() {
        shoulder = new Vector3(0.5f, 0f, 0f);
        feetOnGround = false;

        weapons = new Transform[2] { pistol, rifle };
        currWeapon = 0;

        isFiring = false;
        triggerDown = false;

        isRunning = false;

        shot = false;
    }

    // Update is called once per frame
    void Update() {
        float rotSpeed = 300f;
        float pivSpeed = 0f;
        float moveSpeed = 3f;

        if (Input.GetButton("Run"))
        {
            weapons[currWeapon].gameObject.SetActive(false);
            isRunning = true;
        }
        else
        {
            weapons[currWeapon].gameObject.SetActive(true);
            isRunning = false;
        }

        if (Input.GetButtonDown("Switch"))
        {
            weapons[currWeapon].gameObject.SetActive(false);
            currWeapon = (currWeapon + 1) % weapons.Length;
            weapons[currWeapon].gameObject.SetActive(true);
        }

        laser.gameObject.SetActive(false);
        float aimTrigger = Input.GetAxisRaw("Fire2");
        if (aimTrigger > 0.5f && !isRunning)
        {
            laser.gameObject.SetActive(true);
            laser.SetVertexCount(2);

            RaycastHit hit;
            float laserDist = 1000000f;
            if (Physics.Raycast(weapons[currWeapon].position, weapons[currWeapon].forward, out hit))
            {
                Zombie zombie = null;
                if ((zombie = hit.transform.GetComponent<Zombie>()) != null)
                {
                    Vector3 worldHit = zombie.Hit(hit.point, hit.normal, hit.distance, weapons[currWeapon].forward, false);
                    if (worldHit != Vector3.zero)
                    {
                        laserDist = Vector3.Distance(weapons[currWeapon].position, worldHit);
                    }
                }
            }

            laser.SetPosition(0, weapons[currWeapon].position);
            laser.SetPosition(1, weapons[currWeapon].position + laserDist * weapons[currWeapon].forward);
            laser.SetWidth(0.007f, 0.007f);

            //Debug.DrawLine(weapons[currWeapon].position, weapons[currWeapon].position + laserDist * weapons[currWeapon].forward, Color.red, 0.1f);

            rotSpeed *= 0.15f;
            pivSpeed = 20f;
            moveSpeed *= 0.5f;
        }
        else
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].localRotation = Quaternion.identity;
                weapons[i].localPosition = new Vector3(0.5f, 0f, 0.75f);
            }
        }

        Rigidbody body = GetComponent<Rigidbody>();

        Vector3 rotate = new Vector3(0f, Input.GetAxisRaw("RS Horizontal"), 0f);
        rotate *= Time.deltaTime * rotSpeed;
        body.MoveRotation(Quaternion.Euler(rotate) * transform.rotation);

        Vector3 pivot = new Vector3(Input.GetAxisRaw("RS Vertical"), 0f, 0f);
        pivot *= Time.deltaTime * pivSpeed;
        //pivot.x *= 0f;

        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        float rotStep = 50f;
        rotStep *= Time.deltaTime;
        if (isRunning)
        {
            moveSpeed *= 2.5f;
            //transform.rotation = transform.rotation * Quaternion.FromToRotation(transform.forward, move.normalized);
            if (move.sqrMagnitude > 0.1f)
            {
                body.MoveRotation(Quaternion.LookRotation(move.normalized, Vector3.up));
            }
            rotate = Vector3.zero;
            pivot = Vector3.zero;
        }

        
        //move = move.normalized;
        move *= Time.deltaTime;
        body.MovePosition(transform.position + move * moveSpeed);

        float currAngle = (pistol.localEulerAngles.x + 180) % 360f - 180f;
        float newAngle = (pistol.localEulerAngles.x + pivot.x + 180f) % 360f - 180f;
        float maxPivot = 40f;
        if (newAngle > maxPivot)
        {
            pivot.x = maxPivot - currAngle;
        }
        else if (newAngle < -maxPivot)
        {
            pivot.x = -maxPivot - currAngle;
        }

        pistol.RotateAround(transform.position, transform.right, pivot.x);
        rifle.RotateAround(transform.position, transform.right, pivot.x);

        float jumpForce = 6f;
        if (Input.GetButtonDown("Jump") && feetOnGround)
        {
            body.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        float fireTrigger = Input.GetAxisRaw("Fire1");
        float bulletSpeed = 2.5f;
        float bulletLength = bulletSpeed * Random.Range(0.5f, 1f);
        if (isFiring)
        {
            // bullet physics

            RaycastHit hit;
            bool bulletContinue = true;
            if (Physics.Raycast(bullet.transform.position, bullet.transform.forward, out hit, bulletSpeed)) 
            {
                bulletContinue = false;

                Zombie zombie = null;
                if ((zombie = hit.transform.GetComponent<Zombie>()) != null)
                {
                    Vector3 worldHit = zombie.Hit(hit.point, hit.normal, hit.distance, weapons[currWeapon].forward, true);
                    if (worldHit == Vector3.zero)
                    {
                        bulletContinue = true;
                    }
                }
            }
            
            if (bulletContinue)
            {
                bullet.transform.position += bullet.transform.forward * bulletSpeed;
                bullet.SetPosition(1, bullet.transform.position);
                bullet.SetPosition(0, bullet.transform.position - bulletLength * bullet.transform.forward);
                bullet.SetWidth(0.0f, 0.075f);
                bullet.SetColors(Color.white, Color.black * 0.75f);
            }
            else
            {
                bullet.gameObject.SetActive(false);
                isFiring = false;
            }
        }

        if (fireTrigger > 0.5f && !isRunning && (!triggerDown || currWeapon == 1))
        {
            bullet.gameObject.SetActive(true);
            bullet.transform.position = weapons[currWeapon].position;
            bullet.transform.rotation = weapons[currWeapon].rotation;

            // a little randomness (not recoil)
            float dev = 0f;
            float xdev = dev * Random.Range(-0.5f, 0.5f);
            float ydev = dev * Random.Range(-0.5f, 0.5f);
            Vector3 devVec = bullet.transform.forward + bullet.transform.up * ydev + bullet.transform.right * xdev;
            bullet.transform.rotation = Quaternion.LookRotation(devVec);

            bullet.SetVertexCount(2);
            bullet.SetWidth(0.0f, 0.5f);
            bullet.SetColors(Color.white, Color.white);

            bullet.SetPosition(1, bullet.transform.position + weapons[currWeapon].localScale.z * 0.5f * bullet.transform.forward);
            bullet.SetPosition(0, bullet.transform.position + (weapons[currWeapon].localScale.z * 0.5f + 0.2f) * bullet.transform.forward);
            isFiring = true;
            triggerDown = true;

            shot = true;
        }
        else if (fireTrigger < 0.5f)
        {
            triggerDown = false;
        }
    }

    void FixedUpdate()
    {
        feetOnGround = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        if (shot)
        {
            GetComponent<Rigidbody>().AddForceAtPosition(-transform.forward * blowBack, transform.position + new Vector3(0f, -0.5f, 0f), ForceMode.Impulse);
            shot = false;
        }
    }

    
}

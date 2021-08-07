using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using shared;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    [SerializeField]
    Material matGreen;
    [SerializeField]
    Material matYellow;
    [SerializeField]
    Material matRed;

    public bool Finished { get; private set; } = false;
    public bool toBeStored { get; private set; } = false;

    public Transform storageTransforms { set; private get; }


    private int minIt = 14;
    private int currentIt = 0;

    private Quaternion origin;
    private Quaternion target;
    private float alpha = 1;

    private Vector3 originPos;
    private Vector3 targetPos;
    private float alphaPos = 0;

    
    private DiceInfo.State stopOn = DiceInfo.State.UNROLLED;


    private int currentRot = 5;

    //the six positions the dice can be at, orderd so we can roll trough them
    private Vector3[] rotations = new Vector3[6] {
    new Vector3(0,180,-90),
    new Vector3(90,90,180),
    new Vector3(0,90,180),
    new Vector3(0,90,90),
    new Vector3(-90,180,0),
    new Vector3(0,180,0),
    };

    private DiceInfo.State[] faceUp = null;


    //what face is up in each rotation depanding on the color of the dice
    private DiceInfo.State[] faceUpGreen = new DiceInfo.State[6] {
    DiceInfo.State.SHOT,
    DiceInfo.State.FOOT,
    DiceInfo.State.BRAIN,
    DiceInfo.State.BRAIN,
    DiceInfo.State.FOOT,
    DiceInfo.State.BRAIN,
    };

    private DiceInfo.State[] faceUpYellow = new DiceInfo.State[6] {
    DiceInfo.State.SHOT,
    DiceInfo.State.FOOT,
    DiceInfo.State.BRAIN,
    DiceInfo.State.SHOT,
    DiceInfo.State.FOOT,
    DiceInfo.State.BRAIN,
    };

    private DiceInfo.State[] faceUpRed = new DiceInfo.State[6] {
    DiceInfo.State.SHOT,
    DiceInfo.State.FOOT,
    DiceInfo.State.SHOT,
    DiceInfo.State.SHOT,
    DiceInfo.State.FOOT,
    DiceInfo.State.BRAIN,
    };

    private void Start()
    {
        originPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        targetPos = new Vector3(transform.position.x,transform.position.y+6f,transform.position.z);
    }

    //sets the color of dice and the right array of states for that color
    public void SetColor(DiceInfo.Color c) 
    {
        if (c == DiceInfo.Color.GREEN)
        {
            GetComponent<MeshRenderer>().material = matGreen;
            faceUp = faceUpGreen;
        }
        else if (c == DiceInfo.Color.YELLOW)
        {
            GetComponent<MeshRenderer>().material = matYellow;
            faceUp = faceUpYellow;
        }
        else if (c == DiceInfo.Color.RED)
        {
            GetComponent<MeshRenderer>().material = matRed;
            faceUp = faceUpRed;
        }

        StartCoroutine(Roll());
    }
    public void DisableCollision()
    {
        GetComponent<BoxCollider>().enabled = false;
    }
    public void EnableCollision()
    {
        GetComponent<BoxCollider>().enabled = true;
    }
    public void EnableGravity()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
    }

    public void DisableGravity()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    //sets what the dice roll result should be
    public void SetStopState(DiceInfo.State st)
    {
        stopOn = st;
    }


    public DiceInfo.State getStopState()
    {
        return stopOn;
    }

    //when a dice hits the ground store it sets itself in list to be stored, so the game state can the store them in the right order
    private void OnCollisionEnter(Collision collision)
    {
        DisableGravity();
        DisableCollision();

        if (stopOn == DiceInfo.State.FOOT)
        {
            Finished = true;
            return;
        }
        else
            toBeStored = true;
    }

    //start movement to storage position
    public void StartStoring() { 
        transform.rotation = target;

        foreach (Transform t in storageTransforms)
            if (t.childCount == 0)
            {
                transform.parent = t;
                originPos = transform.localPosition;
                targetPos = new Vector3(0,0,0);
                alphaPos = 0;
            }
        StartCoroutine(MoveToStorage());
    }

    //rolling corutine, follows the rotations until it finds the right position (after a minimum of steps)
    public IEnumerator Roll()
    {
        while (true)
        {
            if (stopOn == faceUp[currentRot] && alpha >= 1 && currentIt > minIt)
            {
                transform.rotation = target;
                EnableGravity();
                StopAllCoroutines();
                yield break;
            }
            else
                yield return new WaitForSeconds(0.01f);

            if (alpha >= 1)
            {
                //set origin rotation
                Transform aux = new GameObject().transform;
                aux.Rotate(rotations[currentRot]);
                origin = aux.rotation;

                currentIt++;
                if (++currentRot == 6)
                    currentRot = 0;
                alpha = 0;

                //set target of rotation
                aux.rotation = new Quaternion();
                aux.Rotate(rotations[currentRot]);
                target = aux.rotation;
                Destroy(aux.gameObject);
            }

            if (alphaPos >= 0.99)
            {
                Vector3 aux = originPos;
                originPos = targetPos;
                targetPos = aux;
                alphaPos = 0;
            }

            transform.position = Vector3.Slerp(originPos, targetPos, alphaPos);
            transform.rotation = Quaternion.Slerp(origin, target, alpha);
            alpha += 0.15f;
            if (alphaPos < 0.15 || alphaPos > 0.85)
                alphaPos += 0.013f;
            else if (alphaPos < 0.35 || alphaPos > 0.65)
                alphaPos += 0.03f;
            else
                alphaPos += 0.05f;

        }
    }

    //moves the dice towards teh storage position
    public IEnumerator MoveToStorage()
    {
        while (true)
        {
            if (alphaPos >= 1)
            {
                transform.rotation = target;
                DisableGravity();
                StopAllCoroutines();
                Finished = true;
                yield break;
            }
            else
                yield return new WaitForSeconds(0.01f);

            transform.localPosition = Vector3.Slerp(originPos, targetPos, alphaPos);
            if (alphaPos < 0.15 || alphaPos > 0.85)
                alphaPos += 0.013f;
            else if (alphaPos < 0.35 || alphaPos > 0.65)
                alphaPos += 0.03f;
            else
                alphaPos += 0.055f;

        }
    }

}

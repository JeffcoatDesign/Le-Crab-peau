using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float rotationSpeed;
    public float jumpForce;
    public float minY;
    public GameObject hatObject;
    public Vector3 respawnPoint;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public TextMeshPro nameText;

    [PunRPC]
    public void Initialize (Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        nameText.text = PhotonNetwork.PlayerList[id - 1].NickName;

        //give first player the hat
        if (id == 1)
            GameManager.instance.GiveHat(id, true);

        if (!photonView.IsMine)
            rig.isKinematic = true;
    }

    void Update()
    {
        if (nameText != null)
        {
            nameText.transform.LookAt(Camera.main.transform.position);
            nameText.transform.Rotate(0, 180, 0);
        }

        if(PhotonNetwork.IsMasterClient)
        {
            if(curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            //track time wearing hat
            if (hatObject.activeInHierarchy)
                curHatTime += Time.deltaTime;

            //respawn on fall
            if(rig.position.y < minY)
            {
                rig.position = respawnPoint;
            }
        }
    }

    void Move ()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;
        if (Input.GetAxis("Vertical") < 0)
            RotateTo(0);
        else if (Input.GetAxis("Vertical") > 0)
            RotateTo(180);
        else if (Input.GetAxis("Horizontal") < 0)
            RotateTo(90);
        else if (Input.GetAxis("Horizontal") > 0)
            RotateTo(270);

            rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump ()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void RotateTo (int targetAngle)
    {
        int currentAngle = (int) rig.transform.eulerAngles.y;
        int angleDifference;
        currentAngle = currentAngle % 360;
        angleDifference = modulo((targetAngle - currentAngle), 360);
        Debug.Log("Angle Difference: " + angleDifference);
        if (currentAngle != targetAngle)
        {
            if (angleDifference > 180)
                rig.transform.Rotate(0, 360 - rotationSpeed, 0);
            else
                rig.transform.Rotate(0, rotationSpeed, 0);
        }
    }

    //gets the modulo of an interger and a modulus
    int modulo (int x, int m)
    {
        return (x % m + m) % m;
    }

    public void SetHat (bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if(GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {
                if(GameManager.instance.CanGetHat())
                {
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}

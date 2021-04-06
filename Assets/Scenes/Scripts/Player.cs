using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bolt;

public class Player : EntityBehaviour<IPlayerState>
{
    
    [SerializeField] float speed; //6
    [Range(0f,1f)]
    [SerializeField] float rotationLerp; //0.08
    
    [SerializeField] float jumpPower; //12
    [SerializeField] float jumpCoolTime; //0.8

    [SerializeField] Text nickText;
    [SerializeField] Transform playerCanvas;

    Rigidbody rigid;

    [SerializeField] bool isGround = true;
    bool jumpable = true;
    public Vector3 tempPosition => state.transform.Position;

    public void SetIsServer(bool isServer) => state.isServer = isServer;

    public override void Attached()
    {
        state.SetTransforms(state.transform, transform); // ��ġ ����ȭ
        rigid = GetComponent<Rigidbody>();
        state.nickname = NetworkManager.Instance.myNickName; 
        state.AddCallback("nickname", NicknameCallback);
    }

    void NicknameCallback()
    {
        nickText.text = state.nickname;
    }

    public override void SimulateController()
    {
        IPlayerCommandInput input = PlayerCommand.Create();
        input.up = Input.GetKey(KeyCode.W);
        input.left = Input.GetKey(KeyCode.A);
        input.down = Input.GetKey(KeyCode.S);
        input.right = Input.GetKey(KeyCode.D);
        input.jump = Input.GetKey(KeyCode.Space);
        entity.QueueInput(input);
    }

    //�Է¹����� �θ��� �ݹ�
    public override void ExecuteCommand(Command command, bool resetState)
    {
        PlayerCommand cmd = (PlayerCommand)command;

        if (resetState)
        {
            cmd.Result.velocity = rigid.velocity;
            cmd.Result.angularVelocity = rigid.angularVelocity;
            rigid.velocity = cmd.Result.velocity;
            rigid.angularVelocity = cmd.Result.angularVelocity;
        }
        else
        {
            Vector3 dir = Vector3.zero;

            if (cmd.Input.up)
            {
                dir += transform.forward;
                //ȸ�� ���� �ʿ�
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(transform.forward), rotationLerp);
            }
            else if (cmd.Input.down)
            {
                dir += -transform.forward;
                //transform.rotation = Quaternion.Lerp(transform.rotation,
                //    Quaternion.LookRotation(-transform.forward), rotationLerp);
            }

            if (cmd.Input.right)
            {
                dir += transform.right;
                /*
                Vector3 right = Quaternion.Euler(0, 90, 0) * transform.forward;
                dir += right;
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(right), rotationLerp);
                */
            }
            else if (cmd.Input.left)
            {
                dir += -transform.right;
                /*
                Vector3 left = Quaternion.Euler(0, -90, 0) * transform.forward;
                dir += left;
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(left), rotationLerp);
                */
            }

            Vector3 tempVelocity = dir.normalized * speed + Vector3.up * (rigid.velocity.y>=0 ? rigid.velocity.y : 1.1f * rigid.velocity.y);

            if (cmd.Input.jump && jumpable && isGround)
            {
                jumpable = false;
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                //tempVelocity += Vector3.up * jumpPower;
                //Invoke(nameof(JumpCoolTimeDelay), jumpCoolTime);
                StartCoroutine(JumpCoolTimeDelay());
            }

            rigid.velocity = tempVelocity;

            cmd.Result.velocity = rigid.velocity;
            cmd.Result.angularVelocity = rigid.angularVelocity;
            //cmd.Result.po 
        }
    }

    //void JumpCoolTimeDelay() => jumpable = true;
    IEnumerator JumpCoolTimeDelay()
    {
        yield return new WaitForSeconds(jumpCoolTime);
        jumpable = true;
    }

    void Update()
    {
        if (!entity.IsOwner) return;

        isGround = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.05f);
    }

    void LateUpdate() => playerCanvas.rotation = transform.rotation;
}

using System.Collections;
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

    bool isGround = true;
    bool jumpable = true;

    public void SetIsServer(bool isServer) => state.isServer = isServer;

    void NicknameCallback()
    {
        nickText.text = state.nickname;
    }

    // ������ Ŭ���̾�Ʈ�� ����Ǹ鼭 �����Ǵ� �Լ��� Start()�� ���� ����
    public override void Attached()
    {
        state.SetTransforms(state.transform, transform); // ��ġ ����ȭ
        rigid = GetComponent<Rigidbody>();
        state.nickname = NetworkManager.Instance.myNickName; 
        state.AddCallback("nickname", NicknameCallback);
    }

    // �̸� ������ Bolt Asset�� Command�� ���� �Է��� �����Ͽ�, ������ ���� �Է¹޵��� �ϴ� �Լ�
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

    // �Է¹����� �θ��� �ݹ�
    public override void ExecuteCommand(Command command, bool resetState)
    {
        PlayerCommand cmd = (PlayerCommand)command;

        if (resetState)
        {
            rigid.velocity = cmd.Result.velocity;
            rigid.angularVelocity = cmd.Result.angularVelocity;
            transform.position = cmd.Result.position;
            transform.rotation = Quaternion.Euler(
                new Vector3(cmd.Result.rotation.x, cmd.Result.rotation.y, cmd.Result.rotation.z));
        }
        else
        {
            Vector3 dir = Vector3.zero;

            if (cmd.Input.up)
            {
                dir += transform.forward;
                //ȸ�� ���� �ʿ�
            }
            else if (cmd.Input.down)
            {
                dir += -transform.forward;
            }

            if (cmd.Input.right)
            {
                dir += transform.right;
            }
            else if (cmd.Input.left)
            {
                dir += -transform.right;
            }

            Vector3 tempVelocity = dir.normalized * speed + Vector3.up * rigid.velocity.y;

            if (cmd.Input.jump && jumpable && isGround)
            {
                jumpable = false;
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                StartCoroutine(JumpCoolTimeDelay());
            }

            rigid.velocity = tempVelocity;

            cmd.Result.position = transform.position;
            cmd.Result.rotation = transform.eulerAngles;
            cmd.Result.velocity = rigid.velocity;
            cmd.Result.angularVelocity = rigid.angularVelocity;
        }
    }

    IEnumerator JumpCoolTimeDelay()
    {
        yield return new WaitForSeconds(jumpCoolTime);
        jumpable = true;
    }

    void Update()
    {
        if (!entity.IsOwner)
        {
            transform.position = Vector3.Lerp(transform.position, state.transform.Position, BoltNetwork.FrameDeltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, state.transform.Rotation, 1f);
        }

        isGround = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.05f);
    }

    void LateUpdate() => playerCanvas.rotation = transform.rotation;
}

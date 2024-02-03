using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] allVirtualCameras;

    [Header("Learping")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool isLearpingYDamping {get; private set;}
    public bool learpedFromPlayerFalling {get; set;}

    private Coroutine learpYPanCoroutine;
    private Coroutine panCameraCoroutine; 

    private CinemachineFramingTransposer framingTr;
    private CinemachineVirtualCamera currentCamera;

    private float normYPanAmount; 

    private Vector2 startingTrackedObjectOffset;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        for(int i = 0; i < allVirtualCameras.Length; i++)
        {
            if(allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];
                framingTr = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normYPanAmount = framingTr.m_YDamping;

        startingTrackedObjectOffset = framingTr.m_TrackedObjectOffset;
    }

    //Learp the y damping
    public void LearpYDamping(bool isPlayerFalling)
    {
        learpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLearpingYDamping = true;

        float startDampAmount = framingTr.m_YDamping;
        float endDampAmount = 0f;

        if(isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            learpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }

        float elapsedTime = 0f;
        while(elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float learpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime/ fallYPanTime));
            framingTr.m_YDamping = learpedPanAmount;

            yield return null;
        }

        isLearpingYDamping = false;
    }

    //Pan camera
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        panCameraCoroutine = StartCoroutine(PanCamera(panDistance,panTime,panDirection,panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        if(!panToStartingPos)
        {
            switch(panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.right;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.left;
                    break;
                default:
                    break;
            }

            endPos *= panDistance;

            startingPos =  startingTrackedObjectOffset;

            endPos += startingPos;
        }
        else
        {
            startingPos = framingTr.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime/panTime));
            framingTr.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }

    //Swap camera
    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight,Vector2 triggerExitDirection )
    {
        if(currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            cameraFromRight.enabled = true;
            cameraFromLeft.enabled = false;
            currentCamera = cameraFromRight;
            framingTr = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if(currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            cameraFromRight.enabled = false;
            cameraFromLeft.enabled = true;
            currentCamera = cameraFromLeft;
            framingTr = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        /*
        Up ve down için bu kodlar kullanılabilir fakat kodu iyi okuyup analiz et sonra bak çünkü bazı şeyler karışabilir ayrıca
        üstteki ifleride düzenlemen lazım

        aynı zamanda cameracontroltrigger kodundada bir şeyleri değiştirecen

        else if(currentCamera == cameraFromDown && triggerExitDirection.y > 0f)
        {
            cameraFromRight.enabled = false;
            cameraFromLeft.enabled = false;
            cameraFromDown.enabled = false;
            cameraFromUp.enabled = true;
            currentCamera = cameraFromUp;
            framingTr = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if(currentCamera == cameraFromUp && triggerExitDirection.y < 0f)
        {
            cameraFromRight.enabled = false;
            cameraFromLeft.enabled = false;
            cameraFromDown.enabled = true;
            cameraFromUp.enabled = false;
            currentCamera = cameraFromDown;
            framingTr = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        */
    }

}

using UnityEngine;

/// <summary>
/// 负责为战斗背景加载图片，并按战斗相机视野自动铺满。
/// </summary>
public sealed class BattleBackgroundFitter : MonoBehaviour
{
    private const string BattleCameraObjectName = "battleCamera";
    private const string BattleBackgroundSpriteName = "test_battle_bg";

    private SpriteRenderer m_SpriteRenderer;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyBackground();
    }

    public void ApplyBackground()
    {
        if (m_SpriteRenderer == null)
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (m_SpriteRenderer == null)
        {
            return;
        }

        Sprite backgroundSprite = ResMgr.inst.LoadSprite(BattleBackgroundSpriteName);
        if (backgroundSprite == null)
        {
            return;
        }

        m_SpriteRenderer.sprite = backgroundSprite;

        Camera battleCamera = FindBattleCamera();
        if (battleCamera == null)
        {
            return;
        }

        FitToCamera(battleCamera, backgroundSprite);
    }

    private Camera FindBattleCamera()
    {
        GameObject cameraObject = GameObject.Find(BattleCameraObjectName);
        if (cameraObject == null)
        {
            cameraObject = GameObject.Find("BattleCamera");
        }

        return cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
    }

    private void FitToCamera(Camera battleCamera, Sprite backgroundSprite)
    {
        Vector3 currentPosition = transform.position;
        Vector3 cameraPosition = battleCamera.transform.position;
        Vector3 cameraForward = battleCamera.transform.forward;

        float planeDistance = Vector3.Dot(currentPosition - cameraPosition, cameraForward);
        if (planeDistance <= 0f)
        {
            planeDistance = Mathf.Abs(currentPosition.z - cameraPosition.z);
        }

        if (planeDistance <= 0f)
        {
            return;
        }

        float visibleHeight = 2f * planeDistance * Mathf.Tan(battleCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float visibleWidth = visibleHeight * battleCamera.aspect;

        Vector2 spriteSize = backgroundSprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float scaleY = visibleHeight / spriteSize.y;
        float scaledWidth = spriteSize.x * scaleY;
        float scaleX = scaledWidth < visibleWidth ? visibleWidth / spriteSize.x : scaleY;
        transform.localScale = new Vector3(scaleX, scaleY, transform.localScale.z);

        Vector3 bottomCenterWorld = battleCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, planeDistance));
        transform.position = new Vector3(cameraPosition.x, bottomCenterWorld.y, currentPosition.z);
    }
}

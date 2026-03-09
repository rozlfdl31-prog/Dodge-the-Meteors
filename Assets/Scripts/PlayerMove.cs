using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    // 화면 가장자리에서 얼마나 띄울지(숫자 키우면 더 안쪽에서 멈춤)
    public float screenPadding = 0.5f;

    // 스프라이트가 “어느 방향을 앞”으로 보고 그려졌는지 보정 (보통 위를 보면 90)
    public float spriteForwardAngleOffset = 90f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCamera;
    [SerializeField] private GameObject thrusterEffect;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector2 raw = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) raw.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) raw.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) raw.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) raw.y += 1f;
        }

        moveInput = raw;

        if (thrusterEffect != null)
        {
            bool isMoving = moveInput.sqrMagnitude > 0f;
            thrusterEffect.SetActive(isMoving);
        }

    }

    private void FixedUpdate()
    {
        Vector2 dir = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;

        Vector2 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;

        // 카메라 화면 안으로 좌표를 강제로 “잘라내기(Clamp)”
        if (mainCamera != null && mainCamera.orthographic)
        {
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;

            next.x = Mathf.Clamp(next.x, -halfWidth + screenPadding, halfWidth - screenPadding);
            next.y = Mathf.Clamp(next.y, -halfHeight + screenPadding, halfHeight - screenPadding);
        }

        rb.MovePosition(next);

        // 움직이는 방향으로 회전
        if (dir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle + spriteForwardAngleOffset);
        }

    }
}

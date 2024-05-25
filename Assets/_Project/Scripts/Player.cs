using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement settings")] 
    [SerializeField] float _maxHorizontalSpeed = 5f;

    [Header("Jump settings")]
    [SerializeField] float _jumpVelocity = 5f;
    [SerializeField] float _jumpDuration = 0.5f;

    [Header("Sprites")] 
    [SerializeField] LayerMask _layerMask;
    [SerializeField] float _footOffset = 0.5f;
    [SerializeField] float _groundAcceleration = 10;
    [SerializeField] float _snowAcceleration = 1;

    [Header("Dimension settings")] 
    [SerializeField] bool _whiteDimension = true;
    [SerializeField] GameObject _whiteDimensionObject;
    [SerializeField] GameObject _whiteDimensionFloor;
    [SerializeField] GameObject _whiteDimensionLeftWall;
    [SerializeField] GameObject _whiteDimensionRightWall;
    [SerializeField] GameObject _whiteDimensionCeiling;
    [SerializeField] GameObject _blackDimensionObject;
    [SerializeField] GameObject _blackDimensionFloor;
    [SerializeField] GameObject _blackDimensionLeftWall;
    [SerializeField] GameObject _blackDimensionRightWall;
    [SerializeField] GameObject _blackDimensionCeiling;

    [SerializeField] GameObject _finishFlagObject;
    [SerializeField] Sprite _blackFinishFlag;
    [SerializeField] Sprite _whiteFinishFlag;
    
    public bool IsGrounded;
    public bool IsOnSnow;
    
    SpriteRenderer _spriteRenderer;
    Rigidbody2D _rigidbody;
    
    float _horizontal;
    int _jumpsRemaining;
    float _jumpEndTime;

    PlayerData _playerData = new PlayerData();
    public event Action HealthChanged;

    public int Health => _playerData.Health;

    void Awake()
    {
        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        UpdateGrounding();

        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var vertical = _rigidbody.velocity.y;

        if (Input.GetKeyDown(KeyCode.Space) && _jumpsRemaining > 0)
        {
            _jumpEndTime = Time.time + _jumpDuration;
            _jumpsRemaining--;
            vertical = _jumpVelocity;
            
            // Do something with jump audio here.
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _whiteDimension = !_whiteDimension;
            SwitchDimensions();
        }

        var desiredHorizontal = horizontalInput * _maxHorizontalSpeed;
        var acceleration = IsOnSnow ? _snowAcceleration : _groundAcceleration;
        _horizontal = Mathf.Lerp(_horizontal, desiredHorizontal, Time.deltaTime * acceleration);
        _rigidbody.velocity = new Vector2(_horizontal, vertical);
        
        UpdateSprite();
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();

        Vector2 origin = new Vector2(transform.position.x, transform.position.y - capsuleCollider.bounds.extents.y);
        Gizmos.DrawLine(origin, origin + Vector2.down * 0.1f);

        // Draw Left Foot
        origin = new Vector2(transform.position.x - _footOffset, transform.position.y - capsuleCollider.bounds.extents.y);
        Gizmos.DrawLine(origin, origin + Vector2.down * 0.1f);

        // Draw Right Foot
        origin = new Vector2(transform.position.x + _footOffset, transform.position.y - capsuleCollider.bounds.extents.y);
        Gizmos.DrawLine(origin, origin + Vector2.down * 0.1f);
    }
    
    void UpdateGrounding()
    {
        IsGrounded = false;
        IsOnSnow = false;
        // Check center
        Vector2 origin = new Vector2(transform.position.x, transform.position.y - _spriteRenderer.bounds.extents.y);

        var hit = Physics2D.Raycast(origin, Vector2.down, 0.1f, _layerMask);
        if (hit.collider)
        {
            IsGrounded = true;
            IsOnSnow = hit.collider.CompareTag("Snow");
        }

        // Check left
        origin = new Vector2(transform.position.x - _footOffset, transform.position.y - _spriteRenderer.bounds.extents.y);

        hit = Physics2D.Raycast(origin, Vector2.down, 0.1f, _layerMask);
        if (hit.collider)
        {
            IsGrounded = true;
            IsOnSnow = hit.collider.CompareTag("Snow");
        }

        // Check right
        origin = new Vector2(transform.position.x + _footOffset, transform.position.y - _spriteRenderer.bounds.extents.y);

        hit = Physics2D.Raycast(origin, Vector2.down, 0.1f, _layerMask);
        if (hit.collider)
        {
            IsGrounded = true;
            IsOnSnow = hit.collider.CompareTag("Snow");
        }

        if (IsGrounded && GetComponent<Rigidbody2D>().velocity.y  <= 0)
            _jumpsRemaining = 2;
    }
    
    void UpdateSprite()
    {
        //_animator.SetBool("IsGrounded", IsGrounded);
        //_animator.SetFloat("HorizontalSpeed", Math.Abs(_horizontal));

        if (_horizontal > 0)
            _spriteRenderer.flipX = false;
        else if (_horizontal < 0)
            _spriteRenderer.flipX = true;
    }

    void SwitchDimensions()
    {
        if (_whiteDimension)
        {
            _whiteDimensionObject.gameObject.SetActive(true);
            _blackDimensionObject.gameObject.SetActive(false);
            _whiteDimensionFloor.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            _whiteDimensionLeftWall.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            _whiteDimensionRightWall.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            _whiteDimensionCeiling.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            Camera.main.backgroundColor = Color.white;
            _finishFlagObject.gameObject.GetComponent<SpriteRenderer>().sprite = _blackFinishFlag;
        }
        else
        {
            _whiteDimensionObject.gameObject.SetActive(false);
            _blackDimensionObject.gameObject.SetActive(true);
            _whiteDimensionFloor.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            _whiteDimensionLeftWall.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            _whiteDimensionRightWall.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            _whiteDimensionCeiling.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            Camera.main.backgroundColor = Color.black;
            _finishFlagObject.gameObject.GetComponent<SpriteRenderer>().sprite = _whiteFinishFlag;
        }
    }
    
}
